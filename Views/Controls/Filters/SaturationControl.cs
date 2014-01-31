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
using System.Windows.Forms;

using Pixelaria.Filters;

namespace Pixelaria.Views.Controls.Filters
{
    /// <summary>
    /// Represents a FilterControl that handles a SaturationFilter
    /// </summary>
    public partial class SaturationControl : FilterControl
    {
        /// <summary>
        /// Initializes a new instance of the SaturationControl class
        /// </summary>
        public SaturationControl()
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

            if (this.filter == null)
            {
                this.filter = new SaturationFilter();
                (filter as SaturationFilter).Saturation = 100;
                (filter as SaturationFilter).Relative = false;
                (filter as SaturationFilter).KeepGrays = true;
            }

            this.updateRequired = true;
        }

        /// <summary>
        /// Updates the fields from this FilterControl based on the data from the
        /// given IFilter instance
        /// </summary>
        /// <param name="filter">The IFilter instance to update the fields from</param>
        public override void UpdateFieldsFromFilter(IFilter filter)
        {
            if (!(filter is SaturationFilter))
                return;

            anud_saturation.Value = (decimal)(filter as SaturationFilter).Saturation * 100;
            cb_relative.Checked = (filter as SaturationFilter).Relative;
            cb_keepGrays.Checked = (filter as SaturationFilter).KeepGrays;
            cb_multiply.Checked = (filter as SaturationFilter).Multiply;
        }

        // 
        // Saturation nud value changed
        // 
        private void anud_saturation_ValueChanged(object sender, EventArgs e)
        {
            (filter as SaturationFilter).Saturation = (int)anud_saturation.Value;

            updateRequired = true;
            FireFilterUpdated();
        }

        // 
        // Relative checkbox checked
        // 
        private void cb_relative_CheckedChanged(object sender, EventArgs e)
        {
            (filter as SaturationFilter).Relative = cb_relative.Checked;

            updateRequired = true;
            FireFilterUpdated();
        }

        // 
        // Keep Grays checkbox checked
        // 
        private void cb_keepGrays_CheckedChanged(object sender, EventArgs e)
        {
            (filter as SaturationFilter).KeepGrays = cb_keepGrays.Checked;

            updateRequired = true;
            FireFilterUpdated();
        }

        // 
        // Multiply checkbox checked
        // 
        private void cb_multiply_CheckedChanged(object sender, EventArgs e)
        {
            cb_keepGrays.Enabled = cb_relative.Enabled = !cb_multiply.Checked;

            (filter as SaturationFilter).Multiply = cb_multiply.Checked;

            updateRequired = true;
            FireFilterUpdated();
        }
    }
}