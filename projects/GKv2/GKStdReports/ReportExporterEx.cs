﻿/*
 *  "GEDKeeper", the personal genealogical database editor.
 *  Copyright (C) 2009-2022 by Sergey V. Zhdanovskih.
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

using GDModel;
using GKCore;
using GKCore.Export;
using GKCore.Interfaces;

namespace GKStdReports
{
    public abstract class ReportExporterEx : ReportExporter
    {
        public ReportExporterEx(IBaseWindow baseWin, bool albumPage) : base(baseWin, albumPage)
        {
        }

        protected static string GetName(GDMIndividualRecord iRec)
        {
            return GKUtils.GetNameString(iRec, true, false);
        }

        protected static string Localize(RLS lsid, params object[] args)
        {
            return string.Format(SRLangMan.LS(lsid), args);
        }
    }
}
