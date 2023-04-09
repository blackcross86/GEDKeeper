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
using Eto.Forms;
using Eto.Serialization.Xaml;
using GKCore;
using GKCore.Interfaces;

namespace GKCalendarPlugin
{
    /// <summary>
    /// 
    /// </summary>
    public partial class CalendarWidget : Form, IWidgetForm
    {
        #region Design components
#pragma warning disable CS0169, CS0649, IDE0044, IDE0051

        private GroupBox grpSourceDate;
        private HistoryDateBox historyDateBox1;
        private GroupBox grpConvertedDate;
        private HistoryDateBox historyDateBox2;

#pragma warning restore CS0169, CS0649, IDE0044, IDE0051
        #endregion

        private readonly Plugin fPlugin;

        public CalendarWidget(Plugin plugin)
        {
            XamlReader.Load(this);

            fPlugin = plugin;

            historyDateBox1.DateChanged += hdb_DateChanged;

            historyDateBox2.ReadOnly = true;
            historyDateBox2.CalendarChanged += hdb_DateChanged;

            SetLocale();
        }

        private void CalendarWidget_Shown(object sender, EventArgs e)
        {
            AppHost.Instance.WidgetLocate(this, WidgetLocation.HRight | WidgetLocation.VTop);

            fPlugin.Host.WidgetShow(fPlugin);
        }

        private void CalendarWidget_Closed(object sender, EventArgs e)
        {
            fPlugin.Host.WidgetClose(fPlugin);
        }

        private void hdb_DateChanged(object sender, EventArgs e)
        {
            double jd = historyDateBox1.Date;
            historyDateBox2.Date = jd;
        }

        #region ILocalizable support

        public void SetLocale()
        {
            var langMan = fPlugin.LangMan;

            Title = langMan.LS(PLS.LSID_MICalendar);
            grpSourceDate.Text = langMan.LS(PLS.LSID_SourceDate);
            grpConvertedDate.Text = langMan.LS(PLS.LSID_ConvertedDate);
        }

        #endregion
    }
}
