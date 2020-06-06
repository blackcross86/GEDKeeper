﻿/*
 *  "GEDKeeper", the personal genealogical database editor.
 *  Copyright (C) 2009-2018 by Sergey V. Zhdanovskih.
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

using BSLib;
using BSLib.Design.Graphics;
using GKCore;
using GKCore.Charts;
using GKCore.Interfaces;

using BSDColors = BSLib.Design.BSDConsts.Colors;

namespace GKCore.Charts
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class TCPersonControl : ITreeControl
    {
        private readonly IPen fCtlPen;
        private readonly IBrush fCtlBrush;

        private TreeChartPerson fPerson;

        #region Public properties

        public override string Tip
        {
            get { return string.Empty; }
        }

        public override int Width
        {
            get { return 0; }
        }

        public override int Height
        {
            get { return 0; }
        }

        #endregion

        public TCPersonControl(ITreeChart chart) : base(chart)
        {
            var gfxProv = AppHost.GfxProvider;
            fCtlPen = gfxProv.CreatePen(gfxProv.CreateColor(BSDColors.Black), 2.0f);
            fCtlBrush = gfxProv.CreateSolidBrush(gfxProv.CreateColor(128, 128, 128, 128));
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) {
                fCtlPen.Dispose();
                fCtlBrush.Dispose();
            }
            base.Dispose(disposing);
        }

        public void SetPerson(TreeChartPerson person)
        {
            if (person == null) return;
            fPerson = person;

            ExtPoint offsets = fChart.GetOffsets();
            ExtRect rt = fPerson.Rect.GetOffset(offsets.X, offsets.Y);
            rt = ExtRect.CreateBounds(rt.Right, rt.Top, 40, rt.Height);
            fDestRect = rt;
        }

        public override void UpdateState()
        {
        }

        public override void UpdateView()
        {
        }

        public override void Draw(ChartRenderer gfx)
        {
            if (gfx == null) return;

            //gfx.FillRectangle(fCtlBrush, fDestRect);
            //gfx.DrawRectangle(fCtlPen, fDestRect);
        }

        public override void MouseDown(int x, int y)
        {
        }

        public override void MouseMove(int x, int y)
        {
        }

        public override void MouseUp(int x, int y)
        {
        }
    }
}
