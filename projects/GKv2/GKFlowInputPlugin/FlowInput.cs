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

using System;
using System.Windows.Forms;
using BSLib;
using GDModel;
using GDModel.Providers.GEDCOM;
using GKCore.Interfaces;
using GKCore.Types;

namespace GKFlowInputPlugin
{
    [Serializable]
    public class PersonScanException : Exception
    {
        public PersonScanException()
        {
        }

        public PersonScanException(string message) : base(message)
        {
        }
    }


    internal enum PersonLink
    {
        plNone,
        plPerson,
        plFather,
        plMother,
        plGodparent,
        plSpouse,
        plChild,

        plLast = plChild
    }


    internal class FlowInput
    {
        public static readonly FLS[] PersonLinks = new FLS[] {
            FLS.LSID_RK_Unk,
            FLS.LSID_PLPerson,
            FLS.LSID_Father,
            FLS.LSID_Mother,
            FLS.LSID_PLGodparent,
            FLS.LSID_Spouse,
            FLS.LSID_Child
        };

        private readonly IPlugin fPlugin;
        private readonly ILangMan fLangMan;
        private readonly IBaseWindow fBase;


        public FlowInput(IPlugin plugin, IBaseWindow baseWin)
        {
            fPlugin = plugin;
            fLangMan = plugin.LangMan;
            fBase = baseWin;
        }

        public void ShowError(string msg)
        {
            string title = fLangMan.LS(FLS.LSID_PluginTitle);
            MessageBox.Show(msg, title, MessageBoxButtons.OK, MessageBoxIcon.Hand);
        }

        public PersonLink GetLinkByName(string aName)
        {
            PersonLink res = PersonLink.plNone;

            for (PersonLink pl = PersonLink.plPerson; pl <= PersonLink.plLast; pl++) {
                if (fLangMan.LS(FlowInput.PersonLinks[(int)pl]) == aName) {
                    res = pl;
                    break;
                }
            }

            return res;
        }

        public void ParseSimple(string fullName, GDMSex sex,
            bool hasBirth, string birthDate, string birthPlace,
            bool hasDeath, string deathDate, string deathPlace,
            string note)
        {
            string tmp = fullName.ToLower();
            string[] tokens = tmp.Split(' ');
            if (tokens.Length < 3) {
                ShowError(fLangMan.LS(FLS.LSID_NameInvalid));
                return;
            }

            string fam = StringHelper.UniformName(tokens[0]);
            string nam = StringHelper.UniformName(tokens[1]);
            string pat = StringHelper.UniformName(tokens[2]);

            GDMIndividualRecord iRec = fBase.Context.CreatePersonEx(nam, pat, fam, sex, false);
            if (hasBirth) {
                fBase.Context.CreateEventEx(iRec, GEDCOMTagName.BIRT, GDMDate.CreateByFormattedStr(birthDate, true), birthPlace);
            }

            if (hasDeath) {
                fBase.Context.CreateEventEx(iRec, GEDCOMTagName.DEAT, GDMDate.CreateByFormattedStr(deathDate, true), deathPlace);
            }

            if (!string.IsNullOrEmpty(note)) {
                GDMNoteRecord noteRec = fBase.Context.Tree.CreateNote();
                noteRec.SetNoteText(note);
                iRec.AddNote(noteRec);
            }

            fBase.NotifyRecord(iRec, RecordAction.raAdd);
        }

        public GDMSourceRecord InitializeSource(string srcName)
        {
            GDMSourceRecord srcRec = null;

            if (!string.IsNullOrEmpty(srcName)) {
                srcRec = fBase.Context.FindSource(srcName);
                if (srcRec == null) {
                    srcRec = fBase.Context.Tree.CreateSource();
                    srcRec.ShortTitle = srcName;
                }
            }

            return srcRec;
        }

        public void ParseSource(GDMSourceRecord srcRec, int srcYear, string srcPage, string place,
            ref GDMIndividualRecord iMain, string lnk, string nm, string pt, string fm, string age, string comment,
            int eventType, string eventDate)
        {
            if (!string.IsNullOrEmpty(lnk)) {
                PersonLink link = GetLinkByName(lnk);
                if (link == PersonLink.plNone) return;

                GDMSex sx = fBase.Context.DefineSex(fBase, nm, pt);
                GDMIndividualRecord iRec = fBase.Context.CreatePersonEx(nm, pt, fm, sx, false);

                if (!string.IsNullOrEmpty(age) && ConvertHelper.IsDigits(age)) {
                    int birthYear = srcYear - int.Parse(age);
                    fBase.Context.CreateEventEx(iRec, GEDCOMTagName.BIRT, "ABT " + birthYear.ToString(), "");
                }

                if (!string.IsNullOrEmpty(place)) {
                    GDMCustomEvent evt = fBase.Context.CreateEventEx(iRec, GEDCOMTagName.RESI, "", "");
                    evt.Place.StringValue = place;
                }

                if (!string.IsNullOrEmpty(comment)) {
                    GDMNoteRecord noteRec = fBase.Context.Tree.CreateNote();
                    noteRec.SetNoteText(comment);
                    iRec.AddNote(noteRec);
                }

                if (srcRec != null) {
                    iRec.AddSource(srcRec, srcPage, 0);
                }

                fBase.NotifyRecord(iRec, RecordAction.raAdd);

                GDMFamilyRecord family = null;

                if (link == PersonLink.plPerson) {

                    iMain = iRec;
                    string evName = "";

                    if (eventType >= 0) {
                        switch (eventType) {
                            case 0:
                                evName = GEDCOMTagName.BIRT;
                                break;
                            case 1:
                                evName = GEDCOMTagName.DEAT;
                                break;
                            case 2:
                                evName = GEDCOMTagName.MARR;
                                break;
                        }
                    }

                    if (evName == GEDCOMTagName.BIRT || evName == GEDCOMTagName.DEAT) {
                        GDMCustomEvent evt = fBase.Context.CreateEventEx(iRec, evName, GDMDate.CreateByFormattedStr(eventDate, false), "");
                        evt.Place.StringValue = place;
                    } else if (evName == GEDCOMTagName.MARR) {
                        family = fBase.Context.GetMarriageFamily(iRec, true);
                        GDMCustomEvent evt = fBase.Context.CreateEventEx(family, evName, GDMDate.CreateByFormattedStr(eventDate, false), "");
                        evt.Place.StringValue = place;
                    }

                } else {

                    if (iMain == null) {
                        throw new PersonScanException(fLangMan.LS(FLS.LSID_BasePersonInvalid));
                    } else {
                        switch (link) {
                            case PersonLink.plFather:
                            case PersonLink.plMother:
                                family = fBase.Context.GetParentsFamily(iMain, true);
                                family.AddSpouse(iRec);
                                break;

                            case PersonLink.plGodparent:
                                iMain.AddAssociation(fLangMan.LS(FLS.LSID_PLGodparent), iRec);
                                break;

                            case PersonLink.plSpouse:
                                family = fBase.Context.GetMarriageFamily(iMain, true);
                                family.AddSpouse(iRec);
                                break;

                            case PersonLink.plChild:
                                family = fBase.Context.GetMarriageFamily(iMain, true);
                                family.AddChild(iRec);
                                break;
                        }
                    }

                }
            }
        }
    }
}
