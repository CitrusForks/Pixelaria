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
using System.Drawing;

using Pixelaria.Filters;

namespace Pixelaria.Views.Controls.Filters
{
    /// <summary>
    /// Represents a FilterControl that handles a LightnessFilter
    /// </summary>
    public partial class LightnessControl : FilterControl
    {
        /// <summary>
        /// Initializes a new instance of the LightnessControl class
        /// </summary>
        public LightnessControl()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Initializes this TransparencyControl
        /// </summary>
        /// <param name="bitmap">The Bitmap to generate the visualization for</param>
        public override void Initialize(Bitmap bitmap)
        {
            base.Initialize(bitmap);

            if (filter == null)
            {
                filter = new LightnessFilter();
                ((LightnessFilter)filter).Lightness = 100;
                ((LightnessFilter)filter).Relative = false;
            }
        }

        /// <summary>
        /// Updates the fields from this FilterControl based on the data from the
        /// given IFilter instance
        /// </summary>
        /// <param name="referenceFilter">The IFilter instance to update the fields from</param>
        public override void UpdateFieldsFromFilter(IFilter referenceFilter)
        {
            if (!(referenceFilter is LightnessFilter))
                return;

            anud_lightness.Value = (decimal)((LightnessFilter)referenceFilter).Lightness * 100;
            cb_relative.Checked = ((LightnessFilter)referenceFilter).Relative;
            cb_multiply.Checked = ((LightnessFilter)referenceFilter).Multiply;
        }

        // 
        // Lightness nud value changed
        // 
        private void anud_lightness_ValueChanged(object sender, EventArgs e)
        {
            ((LightnessFilter)filter).Lightness = (int)anud_lightness.Value;

            FireFilterUpdated();
        }

        // 
        // Relative checkbox checked
        // 
        private void cb_relative_CheckedChanged(object sender, EventArgs e)
        {
            ((LightnessFilter)filter).Relative = cb_relative.Checked;

            FireFilterUpdated();
        }

        // 
        // Multiply checkbox checked
        // 
        private void cb_multiply_CheckedChanged(object sender, EventArgs e)
        {
            cb_relative.Enabled = !cb_multiply.Checked;

            ((LightnessFilter)filter).Multiply = cb_multiply.Checked;

            FireFilterUpdated();
        }
    }
}