using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using Pixelaria.Views.Controls.PaintOperations.Abstracts;
using Pixelaria.Views.Controls.PaintOperations.Interfaces;

namespace Pixelaria.Views.Controls.PaintOperations
{
    /// <summary>
    /// Implements a Zoom paint operation
    /// </summary>
    public class ZoomPaintOperation : BaseDraggingPaintOperation, IPaintOperation
    {
        /// <summary>
        /// Gets the cursor to use when hovering over the InternalPictureBox while this operation is up
        /// </summary>
        public override Cursor OperationCursor { get; protected set; }

        /// <summary>
        /// The relative point where the mouse was held down, in control coordinates
        /// </summary>
        public Point mouseDownRelative;

        /// <summary>
        /// The relative point where the mouse is currently at, in control coordinates
        /// </summary>
        public Point mousePointRelative;

        /// <summary>
        /// Initializes this Paint Operation
        /// </summary>
        /// <param name="pictureBox">The picture box to initialize the paint operation on</param>
        public override void Initialize(ImageEditPanel.InternalPictureBox pictureBox)
        {
            base.Initialize(pictureBox);

            // Initialize the operation cursor
            MemoryStream cursorMemoryStream = new MemoryStream(Properties.Resources.zoom_cursor);
            this.OperationCursor = new Cursor(cursorMemoryStream);
            cursorMemoryStream.Dispose();
        }

        /// <summary>
        /// Finalizes this Paint Operation
        /// </summary>
        public override void Destroy()
        {
            this.OperationCursor.Dispose();
        }

        /// <summary>
        /// Called to notify this PaintOperation that the control is being redrawn
        /// </summary>
        /// <param name="e">The event args for this event</param>
        public override void Paint(PaintEventArgs e)
        {
            if (mouseDown)
            {
                // Draw the zoom area
                Rectangle rec = GetRectangleArea(new Point[] { mouseDownRelative, mousePointRelative }, false);

                e.Graphics.ResetTransform();

                e.Graphics.DrawRectangle(Pens.Gray, rec);
            }
        }

        /// <summary>
        /// Called to notify this PaintOperation that the mouse is being held down
        /// </summary>
        /// <param name="e">The event args for this event</param>
        public override void MouseDown(MouseEventArgs e)
        {
            base.MouseDown(e);

            mousePointRelative = mouseDownRelative = e.Location;
        }

        /// <summary>
        /// Called to notify this PaintOperation that the mouse is being moved
        /// </summary>
        /// <param name="e">The event args for this event</param>
        public override void MouseMove(MouseEventArgs e)
        {
            base.MouseMove(e);

            if (mouseDown)
            {
                pictureBox.Invalidate(GetRectangleArea(new Point[] { mouseDownRelative, mousePointRelative }, false));

                mousePointRelative = e.Location;

                pictureBox.Invalidate(GetRectangleArea(new Point[] { mouseDownRelative, mousePointRelative }, false));
            }
        }

        /// <summary>
        /// Called to notify this PaintOperation that the mouse is being released
        /// </summary>
        /// <param name="e">The event args for this event</param>
        public override void MouseUp(MouseEventArgs e)
        {
            FinishOperation();

            base.MouseUp(e);
        }

        /// <summary>
        /// Finishes this ZoomOperation's current operation
        /// </summary>
        public void FinishOperation()
        {
            if (!mouseDown)
                return;

            Rectangle zoomArea = GetRectangleArea(new Point[] { mouseDownRelative, mousePointRelative }, false);

            pictureBox.Invalidate(zoomArea);

            float zoomX = pictureBox.Width / (float)zoomArea.Width;
            float zoomY = pictureBox.Height / (float)zoomArea.Height;

            if (zoomArea.Width < 2 && zoomArea.Height < 2)
            {
                zoomX = zoomY = 2f;

                zoomArea.X -= pictureBox.Width / 2;
                zoomArea.Y -= pictureBox.Height / 2;
            }

            zoomY = zoomX = Math.Min(zoomX, zoomY);

            pictureBox.Zoom = new PointF(pictureBox.Zoom.X * zoomX, pictureBox.Zoom.Y * zoomY);

            // Zoom in into the located region
            Point relative = pictureBox.Offset;

            relative.X += (int)(zoomArea.X * zoomX);
            relative.Y += (int)(zoomArea.Y * zoomX);

            pictureBox.Offset = relative;
        }
    }
}