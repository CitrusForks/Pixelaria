﻿using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

using Pixelaria.Views.Controls;

using Pixelaria.Utils;

namespace Pixelaria.Views.ModelViews.Miscs
{
    /// <summary>
    /// Decorates the picture box with an onion skin display
    /// </summary>
    public class OnionSkinDecorator : PictureBoxDecorator
    {
        /// <summary>
        /// The frame view binded to this OnionSkinDecorator
        /// </summary>
        protected FrameView frameView;

        /// <summary>
        /// The onion skin bitmap
        /// </summary>
        protected Bitmap onionSkin;

        /// <summary>
        /// The settings to utilize when rendering this onion skin
        /// </summary>
        public OnionSkinSettings Settings;

        /// <summary>
        /// The event handler for the frame changed event
        /// </summary>
        readonly FrameView.EditFrameChangedEventHandler _frameChangedEventHandler;

        /// <summary>
        /// Initializes a new instance of the OnionSkinDecorator class
        /// </summary>
        /// <param name="frameView">The frame editor view to show the onion skin to</param>
        /// <param name="pictureBox">The picture box to decorate</param>
        public OnionSkinDecorator(FrameView frameView, ImageEditPanel.InternalPictureBox pictureBox)
            : base(pictureBox)
        {
            this.frameView = frameView;

            _frameChangedEventHandler = frameView_EditFrameChanged;

            this.frameView.EditFrameChanged += _frameChangedEventHandler;
        }

        // 
        // Frame View's frame changed event handler
        // 
        private void frameView_EditFrameChanged(object sender, EditFrameChangedEventArgs args)
        {
            if (Settings.OnionSkinEnabled)
            {
                DestroyOnionSkin();
                ShowOnionSkin();
            }
        }

        /// <summary>
        /// Initializes this PictureBoxDecorator's instance
        /// </summary>
        public override void Initialize()
        {

        }

        /// <summary>
        /// Destroys this PictureBoxDecorator's instance
        /// </summary>
        public override void Destroy()
        {
            if (onionSkin != null)
            {
                onionSkin.Dispose();
            }

            frameView.EditFrameChanged -= _frameChangedEventHandler;
        }

        /// <summary>
        /// Shows the onion skin
        /// </summary>
        public void ShowOnionSkin()
        {
            Settings.OnionSkinEnabled = true;

            if (frameView.FrameLoaded == null)
                return;

            if (onionSkin != null && (onionSkin.Width != frameView.FrameLoaded.Width || onionSkin.Height != frameView.FrameLoaded.Height))
            {
                onionSkin.Dispose();
                onionSkin = null;
            }
            else if (onionSkin != null)
            {
                FastBitmap.ClearBitmap(onionSkin, 0);
            }

            if (onionSkin == null)
            {
                // Create the new onion skin
                onionSkin = new Bitmap(frameView.FrameLoaded.Width, frameView.FrameLoaded.Height, PixelFormat.Format32bppArgb);
            }

            Graphics og = Graphics.FromImage(onionSkin);

            og.CompositingMode = CompositingMode.SourceOver;

            Rectangle bounds = new Rectangle(0, 0, frameView.FrameLoaded.Width, frameView.FrameLoaded.Height);

            // Create image attributes
            ImageAttributes attributes = new ImageAttributes();

            // Create a color matrix object
            ColorMatrix matrix = new ColorMatrix();

            //float multDecay = 0.3f + (0.7f / OnionSkinDepth);
            float multDecay = 0.5f + (Settings.OnionSkinDepth / 50.0f);

            // Draw the previous frames
            if (Settings.OnionSkinMode == OnionSkinMode.PreviousFrames || Settings.OnionSkinMode == OnionSkinMode.PreviousAndNextFrames)
            {
                int fi = frameView.FrameLoaded.Index;
                float mult = 1;
                for (int i = fi - 1; i > fi - Settings.OnionSkinDepth - 1 && i >= 0; i--)
                {
                    matrix.Matrix33 = Settings.OnionSkinTransparency * mult;
                    mult *= multDecay;
                    attributes.SetColorMatrix(matrix, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);

                    og.DrawImage(frameView.FrameLoaded.Animation[i].GetComposedBitmap(), bounds, 0, 0, bounds.Width, bounds.Height, GraphicsUnit.Pixel, attributes);
                }
            }
            // Draw the next frames
            if (Settings.OnionSkinMode == OnionSkinMode.NextFrames || Settings.OnionSkinMode == OnionSkinMode.PreviousAndNextFrames)
            {
                int fi = frameView.FrameLoaded.Index;
                float mult = 1;
                for (int i = fi + 1; i < fi + Settings.OnionSkinDepth + 1 && i < frameView.FrameLoaded.Animation.FrameCount; i++)
                {
                    matrix.Matrix33 = Settings.OnionSkinTransparency * mult;
                    mult *= multDecay;
                    attributes.SetColorMatrix(matrix, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);

                    og.DrawImage(frameView.FrameLoaded.Animation[i].GetComposedBitmap(), bounds, 0, 0, bounds.Width, bounds.Height, GraphicsUnit.Pixel, attributes);
                }
            }

            og.Flush();
            og.Dispose();

            pictureBox.DisplayImage = Settings.OnionSkinShowCurrentFrame;
            pictureBox.Invalidate();
        }

        /// <summary>
        /// Hides the onion skin for the current frame
        /// </summary>
        public void HideOnionSkin()
        {
            pictureBox.DisplayImage = true;

            DestroyOnionSkin();

            Settings.OnionSkinEnabled = false;
        }

        /// <summary>
        /// Destroys the current onion skin
        /// </summary>
        private void DestroyOnionSkin()
        {
            // Dispose of the onion skin
            if (onionSkin != null)
            {
                onionSkin.Dispose();
                onionSkin = null;
            }

            pictureBox.Invalidate();
        }

        /// <summary>
        /// Decorates the under image, using the given event arguments
        /// </summary>
        /// <param name="image">The under image to decorate</param>
        public override void DecorateUnderImage(Image image)
        {
            if (onionSkin != null && !Settings.DisplayOnFront)
            {
                Graphics g = Graphics.FromImage(image);

                g.DrawImage(onionSkin, 0, 0);
            }

            pictureBox.DisplayImage = Settings.OnionSkinShowCurrentFrame;
        }

        /// <summary>
        /// Decorates the over image, using the given event arguments
        /// </summary>
        /// <param name="image">The over image to decorate</param>
        public override void DecorateFrontImage(Image image)
        {
            if (onionSkin != null && Settings.DisplayOnFront)
            {
                Graphics g = Graphics.FromImage(image);

                g.DrawImage(onionSkin, 0, 0);
            }
        }
    }

    /// <summary>
    /// Represents the settings to apply to an onion skin decorator
    /// </summary>
    public struct OnionSkinSettings
    {
        /// <summary>
        /// Whether onion skin is currently enabled
        /// </summary>
        public bool OnionSkinEnabled;

        /// <summary>
        /// The depth of the onion skin, in frames
        /// </summary>
        public int OnionSkinDepth;

        /// <summary>
        /// The transparency of the onion skin
        /// </summary>
        public float OnionSkinTransparency;

        /// <summary>
        /// Whether to show the current frame on onion skin mode
        /// </summary>
        public bool OnionSkinShowCurrentFrame;

        /// <summary>
        /// Whether to display the onion skin above the current frame
        /// </summary>
        public bool DisplayOnFront;

        /// <summary>
        /// The mode to use on the onion skin
        /// </summary>
        public OnionSkinMode OnionSkinMode;
    }

    /// <summary>
    /// Specifies the mode of an onion skin display
    /// </summary>
    public enum OnionSkinMode
    {
        /// <summary>
        /// Display no frames on the onion skin
        /// </summary>
        None,
        /// <summary>
        /// Displays only the previous frames on the onion skin
        /// </summary>
        PreviousFrames,
        /// <summary>
        /// Displays both the previous and next frames on the onion skin
        /// </summary>
        PreviousAndNextFrames,
        /// <summary>
        /// Displays only the next frames on the onion skin
        /// </summary>
        NextFrames
    }
}