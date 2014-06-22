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
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

using Pixelaria.Data;
using Pixelaria.Data.Exports;

using Pixelaria.Controllers;

using Pixelaria.Utils;

namespace Pixelaria.Views.ModelViews
{
    /// <summary>
    /// Form used as interface for creating a new Animation Sheet
    /// </summary>
    public partial class AnimationSheetView : ModifiableContentView
    {
        /// <summary>
        /// The current AnimationSheet being edited
        /// </summary>
        AnimationSheet sheetToEdit;

        /// <summary>
        /// The controller that owns this form
        /// </summary>
        Controller controller;
        
        /// <summary>
        /// The current export settings
        /// </summary>
        AnimationExportSettings exportSettings;

        /// <summary>
        /// The current bundle sheet export
        /// </summary>
        BundleSheetExport bundleSheetExport;

        /// <summary>
        /// Gets the current AnimationSheet being edited
        /// </summary>
        public AnimationSheet CurrentSheet { get { return sheetToEdit; } }

        /// <summary>
        /// Initializes a new instance of the AnimationSheetEditor class
        /// </summary>
        /// <param name="controller">The controller that owns this form</param>
        /// <param name="sheetToEdit">The sheet to edit on this form. Leave null to show an interface to create a new sheet</param>
        public AnimationSheetView(Controller controller, AnimationSheet sheetToEdit = null)
        {
            InitializeComponent();

            zpb_sheetPreview.HookToForm(this);

            this.controller = controller;
            this.sheetToEdit = sheetToEdit;

            if (sheetToEdit != null)
                exportSettings = sheetToEdit.ExportSettings;

            InitializeFiends();
            ValidateFields();
        }

        /// <summary>
        /// Initializes the fields of this form
        /// </summary>
        public void InitializeFiends()
        {
            // If no sheet is present, disable sheet preview
            if (this.sheetToEdit == null)
            {
                this.btn_generatePreview.Visible = false;
                this.lbl_sheetPreview.Visible = false;
                this.zpb_sheetPreview.Visible = false;

                this.gb_sheetInfo.Visible = false;
                this.gb_exportSummary.Visible = false;

                this.lbl_zoomLevel.Visible = false;

                this.Text = "New Animation Sheet";

                return;
            }

            this.txt_sheetName.Text = this.sheetToEdit.Name;

            this.cb_favorRatioOverarea.Checked = this.sheetToEdit.ExportSettings.FavorRatioOverArea;
            this.cb_forcePowerOfTwoDimensions.Checked = this.sheetToEdit.ExportSettings.ForcePowerOfTwoDimensions;
            this.cb_forceMinimumDimensions.Checked = this.sheetToEdit.ExportSettings.ForceMinimumDimensions;
            this.cb_reuseIdenticalFrames.Checked = this.sheetToEdit.ExportSettings.ReuseIdenticalFramesArea;
            this.cb_highPrecision.Checked = this.sheetToEdit.ExportSettings.HighPrecisionAreaMatching;
            this.cb_allowUordering.Checked = this.sheetToEdit.ExportSettings.AllowUnorderedFrames;
            this.cb_useUniformGrid.Checked = this.sheetToEdit.ExportSettings.UseUniformGrid;
            this.cb_padFramesOnXml.Checked = this.sheetToEdit.ExportSettings.UsePaddingOnXml;
            this.cb_exportXml.Checked = this.sheetToEdit.ExportSettings.ExportXml;
            this.nud_xPadding.Value = this.sheetToEdit.ExportSettings.XPadding;
            this.nud_yPadding.Value = this.sheetToEdit.ExportSettings.YPadding;

            this.modified = false;

            this.Text = "Animation Sheet [" + sheetToEdit.Name + "]";

            UpdateCountLabels();
        }

        /// <summary>
        /// Updates the animation and frame count labels 
        /// </summary>
        public void UpdateCountLabels()
        {
            lbl_animCount.Text = sheetToEdit.AnimationCount + "";
            lbl_frameCount.Text = sheetToEdit.GetFrameCount() + "";
        }

        /// <summary>
        /// Validates the fields from this form, and disables the saving of changes if one or more of the fields is invalid
        /// </summary>
        /// <returns>Whether the validation was successful</returns>
        public bool ValidateFields()
        {
            bool valid = true;
            bool alert = false;
            string validation;

            // Animation name
            validation = controller.AnimationSheetValidator.ValidateAnimationSheetName(txt_sheetName.Text, sheetToEdit);
            if (validation != "")
            {
                txt_sheetName.BackColor = Color.LightPink;
                lbl_error.Text = validation;
                valid = false;
            }
            else
            {
                txt_sheetName.BackColor = Color.White;
            }

            pnl_errorPanel.Visible = !valid;
            pnl_alertPanel.Visible = alert;

            btn_ok.Enabled = valid;
            btn_apply.Enabled = (valid && modified);

            return valid;
        }

        /// <summary>
        /// Applies the changes made by this form to the affected objects
        /// </summary>
        public override void ApplyChanges()
        {
            if(!ValidateFields())
                return;

            sheetToEdit.Name = txt_sheetName.Text;
            sheetToEdit.ExportSettings = RepopulateExportSettings();

            controller.UpdatedAnimationSheet(sheetToEdit);

            this.Text = "Animation Sheet [" + sheetToEdit.Name + "]";
            this.btn_apply.Enabled = false;

            base.ApplyChanges();
        }

        /// <summary>
        /// Displays a confirmation to the user when changes have been made to the animation sheet.
        /// If the view is opened as a creation view, no confirmation is displayed to the user in any way
        /// </summary>
        /// <returns>The DialogResult of the confirmation MessageBox displayed to the user</returns>
        public override DialogResult ConfirmChanges()
        {
            if (sheetToEdit != null)
            {
                return base.ConfirmChanges();
            }
            else
            {
                return DialogResult.OK;
            }
        }

        /// <summary>
        /// Marks the contents of this view as Modified
        /// </summary>
        public override void MarkModified()
        {
            if (sheetToEdit != null)
            {
                this.Text = "Animation Sheet [" + sheetToEdit.Name + "]*";
                this.btn_apply.Enabled = true;
            }

            base.MarkModified();
        }

        /// <summary>
        /// Repopulates the AnimationExportSettings field of this form with the form's fields and returns it
        /// </summary>
        /// <returns>The newly repopulated AnimationExportSettings</returns>
        public AnimationExportSettings RepopulateExportSettings()
        {
            exportSettings.FavorRatioOverArea = cb_favorRatioOverarea.Checked;
            exportSettings.ForcePowerOfTwoDimensions = cb_forcePowerOfTwoDimensions.Checked;
            exportSettings.ForceMinimumDimensions = cb_forceMinimumDimensions.Checked;
            exportSettings.ReuseIdenticalFramesArea = cb_reuseIdenticalFrames.Checked;
            exportSettings.HighPrecisionAreaMatching = cb_highPrecision.Checked;
            exportSettings.AllowUnorderedFrames = cb_allowUordering.Checked;
            exportSettings.UseUniformGrid = cb_useUniformGrid.Checked;
            exportSettings.UsePaddingOnXml = cb_padFramesOnXml.Checked;
            exportSettings.ExportXml = cb_exportXml.Checked;
            exportSettings.XPadding = (int)nud_xPadding.Value;
            exportSettings.YPadding = (int)nud_yPadding.Value;

            return exportSettings;
        }

        /// <summary>
        /// Generates a preview for the AnimationSheet currently loaded into this form
        /// </summary>
        public void GeneratePreview()
        {
            RepopulateExportSettings();
            UpdateCountLabels();

            // Dispose of current preview
            RemovePreview();

            if (sheetToEdit.Animations.Length > 0)
            {
                // Time the bundle export
                pb_exportProgress.Visible = true;

                BundleExportProgressEventHandler handler = new BundleExportProgressEventHandler(
                    (BundleExportProgressEventArgs args) =>
                    {
                        pb_exportProgress.Value = args.StageProgress;
                        pb_exportProgress.Refresh();
                    }
                );

                this.FindForm().Cursor = Cursors.WaitCursor;
                Stopwatch sw = Stopwatch.StartNew();

                // Export the bundle
                bundleSheetExport = controller.GenerateBundleSheet(exportSettings, handler, sheetToEdit.Animations);

                Image img = bundleSheetExport.Sheet;

                sw.Stop();
                this.FindForm().Cursor = Cursors.Default;

                zpb_sheetPreview.SetImage(img);

                pb_exportProgress.Visible = false;

                // Update labels
                lbl_sheetPreview.Text = "Sheet Preview: (generated in " + sw.ElapsedMilliseconds + "ms)";

                lbl_dimensions.Text = img.Width + "x" + img.Height;
                lbl_pixelCount.Text = (img.Width * img.Height).ToString("N0");
                lbl_framesOnSheet.Text = (bundleSheetExport.FrameCount - bundleSheetExport.ReusedFrameCount) + "";
                lbl_reusedFrames.Text = (bundleSheetExport.ReusedFrameCount) + "";
                lbl_memoryUsage.Text = Utilities.FormatByteSize(Utilities.MemoryUsageOfImage(img));

                if (pnl_alertPanel.Visible && lbl_alertLabel.Text == "No animations on sheet to generate preview.")
                {
                    pnl_alertPanel.Visible = false;
                }

                if (cb_showFrameBounds.Checked)
                {
                    ShowFrameBounds();
                }
            }
            else
            {
                lbl_alertLabel.Text = "No animations on sheet to generate preview.";
                pnl_alertPanel.Visible = true;
            }
        }

        /// <summary>
        /// Shows the frame bounds for the exported image
        /// </summary>
        public void ShowFrameBounds()
        {
            if (bundleSheetExport != null)
            {
                zpb_sheetPreview.LoadExportSheet(bundleSheetExport);
            }
        }

        /// <summary>
        /// Hides the frame bounds for the exported image
        /// </summary>
        public void HideFrameBounds()
        {
            zpb_sheetPreview.Unload();
        }

        /// <summary>
        /// Removes the currently displayed preview and disposes of the image
        /// </summary>
        public void RemovePreview()
        {
            if (zpb_sheetPreview.Image != null)
            {
                zpb_sheetPreview.Image.Dispose();
                zpb_sheetPreview.Image = null;
            }

            if (bundleSheetExport != null)
            {
                bundleSheetExport = null;
            }
        }

        /// <summary>
        /// Generates an AnimationSheet using the settings on this form's fields
        /// </summary>
        /// <returns>The new AnimationSheet object</returns>
        public AnimationSheet GenerateAnimationSheet()
        {
            AnimationSheet sheet = new AnimationSheet(txt_sheetName.Text);

            sheet.ExportSettings = RepopulateExportSettings();

            return sheet;
        }

        // 
        // Animation Sheet Name textfield change
        // 
        private void txt_sheetName_TextChanged(object sender, EventArgs e)
        {
            ValidateFields();

            MarkModified();
        }

        // 
        // Common event for all checkboxes on the form
        // 
        private void checkboxesChange(object sender, EventArgs e)
        {
            MarkModified();
        }

        // 
        // Common event for all nuds on the form
        // 
        private void nudsCommon(object sender, EventArgs e)
        {
            MarkModified();
        }

        // 
        // Generate Preview button click
        // 
        private void btn_generatePreview_Click(object sender, EventArgs e)
        {
            GeneratePreview();
        }

        // 
        // Form Closing event handler
        // 
        private void AnimationSheetView_FormClosed(object sender, FormClosedEventArgs e)
        {
            RemovePreview();
        }

        // 
        // Show Frame Bounds checkbox check
        // 
        private void cb_showFrameBounds_CheckedChanged(object sender, EventArgs e)
        {
            if (cb_showFrameBounds.Checked)
            {
                ShowFrameBounds();
            }
            else
            {
                HideFrameBounds();
            }
        }

        // 
        // Ok button click
        // 
        private void btn_ok_Click(object sender, EventArgs e)
        {
            // Validate one more time before closing
            if (ValidateFields() == false)
            {
                return;
            }

            if (sheetToEdit != null && modified)
            {
                ApplyChanges();
            }

            this.Close();
        }

        // 
        // Cancel button click
        // 
        private void btn_cancel_Click(object sender, EventArgs e)
        {
            DiscardChangesAndClose();
        }

        // 
        // Apply Changes button click
        // 
        private void btn_apply_Click(object sender, EventArgs e)
        {
            ApplyChanges();
        }

        // 
        // Sheet Preview ZPB zoom changed
        // 
        private void zpb_sheetPreview_ZoomChanged(object sender, Controls.ZoomChangedEventArgs e)
        {
            lbl_zoomLevel.Text = "Zoom: " + (Math.Ceiling(e.NewZoom * 100) / 100) + "x";
        }
    }
}