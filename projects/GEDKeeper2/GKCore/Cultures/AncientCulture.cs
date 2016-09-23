﻿/*
 *  "GEDKeeper", the personal genealogical database editor.
 *  Copyright (C) 2009-2016 by Serg V. Zhdanovskih (aka Alchemist, aka Norseman).
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
using GKCommon.GEDCOM;
using GKCore.Interfaces;

namespace GKCore.Cultures
{
    /// <summary>
    /// 
    /// </summary>
    public class AncientCulture : ICulture
    {
        public AncientCulture()
        {
        }

        public bool HasPatronymic()
        {
            return false;
        }

        public bool HasSurname()
        {
            return false;
        }

        public string NormalizeSurname(string sn, bool aFemale)
        {
            return sn;
        }

        public string GetMarriedSurname(string husbSurname)
        {
            return husbSurname;
        }

        public GEDCOMSex GetSex(string iName, string iPat, bool canQuery)
        {
            return GEDCOMSex.svUndetermined;
        }

        public string[] GetSurnames(string surname, bool female)
        {
            string[] result = new string[1];
            result[0] = surname;
            return result;
        }

        public string[] GetSurnames(GEDCOMIndividualRecord iRec)
        {
            if (iRec == null)
                throw new ArgumentNullException("iRec");

            string fam, nam, pat;
            GKUtils.GetNameParts(iRec, out fam, out nam, out pat);
            bool female = (iRec.Sex == GEDCOMSex.svFemale);

            return GetSurnames(fam, female);
        }

        public string GetGenitiveName(string name)
        {
            return name;
        }
    }
}
