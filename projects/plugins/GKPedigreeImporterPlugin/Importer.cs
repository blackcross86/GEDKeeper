﻿/*
 *  "GEDKeeper", the personal genealogical database editor.
 *  Copyright (C) 2009-2023 by Sergey V. Zhdanovskih.
 *
 *  This file is part of "GEDKeeper".
 *
 *  This program is free software: you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation, either version 3 of the License, or
 *  (at your option) any later version.
 *
 *  This program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public License
 *  along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */

#if MONO
#define NO_DEPEND
#endif

#if CI_MODE
#define NO_DEPEND
#endif

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using BSLib;
using GDModel;
using GDModel.Providers.GEDCOM;
using GKCore;
using GKCore.Design;
using GKCore.Import;
using GKCore.Interfaces;
using GKCore.Types;

namespace GKPedigreeImporterPlugin
{
#if !NO_DEPEND
    using MSOExcel = Microsoft.Office.Interop.Excel;
    using MSOWord = Microsoft.Office.Interop.Word;
#endif

    [Serializable]
    public class ImporterException : Exception
    {
        public ImporterException()
        {
        }

        public ImporterException(string message) : base(message)
        {
        }
    }

    public enum SourceType
    {
        stText,
        stTable
    }

    public enum PersonNumbersType
    {
        pnUndefined,
        pnDAboville,
        pnKonovalov
    }

    public enum CellType
    {
        ct
    }

    public enum NameFormat
    {
        nfIOF,
        nfFIO
    }

    public enum GenerationFormat
    {
        gfRome,
        gfGenWord
    }

    public enum RawLineType
    {
        rltComment,
        rltPerson,
        rltRomeGeneration,
        rltEOF
    }

    public sealed class RawLine
    {
        public int SourceNum;
        public RawLineType Type;
        public PersonNumbersType NumbersType;

        public RawLine(int sourceNum)
        {
            SourceNum = sourceNum;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public class Importer : BaseObject
    {
        private const bool DEBUG_EXCEL = false;
        private const bool DEBUG_WORD = false;

        private readonly IBaseWindow fBase;
        private readonly ILangMan fLangMan;
        private readonly IList fLog;
        private readonly StringList fRawContents;
        private readonly GDMTree fTree;
        private readonly IView fView;

        private Dictionary<string, GDMIndividualRecord> fPersonsList;
        private string fFileName;

        // settings
        public PersonNumbersType NumbersType;
        public PersonNumbersType CanNumbersType;
        public char PersonLineSeparator;
        public SourceType SourceType;
        public NameFormat NameFormat;
        public GenerationFormat GenerationFormat;
        public bool SurnamesNormalize;
        public DateFormat DateFormat;
        public char DateSeparator;

        public bool SpecialFormat_1;

        public StringList RawContents
        {
            get { return fRawContents; }
        }

        public Importer(IView view, IBaseWindow baseWin, ILangMan langMan, IList aLog)
        {
            fView = view;
            fBase = baseWin;
            fTree = baseWin.Context.Tree;
            fLog = aLog;
            fLangMan = langMan;

            NumbersType = PersonNumbersType.pnKonovalov;
            CanNumbersType = PersonNumbersType.pnUndefined;
            PersonLineSeparator = (char)0;
            SurnamesNormalize = false;

            fPersonsList = new Dictionary<string, GDMIndividualRecord>();
            fRawContents = new StringList();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) {
                if (fRawContents != null) fRawContents.Dispose();
                fPersonsList = null;
            }
            base.Dispose(disposing);
        }

        private GDMFamilyRecord GetFamilyByNum(GDMIndividualRecord parent, int marrNum)
        {
            // it's source of ERRORS! but without this - bad! (AddSpouse() not linking parent to family)
            GDMSex sex = parent.Sex;
            if (sex == GDMSex.svUnknown || sex == GDMSex.svIntersex) {
                parent.Sex = GDMSex.svMale;
            }

            while (parent.SpouseToFamilyLinks.Count < marrNum) {
                GDMFamilyRecord fam = fTree.CreateFamily();
                fam.AddSpouse(parent);
            }

            GDMFamilyRecord family = fTree.GetPtrValue(parent.SpouseToFamilyLinks[marrNum - 1]);
            return family;
        }

        private void AddChild(GDMIndividualRecord parent, int marrNum, GDMIndividualRecord child)
        {
            if (marrNum <= 0) {
                marrNum = 1;
            }

            GDMFamilyRecord family = GetFamilyByNum(parent, marrNum);
            if (family != null) {
                family.AddChild(child);
            } else {
                // ???
            }
        }

        private static string RemoveDot(string str)
        {
            if (!string.IsNullOrEmpty(str)) {
                if (str[str.Length - 1] == '.') {
                    str = str.Substring(0, str.Length - 1);
                }
                str = str.Trim();
            }
            return str;
        }

        private static string RemoveCommaDot(string str)
        {
            if (!string.IsNullOrEmpty(str)) {
                char last = str[str.Length - 1];
                if (last == ',' || last == '.') {
                    str = str.Substring(0, str.Length - 1);
                }
                str = str.Trim();
            }
            return str;
        }

        internal sealed class PersonNameRet
        {
            public string Name;
            public string Patr;
            public string Surname;
            public string BirthDate;
            public string DeathDate;

            public PersonNameRet(string name, string patr, string surname, string bd, string dd)
            {
                Name = name;
                Patr = patr;
                Surname = surname;
                BirthDate = bd;
                DeathDate = dd;
            }
        }

        private PersonNameRet DefinePersonName(string str)
        {
            string f_name = "";
            string f_pat = "";
            string f_fam = "";
            string bd = "";
            string dd = "";

            string tmp = str;

            string dates = "";
            if (SpecialFormat_1) {
                int ob_pos = tmp.IndexOf("(*");
                if (ob_pos >= 0) {
                    int cb_pos = tmp.IndexOf(")", ob_pos);
                    if (cb_pos > ob_pos) {
                        dates = tmp.Substring(ob_pos + 1, cb_pos - ob_pos - 1).Trim();
                        tmp = tmp.Remove(ob_pos, dates.Length + 2);
                    }
                }
            }

            // if not Special or SpecialNotFound, then classic
            if (string.IsNullOrEmpty(dates)) {
                int bd_pos = tmp.IndexOf(ImportUtils.STD_BIRTH_SIGN);
                int dd_pos = tmp.IndexOf(ImportUtils.STD_DEATH_SIGN);

                int datesPos = -1;
                if (bd_pos >= 0 && (dd_pos < 0 || dd_pos > bd_pos)) {
                    datesPos = bd_pos;
                } else {
                    datesPos = dd_pos;
                }

                if (datesPos >= 0) {
                    dates = tmp.Substring(datesPos, tmp.Length - datesPos);
                    tmp = tmp.Remove(datesPos, dates.Length).Trim(); // can be blanks at end
                }
            }

            // parse dates line
            if (!string.IsNullOrEmpty(dates)) {
                int b_pos = dates.IndexOf(ImportUtils.STD_BIRTH_SIGN);
                int d_pos = dates.IndexOf(ImportUtils.STD_DEATH_SIGN);

                if (d_pos >= 0 && d_pos > b_pos) {
                    dd = dates.Substring(d_pos + 1, dates.Length - d_pos - 1);
                    dates = dates.Remove(d_pos, dd.Length + 1);
                    dates = dates.Trim();
                }

                if (b_pos >= 0) {
                    bd = dates.Substring(b_pos + 1, dates.Length - b_pos - 1);
                    dates = dates.Remove(b_pos, bd.Length + 1);
                    dates = dates.Trim();
                }

                bd = RemoveDot(bd);
                dd = RemoveDot(dd);
            }

            tmp = RemoveCommaDot(tmp); // &Trim()

            string[] tokens = tmp.Split(new char[] { ' ' }, 3);

            switch (NameFormat) {
                case NameFormat.nfIOF:
                    if (tokens.Length > 0) f_name = RemoveDot(tokens[0]);
                    if (tokens.Length > 1) f_pat = RemoveDot(tokens[1]);
                    if (tokens.Length > 2) f_fam = RemoveDot(tokens[2]);
                    break;

                case NameFormat.nfFIO:
                    if (tokens.Length > 0) f_fam = RemoveDot(tokens[0]);
                    if (tokens.Length > 1) f_name = RemoveDot(tokens[1]);
                    if (tokens.Length > 2) f_pat = RemoveDot(tokens[2]);
                    break;
            }

            if (SurnamesNormalize) {
                f_fam = StringHelper.UniformName(f_fam);
            }

            return new PersonNameRet(f_name, f_pat, f_fam, bd, dd);
        }

        private string IsPersonLine(string str)
        {
            switch (NumbersType) {
                case PersonNumbersType.pnDAboville:
                    return ImportUtils.IsPersonLine_DAboville(str);

                case PersonNumbersType.pnKonovalov:
                    return ImportUtils.IsPersonLine_Konovalov(str);

                default:
                    return null;
            }
        }

        private ImportUtils.PersonLineRet ParsePersonLine(string str)
        {
            switch (NumbersType) {
                case PersonNumbersType.pnDAboville:
                    return ImportUtils.ParsePersonLine_DAboville(str);

                case PersonNumbersType.pnKonovalov:
                    return ImportUtils.ParsePersonLine_Konovalov(str);

                default:
                    return null;
            }
        }

        private void SetEvent(GDMRecordWithEvents record, string evName, string date)
        {
            int[] val = new int[3];
            GDMCustomEvent evt = fBase.Context.CreateEventEx(record, evName, "", "");
            try {
                string prefix = "";
                if (date.IndexOf("п.") == 0) {
                    prefix = "AFT ";
                    date = date.Remove(0, 2);
                } else if (date.IndexOf("после") == 0) {
                    prefix = "AFT ";
                    date = date.Remove(0, 5);
                } else if (date.IndexOf("до") == 0) {
                    prefix = "BEF ";
                    date = date.Remove(0, 2);
                } else if (date.IndexOf("ок.") == 0) {
                    prefix = "ABT ";
                    date = date.Remove(0, 3);
                } else if (date.IndexOf("около") == 0) {
                    prefix = "ABT ";
                    date = date.Remove(0, 5);
                }

                date = date.Trim();

                string tmp = "";
                string[] toks = date.Split('.');
                if (toks.Length > 3) {
                    throw new ImporterException("date failed");
                }
                string ym = "";

                for (int i = 0; i < toks.Length; i++) {
                    tmp = toks[i];

                    int x = tmp.IndexOf("/");
                    if (x >= 0) {
                        ym = tmp.Substring(x + 1, tmp.Length - x - 1);
                        tmp = tmp.Remove(x, ym.Length + 1);
                    }

                    val[i] = int.Parse(tmp);
                }

                if (toks.Length != 1) {
                    if (toks.Length != 2) {
                        if (toks.Length == 3) {
                            tmp = val[0].ToString() + " " + GEDCOMConsts.GEDCOMMonthArray[val[1] - 1] + " " + val[2].ToString();
                        }
                    } else {
                        tmp = GEDCOMConsts.GEDCOMMonthArray[val[0] - 1] + " " + val[1].ToString();
                    }
                } else {
                    tmp = val[0].ToString();
                }

                tmp = prefix + tmp;
                if (ym != "") {
                    tmp = tmp + "/" + ym;
                }

                evt.Date.ParseString(tmp);
            } catch (Exception) {
                fLog.Add(">>>> " + fLangMan.LS(ILS.LSID_ParseError_DateInvalid) + " \"" + date + "\"");
            }
        }

        private GDMIndividualRecord DefinePerson(string str, GDMSex proposeSex)
        {
            var persName = DefinePersonName(str);

            GDMIndividualRecord result = fBase.Context.CreatePersonEx(persName.Name, persName.Patr, persName.Surname, proposeSex, false);

            if (proposeSex == GDMSex.svUnknown || proposeSex == GDMSex.svIntersex) {
                fBase.Context.CheckPersonSex(fView, result);
            }

            if (persName.BirthDate != "") SetEvent(result, GEDCOMTagName.BIRT, persName.BirthDate);
            if (persName.DeathDate != "") SetEvent(result, GEDCOMTagName.DEAT, persName.DeathDate);

            return result;
        }

        private GDMIndividualRecord ParsePerson(GDMLines buffer, string str, ref int selfId)
        {
            try {
                selfId = -1;
                int marrNum = -1;
                int pid_end = 0;

                var plRet = ParsePersonLine(str);
                // extData - (в/б)

                if (plRet == null) {
                    return null;
                }

                pid_end = plRet.Pos;

                if (fPersonsList.ContainsKey(plRet.PersId)) {
                    fLog.Add(">>>> " + fLangMan.LS(ILS.LSID_ParseError_NumDuplicate) + " \"" + plRet.PersId + "\".");
                    return null;
                }

                if (NumbersType == PersonNumbersType.pnKonovalov) {
                    selfId = int.Parse(plRet.PersId);
                    int.TryParse(plRet.MarNum, out marrNum);
                }

                str = str.Substring(pid_end).Trim();

                GDMSex proposeSex = GetProposeSex(buffer);

                GDMIndividualRecord result = DefinePerson(str, proposeSex);

                fPersonsList.Add(plRet.PersId, result);

                if (!string.IsNullOrEmpty(plRet.ParentId)) {
                    GDMIndividualRecord parent;
                    if (fPersonsList.TryGetValue(plRet.ParentId, out parent)) {
                        AddChild(parent, marrNum, result);
                    } else {
                        fLog.Add(">>>> " + fLangMan.LS(ILS.LSID_ParseError_AncNotFound) + " \"" + plRet.ParentId + "\".");
                    }
                }

                return result;
            } catch (Exception ex) {
                Logger.WriteError("Importer.ParsePerson()", ex);
                throw;
            }
        }

        private GDMSex GetProposeSex(GDMLines buffer)
        {
            GDMSex result = GDMSex.svUnknown;
            if (buffer == null) return result;

            try {
                int num = buffer.Count;
                for (int i = 0; i < num; i++) {
                    string line = buffer[i];
                    if (line.Length <= 2) continue;

                    char c1 = line[0];
                    char c2 = line[1];
                    if ((c1 == 'М' || c1 == 'Ж') && ((c2 == ' ') || (c2 >= '1' && c2 <= '9'))) {
                        // define sex (if spouse is male, then result = female, else result = male)
                        GDMSex res = (c1 == 'М') ? GDMSex.svFemale : GDMSex.svMale;

                        if (result == GDMSex.svUnknown) {
                            result = res;
                        } else {
                            if (result != res) {
                                fLog.Add(">>>> " + fLangMan.LS(ILS.LSID_SpousesInfoConflict));
                                return GDMSex.svUnknown;
                            } else {
                                // matched, checked
                            }
                        }
                    }
                }
            } catch (Exception ex) {
                Logger.WriteError("Importer.GetProposeSex()", ex);
            }

            return result;
        }

        private void CheckSpouses(GDMLines buffer, GDMIndividualRecord curPerson)
        {
            int num2 = buffer.Count;
            for (int i = 0; i < num2; i++) {
                string line = buffer[i];
                if (string.IsNullOrEmpty(line)) continue;

                try {
                    var slRet = ImportUtils.ParseSpouseLine(line);
                    if (slRet != null) {
                        // define sex
                        string spSex = slRet.Spouse;
                        GDMSex sx = (spSex[0] == 'М') ? GDMSex.svMale : GDMSex.svFemale;

                        // extract name
                        line = line.Substring(slRet.Pos).Trim();

                        if (!string.IsNullOrEmpty(line)) {
                            GDMIndividualRecord spouse = DefinePerson(line, sx);
                            GDMFamilyRecord family = GetFamilyByNum(curPerson, slRet.MarrNum);

                            if (spouse == null || family == null) {
                                // TODO: error to log, reporting causes
                            } else {
                                family.AddSpouse(spouse);

                                // extract marriage date
                                if (!string.IsNullOrEmpty(slRet.ExtData)) {
                                    string marrDate = slRet.ExtData.Substring(1, slRet.ExtData.Length - 2).Trim();

                                    if (marrDate != "")
                                        SetEvent(family, GEDCOMTagName.MARR, marrDate);
                                }
                            }
                        }
                    }
                } catch (Exception ex) {
                    Logger.WriteError("Importer.CheckSpouses()", ex);
                }
            }
        }

        private void CheckBuffer(GDMLines buffer, GDMIndividualRecord curPerson)
        {
            if (buffer.IsEmpty()) return;

            if (curPerson != null) {
                CheckSpouses(buffer, curPerson);
            }

            GDMNoteRecord noteRec = fTree.CreateNote();
            noteRec.Lines.Assign(buffer);
            if (curPerson != null) curPerson.AddNote(noteRec);

            buffer.Clear();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="buffer"></param>
        /// <returns>prevId, identifier of person</returns>
        private int ParseBuffer(GDMLines buffer)
        {
            int prevId = 0;

            try {
                if (buffer.IsEmpty()) {
                    return prevId;
                }

                string s = buffer[0];
                string personId = IsPersonLine(s);
                if (!string.IsNullOrEmpty(personId)) {
                    fLog.Add("> " + fLangMan.LS(ILS.LSID_PersonParsed) + " \"" + personId + "\"");

                    int selfId = 0;
                    GDMIndividualRecord curPerson = ParsePerson(buffer, s, ref selfId);

                    if (NumbersType == PersonNumbersType.pnKonovalov && selfId - prevId > 1) {
                        fLog.Add(">>>> " + fLangMan.LS(ILS.LSID_ParseError_LineSeq));
                    }

                    prevId = selfId;

                    CheckBuffer(buffer, curPerson);
                }
            } catch (Exception ex) {
                Logger.WriteError("Importer.ParseBuffer()", ex);
                throw;
            }

            return prevId;
        }

        private bool IsGenerationLine(string str)
        {
            switch (GenerationFormat) {
                case GenerationFormat.gfRome:
                    return ImportUtils.IsRomeLine(str);

                case GenerationFormat.gfGenWord:
                    return str.StartsWith("Поколение ", StringComparison.InvariantCultureIgnoreCase);

                default:
                    return false;
            }
        }

        private static string PrepareLine(string line)
        {
            string result = line.Replace('–', '-');
            result = result.Replace('', '+'); // some formats of the death date prefix

            return result.Trim();
        }

        #region Integral loading

        private bool AnalyseRaw(IProgressController progress)
        {
            if (SourceType == SourceType.stTable) {
                return false;
            }

            try {
                try {
                    int[] numberStats = new int[3];

                    int num = fRawContents.Count;
                    progress.Begin(fLangMan.LS(ILS.LSID_Analyzing), num);

                    for (int i = 0; i < num; i++) {
                        string txt = fRawContents[i].Trim();
                        RawLine rawLine = (RawLine)fRawContents.GetObject(i);

                        if (!string.IsNullOrEmpty(txt)) {
                            if (IsGenerationLine(txt)) {
                                rawLine.Type = RawLineType.rltRomeGeneration;
                            } else {
                                PersonNumbersType numbType = PersonNumbersType.pnUndefined;

                                if (!string.IsNullOrEmpty(ImportUtils.IsPersonLine_DAboville(txt))) {
                                    rawLine.Type = RawLineType.rltPerson;
                                    numbType = PersonNumbersType.pnDAboville;
                                    numberStats[1]++;
                                } else if (!string.IsNullOrEmpty(ImportUtils.IsPersonLine_Konovalov(txt))) {
                                    rawLine.Type = RawLineType.rltPerson;
                                    numbType = PersonNumbersType.pnKonovalov;
                                    numberStats[2]++;
                                }

                                rawLine.NumbersType = numbType;
                            }
                        } else {
                            rawLine.Type = RawLineType.rltEOF;
                        }

                        progress.StepTo(i + 1);
                    }

                    if (numberStats[1] > numberStats[2]) {
                        CanNumbersType = PersonNumbersType.pnDAboville;
                    } else {
                        CanNumbersType = PersonNumbersType.pnKonovalov;
                    }

                    return true;
                } finally {
                    progress.End();
                }
            } catch (Exception ex) {
                Logger.WriteError("Importer.AnalyseRaw()", ex);
                return false;
            }
        }

        public bool ImportContent()
        {
            AppHost.Instance.ExecuteWork((controller) => {
                AnalyseRaw(controller);
            });

            bool result = false;
            switch (SourceType) {
                case SourceType.stText:
                    result = ImportTextContent();
                    break;

                case SourceType.stTable:
#if !NO_DEPEND
                    AppHost.Instance.ExecuteWork((controller) => {
                        result = ImportTableContent(controller);
                    });
#endif
                    break;
            }
            return result;
        }

        private bool ImportTextContent()
        {
            try {
                fLog.Clear();

                GDMLines buffer = new GDMLines();
                int prev_id = 0;

                int num = fRawContents.Count;
                for (int i = 0; i < num; i++) {
                    string line = PrepareLine(fRawContents[i]);
                    RawLine rawLine = (RawLine)fRawContents.GetObject(i);

                    switch (rawLine.Type) {
                        case RawLineType.rltComment:
                            buffer.Add(line);
                            break;

                        case RawLineType.rltPerson:
                        case RawLineType.rltRomeGeneration:
                        case RawLineType.rltEOF: {
                                prev_id = ParseBuffer(buffer);
                                buffer.Clear();

                                switch (rawLine.Type) {
                                    case RawLineType.rltPerson:
                                        buffer.Add(line);
                                        break;
                                    case RawLineType.rltRomeGeneration:
                                        fLog.Add("> " + fLangMan.LS(ILS.LSID_Generation) + " \"" + line + "\"");
                                        break;
                                    case RawLineType.rltEOF:
                                        fLog.Add("> EOF.");
                                        break;
                                }
                            }
                            break;
                    }
                }

                return true;
            } catch (Exception ex) {
                Logger.WriteError("Importer.ImportTextContent()", ex);
                throw;
            }
        }

        private static string GetCell(object[,] values, int row, int col)
        {
            object obj = values[row, col];
            return (obj == null) ? "" : obj.ToString();
        }

#if !NO_DEPEND
        private bool ImportTableContent(IProgressController progress)
        {
            try {
                fLog.Clear();

                MSOExcel.Application excel;
                try {
                    excel = new MSOExcel.Application();
                } catch (Exception) {
                    return false;
                }

                excel.Visible = DEBUG_EXCEL;
                excel.DisplayAlerts = false;
                excel.WindowState = MSOExcel.XlWindowState.xlMaximized;
                excel.Workbooks.Open(fFileName);
                MSOExcel.Worksheet sheet = excel.Worksheets[1] as MSOExcel.Worksheet;
                //sheet.Activate();

                GDMLines buffer = new GDMLines();
                try {
                    int rowsCount = sheet.UsedRange.Rows.Count;
                    //int colsCount = sheet.UsedRange.Columns.Count;

                    progress.Begin(fLangMan.LS(ILS.LSID_Loading), rowsCount);

                    MSOExcel.Range excelRange = sheet.UsedRange;
                    object[,] valueArray = (object[,])excelRange.get_Value(MSOExcel.XlRangeValueDataType.xlRangeValueDefault);

                    int prevId = 0;

                    for (int row = 1; row <= rowsCount; row++) {
                        string c1 = GetCell(valueArray, row, 1).Trim(); // position number
                        string c2 = GetCell(valueArray, row, 2).Trim(); // ancestor number
                        string c3 = GetCell(valueArray, row, 3).Trim(); // name, maybe start with the number of marriage
                        string c4 = GetCell(valueArray, row, 4).Trim(); // birth date
                        string c5 = GetCell(valueArray, row, 5).Trim(); // death date
                        string c6 = GetCell(valueArray, row, 6).Trim(); // birth or residence place

                        string s123 = c1 + c2;
                        if (s123 != "" && !string.IsNullOrEmpty(c3) && c3[0] != '/') {
                            s123 += ". " + c3;
                        } else {
                            s123 += c3;
                        }

                        if (s123 == "") {
                            continue;
                        }

                        string line, p_id = "";
                        RawLineType lineType = RawLineType.rltComment;

                        if (IsGenerationLine(s123)) {
                            line = s123;
                            lineType = RawLineType.rltRomeGeneration;
                        } else {
                            line = s123 + " " + c4 + " " + c5;
                            if (c6 != "") {
                                line = line + ". " + c6 + ".";
                            }

                            line = line.Trim();

                            p_id = IsPersonLine(line);
                            if (!string.IsNullOrEmpty(p_id)) {
                                lineType = RawLineType.rltPerson;
                            }
                        }

                        switch (lineType) {
                            case RawLineType.rltComment:
                                buffer.Add(line);
                                break;

                            case RawLineType.rltPerson:
                            case RawLineType.rltRomeGeneration:
                            case RawLineType.rltEOF: {
                                    prevId = ParseBuffer(buffer);
                                    buffer.Clear();

                                    switch (lineType) {
                                        case RawLineType.rltPerson:
                                            buffer.Add(line);
                                            break;
                                        case RawLineType.rltRomeGeneration:
                                            fLog.Add("> " + fLangMan.LS(ILS.LSID_Generation) + " \"" + line + "\"");
                                            break;
                                        case RawLineType.rltEOF:
                                            fLog.Add("> EOF.");
                                            break;
                                    }
                                }
                                break;
                        }

                        progress.StepTo(row);
                    }

                    // hack: processing last items before end
                    prevId = ParseBuffer(buffer);

                    return true;
                } finally {
                    progress.End();

                    buffer.Clear();
                    buffer = null;

                    excel.Quit();
                    excel = null;
                }
            } catch (Exception ex) {
                fLog.Add(">>>> " + fLangMan.LS(ILS.LSID_DataLoadError));
                Logger.WriteError("Importer.ImportTableContent()", ex);
                return false;
            }
        }
#endif

        private bool LoadRawText(IProgressController progress)
        {
            SourceType = SourceType.stText;

            try {
                using (Stream fs = new FileStream(fFileName, FileMode.Open, FileAccess.Read))
                using (StreamReader strd = GKUtils.GetDetectedStreamReader(fs)) {
                    try {
                        progress.Begin(fLangMan.LS(ILS.LSID_Loading), (int)strd.BaseStream.Length);

                        int lineNum = 0;
                        while (strd.Peek() != -1) {
                            string txt = strd.ReadLine().Trim();

                            if (!string.IsNullOrEmpty(txt)) {
                                fRawContents.AddObject(txt, new RawLine(lineNum));
                            }

                            progress.StepTo((int)strd.BaseStream.Position);
                            lineNum++;
                        }
                        fRawContents.AddObject("", new RawLine(lineNum));
                    } finally {
                        progress.End();
                    }
                }

                return AnalyseRaw(progress);
            } catch (Exception ex) {
                fLog.Add(">>>> " + fLangMan.LS(ILS.LSID_DataLoadError));
                Logger.WriteError("Importer.LoadRawText()", ex);
                return false;
            }
        }

#if !NO_DEPEND
        private bool LoadRawWord(IProgressController progress)
        {
            SourceType = SourceType.stText;

            try {
                MSOWord.Application wordApp;
                try {
                    wordApp = new MSOWord.Application();
                } catch {
                    return false;
                }

                try {
                    wordApp.Visible = DEBUG_WORD;
                    wordApp.WindowState = MSOWord.WdWindowState.wdWindowStateMaximize;

                    MSOWord.Document doc = wordApp.Documents.Open(fFileName);

                    progress.Begin(fLangMan.LS(ILS.LSID_Loading), doc.Paragraphs.Count);

                    int lineNum = 0;
                    for (int i = 0; i < doc.Paragraphs.Count; i++) {
                        string txt = doc.Paragraphs[i + 1].Range.Text;
                        txt = txt.Trim();

                        if (!string.IsNullOrEmpty(txt)) {
                            fRawContents.AddObject(txt, new RawLine(lineNum));
                        }

                        progress.StepTo(i + 1);
                        lineNum++;
                    }
                    fRawContents.AddObject("", new RawLine(lineNum));

                    return AnalyseRaw(progress);
                } finally {
                    progress.End();

                    object saveOptionsObject = MSOWord.WdSaveOptions.wdDoNotSaveChanges;
                    wordApp.Quit(ref saveOptionsObject);
                    wordApp = null;
                }
            } catch (Exception ex) {
                fLog.Add(">>>> " + fLangMan.LS(ILS.LSID_DataLoadError));
                Logger.WriteError("Importer.LoadRawWord()", ex);
                return false;
            }
        }
#endif

        private bool LoadRawExcel()
        {
            SourceType = SourceType.stTable;

            bool result = false;
            AppHost.Instance.ExecuteWork((controller) => {
                result = AnalyseRaw(controller);
            });
            return result;
        }

        public bool LoadRawData(string fileName)
        {
            fRawContents.Clear();

            fFileName = fileName;
            string ext = FileHelper.GetFileExtension(fileName);

            bool result = false;
            if (ext == ".txt") {
                AppHost.Instance.ExecuteWork((controller) => {
                    result = LoadRawText(controller);
                });
            } else if (ext == ".doc") {
#if !NO_DEPEND
                AppHost.Instance.ExecuteWork((controller) => {
                    result = LoadRawWord(controller);
                });
#endif
            } else if (ext == ".xls") {
#if !NO_DEPEND
                return LoadRawExcel();
#endif
            } else {
                throw new ImporterException(fLangMan.LS(ILS.LSID_FormatUnsupported));
            }
            return result;
        }

        #endregion
    }
}
