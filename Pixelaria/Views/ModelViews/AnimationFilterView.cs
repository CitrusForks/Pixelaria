﻿using System;
using System.Drawing;
using System.Windows.Forms;

using Pixelaria.Data;
using Pixelaria.Filters;

using Pixelaria.Views.Controls;
using Pixelaria.Views.Controls.Filters;

namespace Pixelaria.Views.ModelViews
{
    /// <summary>
    /// Implements an interface that the user can use to tweak settings of and apply a filter to an animation
    /// </summary>
    public partial class AnimationFilterView : Form
    {
        /// <summary>
        /// The animation to modify
        /// </summary>
        private readonly Animation _animation;

        /// <summary>
        /// Initializes a new instance of the AnimationFilterView class
        /// </summary>
        /// <param name="animation">The animation to show the filter to</param>
        public AnimationFilterView(Animation animation)
        {
            InitializeComponent();

            _animation = animation;

            tc_timeline.Minimum = 1;
            tc_timeline.Maximum = animation.FrameCount;

            tc_timeline.Range = new Point(0, animation.FrameCount);

            fs_filters.SetImage(animation.GetFrameAtIndex(0).GetComposedBitmap());

            pnl_errorPanel.Visible = false;

            btn_ok.Enabled = true;
        }

        /// <summary>
        /// Initializes a new instance of the BaseFilterView class
        /// </summary>
        /// <param name="filters">The array of FilterControls to use as interface to mediate the interaction between the filters to be applied and the user</param>
        /// <param name="animation">The animation to apply the filter to</param>
        public AnimationFilterView(FilterControl[] filters, Animation animation)
            : this(animation)
        {
            fs_filters.LoadFilters(filters);
        }

        /// <summary>
        /// Initializes a new instance of the BaseFilterView class
        /// </summary>
        /// <param name="preset">A FilterPreset that contains data about filters to load on this BaseFilterView</param>
        /// <param name="animation">The animation to apply the filter to</param>
        public AnimationFilterView(FilterPreset preset, Animation animation)
            : this(animation)
        {
            fs_filters.LoadFilterPreset(preset);
        }

        /// <summary>
        /// Returns whether the current filter configuration can make any significant changes to the bitmap loaded
        /// </summary>
        /// <returns>Whether the current filter configuration can make any significant changes to the bitmap loaded</returns>
        public bool ChangesDetected()
        {
            return fs_filters.ChangesDetected();
        }

        /// <summary>
        /// Applies the filter to the animation
        /// </summary>
        public void ApplyFilter()
        {
            if (fs_filters.ChangesDetected())
            {
                Point range = tc_timeline.GetRange();

                for (int i = range.X - 1; i < range.X + range.Y; i++)
                {
                    Frame frame = _animation[i] as Frame;

                    if(frame != null)
                    {
                        foreach (FilterContainer container in fs_filters.FilterContainers)
                        {
                            Bitmap bitmap = frame.GetComposedBitmap();
                            container.ApplyFilter(bitmap);
                            frame.SetFrameBitmap(bitmap);
                        }
                    }
                }
            }
        }

        // 
        // Timeline frame changed
        // 
        private void tc_timeline_FrameChanged(object sender, FrameChangedEventArgs eventArgs)
        {
            fs_filters.SetImage(_animation.GetFrameAtIndex(eventArgs.NewFrame - 1).GetComposedBitmap());
        }

        // 
        // Ok button click
        // 
        private void btn_ok_Click(object sender, EventArgs e)
        {
            ApplyFilter();

            DialogResult = DialogResult.OK;
            Close();
        }
    }
}