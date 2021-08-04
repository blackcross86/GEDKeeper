﻿/*
 *  This file is part of the "GKMap".
 *  GKMap project borrowed from GMap.NET (by radioman).
 *
 *  Copyright (C) 2009-2018 by radioman (email@radioman.lt).
 *  This program is licensed under the FLAT EARTH License.
 */

using System;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace GKMap.WinForms
{
    /// <summary>
    /// GKMap marker
    /// </summary>
    public class GMapToolTip : IDisposable
    {
        private bool fDisposed;
        private GMapMarker fMarker;

        public Point Offset;

        public static readonly StringFormat DefaultFormat = new StringFormat();

        public GMapMarker Marker
        {
            get {
                return fMarker;
            }
            internal set {
                fMarker = value;
            }
        }

        /// <summary>
        /// string format
        /// </summary>
        public readonly StringFormat Format = DefaultFormat;

        public static readonly Font DefaultFont = new Font(FontFamily.GenericSansSerif, 14, FontStyle.Bold, GraphicsUnit.Pixel);

        /// <summary>
        /// font
        /// </summary>
        public Font Font = DefaultFont;

        public static readonly Pen DefaultStroke = new Pen(Color.FromArgb(140, Color.MidnightBlue));

        /// <summary>
        /// specifies how the outline is painted
        /// </summary>
        public Pen Stroke = DefaultStroke;

        public static readonly Brush DefaultFill = new SolidBrush(Color.FromArgb(222, Color.AliceBlue));

        /// <summary>
        /// background color
        /// </summary>
        public Brush Fill = DefaultFill;

        public static readonly Brush DefaultForeground = new SolidBrush(Color.Navy);

        /// <summary>
        /// text foreground
        /// </summary>
        public Brush Foreground = DefaultForeground;

        /// <summary>
        /// text padding
        /// </summary>
        public Size TextPadding = new Size(10, 10);

        static GMapToolTip()
        {
            DefaultStroke.Width = 2;
            DefaultStroke.LineJoin = LineJoin.Round;
            DefaultStroke.StartCap = LineCap.RoundAnchor;

            DefaultFormat.LineAlignment = StringAlignment.Center;
            DefaultFormat.Alignment = StringAlignment.Center;
        }

        public GMapToolTip(GMapMarker marker)
        {
            Marker = marker;
            Offset = new Point(14, -44);
        }

        public virtual void OnRender(Graphics g)
        {
            Size st = g.MeasureString(Marker.ToolTipText, Font).ToSize();
            Rectangle rect = new Rectangle(Marker.ToolTipPosition.X, Marker.ToolTipPosition.Y - st.Height, st.Width + TextPadding.Width, st.Height + TextPadding.Height);
            rect.Offset(Offset.X, Offset.Y);

            g.DrawLine(Stroke, Marker.ToolTipPosition.X, Marker.ToolTipPosition.Y, rect.X, rect.Y + rect.Height / 2);

            g.FillRectangle(Fill, rect);
            g.DrawRectangle(Stroke, rect);

            g.DrawString(Marker.ToolTipText, Font, Foreground, rect, Format);
        }

        public void Dispose()
        {
            if (!fDisposed) {
                fDisposed = true;
            }
        }
    }
}
