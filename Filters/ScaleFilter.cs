﻿/*
    Pixelaria
    Copyright (C) 2013 Luiz Fernando Silva

    This program is free software; you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation; either version 2 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License along
    with this program; if not, write to the Free Software Foundation, Inc.,
    51 Franklin Street, Fifth Floor, Boston, MA 02110-1301 USA.

    The full license may be found on the License.txt file attached to the
    base directory of this project.
*/

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;

using Pixelaria.Utils;

namespace Pixelaria.Filters
{
    /// <summary>
    /// Implements a Scaling filter
    /// </summary>
    public class ScaleFilter : IFilter
    {
        /// <summary>
        /// Gets a value indicating whether this IFilter instance will modify any of the pixels
        /// of the bitmap it is applied on with the current settings
        /// </summary>
        public bool Modifying { get { return ScaleX != 1 || ScaleY != 1; } }

        /// <summary>
        /// Gets or sets the X scale component as a floating point value
        /// </summary>
        public float ScaleX { get; set; }

        /// <summary>
        /// Gets or sets the Y scale component as a floating point value
        /// </summary>
        public float ScaleY { get; set; }

        /// <summary>
        /// Gets or sets whether to center the scaled image
        /// </summary>
        public bool Centered { get; set; }

        /// <summary>
        /// Gets or sets whether to use nearest-neighbor quality
        /// </summary>
        public bool PixelQuality { get; set; }

        /// <summary>
        /// Applies this ScaleFilter to a Bitmap
        /// </summary>
        /// <param name="bitmap">The bitmap to apply this TransparencyFilter to</param>
        public void ApplyToBitmap(Bitmap bitmap)
        {
            if (ScaleX == 1 && ScaleY == 1)
                return;

            Bitmap bit = bitmap.Clone() as Bitmap;

            Graphics g = Graphics.FromImage(bitmap);

            g.Clear(Color.Transparent);

            if (PixelQuality)
            {
                g.InterpolationMode = InterpolationMode.NearestNeighbor;
            }
            else
            {
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            }

            RectangleF rec = new RectangleF(0, 0, bitmap.Width, bitmap.Height);

            rec.Width *= ScaleX;
            rec.Height *= ScaleY;

            if (Centered)
            {
                rec.X = bitmap.Width / 2 - rec.Width / 2;
                rec.Y = bitmap.Height / 2 - rec.Height / 2;
            }

            g.DrawImage(bit, rec, new RectangleF(0, 0, bitmap.Width, bitmap.Height), GraphicsUnit.Pixel);

            g.Dispose();
            bit.Dispose();
        }
    }
}