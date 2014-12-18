using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Windows.Forms;
using Pixelaria.Utils;
using Pixelaria.Views.Controls.PaintOperations.Abstracts;
using Pixelaria.Views.Controls.PaintOperations.Interfaces;

namespace Pixelaria.Views.Controls.PaintOperations
{
    /// <summary>
    /// Implements a Bucket paint operation
    /// </summary>
    public class BucketPaintOperation : BasePaintOperation, IPaintOperation, IColoredPaintOperation, ICompositingPaintOperation
    {
        /// <summary>
        /// The compositing mode for this paint operation
        /// </summary>
        protected CompositingMode compositingMode;

        /// <summary>
        /// Gets the cursor to use when hovering over the InternalPictureBox while this operation is up
        /// </summary>
        public override Cursor OperationCursor { get; protected set; }

        /// <summary>
        /// The first color currently being used to paint on the InternalPictureBox
        /// </summary>
        private Color firstColor = Color.Black;

        /// <summary>
        /// The second color currently being used to paint on the InternalPictureBox
        /// </summary>
        private Color secondColor = Color.Black;

        /// <summary>
        /// The point at which the mouse is currently over
        /// </summary>
        private Point mousePosition;

        /// <summary>
        /// The last recorded mouse position
        /// </summary>
        private Point lastMousePosition;

        /// <summary>
        /// Gets or sets the first color being used to paint on the InternalPictureBox
        /// </summary>
        public virtual Color FirstColor { get { return firstColor; } set { firstColor = value; } }

        /// <summary>
        /// Gets or sets the first color being used to paint on the InternalPictureBox
        /// </summary>
        public virtual Color SecondColor { get { return secondColor; } set { secondColor = value; } }

        /// <summary>
        /// Gets or sets the compositing mode for this paint operation
        /// </summary>
        public CompositingMode CompositingMode { get { return compositingMode; } set { compositingMode = value; } }

        /// <summary>
        /// Initialies a new instance of the BucketPaintOperation class, setting the two drawing colors
        /// for the paint operation
        /// </summary>
        /// <param name="firstColor">The first color for the paint operation</param>
        /// <param name="secondColor">The second color for the paint operation</param>
        public BucketPaintOperation(Color firstColor, Color secondColor)
        {
            this.firstColor = firstColor;
            this.secondColor = secondColor;
        }

        /// <summary>
        /// Initializes this Paint Operation
        /// </summary>
        /// <param name="pictureBox">The picture box to initialize the paint operation on</param>
        public override void Initialize(ImageEditPanel.InternalPictureBox pictureBox)
        {
            base.Initialize(pictureBox);

            // Initialize the operation cursor
            MemoryStream cursorMemoryStream = new MemoryStream(Properties.Resources.bucket_cursor);
            this.OperationCursor = new Cursor(cursorMemoryStream);
            cursorMemoryStream.Dispose();

            this.Loaded = true;
        }

        /// <summary>
        /// Finalizes this Paint Operation
        /// </summary>
        public override void Destroy()
        {
            this.OperationCursor.Dispose();

            this.Loaded = false;
        }

        /// <summary>
        /// Called to notify this PaintOperation that the mouse is being held down
        /// </summary>
        /// <param name="e">The event args for this event</param>
        public override void MouseDown(MouseEventArgs e)
        {
            Point point = GetAbsolutePoint(e.Location);

            if (e.Button == MouseButtons.Left || e.Button == MouseButtons.Right)
            {
                Color color = e.Button == MouseButtons.Left ? firstColor : secondColor;

                if (WithinBounds(point))
                {
                    PerformBucketOperaiton(color, point, compositingMode);
                }
            }
            else if (e.Button == MouseButtons.Middle)
            {
                firstColor = pictureBox.Bitmap.GetPixel(point.X, point.Y);

                pictureBox.OwningPanel.FireColorChangeEvent(firstColor);
            }
        }

        /// <summary>
        /// Called to notify this PaintOperation that the mouse is being moved
        /// </summary>
        /// <param name="e">The event args for this event</param>
        public override void MouseMove(MouseEventArgs e)
        {
            base.MouseMove(e);

            mousePosition = e.Location;

            if (e.Button == MouseButtons.Middle)
            {
                Point mouse = GetAbsolutePoint(mousePosition);
                Point mouseLast = GetAbsolutePoint(lastMousePosition);

                if (mouse != mouseLast && WithinBounds(mouse))
                {
                    firstColor = pictureBox.Bitmap.GetPixel(mouse.X, mouse.Y);

                    pictureBox.OwningPanel.FireColorChangeEvent(firstColor);
                }
            }

            lastMousePosition = mousePosition;
        }

        /// <summary>
        /// Performs the bucket fill operation
        /// </summary>
        /// <param name="color">The color of the fill operation</param>
        /// <param name="point">The point to start the fill operation at</param>
        /// <param name="compositingMode">The CompositingMode of the bucket fill operation</param>
        protected unsafe void PerformBucketOperaiton(Color color, Point point, CompositingMode compositingMode)
        {
            // Start the fill operation by getting the color under the user's mouse
            Color pColor = pictureBox.Bitmap.GetPixel(point.X, point.Y);

            Color newColor = (compositingMode == CompositingMode.SourceCopy ? color : color.Blend(pColor));

            int pColorI = pColor.ToArgb();
            int newColorI = newColor.ToArgb();

            if (pColorI == newColorI || pColor == color && (compositingMode == CompositingMode.SourceOver && pColor.A == 255 || compositingMode == CompositingMode.SourceCopy))
            {
                return;
            }

            // Lock the bitmap
            FastBitmap fastBitmap = new FastBitmap(pictureBox.Bitmap);
            fastBitmap.Lock();

            // Initialize the undo task
            PerPixelUndoTask undoTask = new PerPixelUndoTask(pictureBox, "Flood fill");

            Stack<int> stack = new Stack<int>();

            int y1;
            bool spanLeft, spanRight;

            int width = fastBitmap.Width;
            int height = fastBitmap.Height;

            stack.Push(((point.X << 16) | point.Y));

            // Do a floodfill using a vertical scanline algorithm
            while(stack.Count > 0)
            {
                int v = stack.Pop();
                int x = (v >> 16);
                int y = (v & 0xFFFF);

                y1 = y;

                while (y1 >= 0 && fastBitmap.GetPixelInt(x, y1) == pColorI) y1--;

                y1++;
                spanLeft = spanRight = false;

                while (y1 < height && fastBitmap.GetPixelInt(x, y1) == pColorI)
                {
                    fastBitmap.SetPixel(x, y1, newColorI);
                    undoTask.RegisterPixel(x, y1, pColorI, newColorI, false);

                    int pixel;

                    if (x > 0)
                    {
                        pixel = fastBitmap.GetPixelInt(x - 1, y1);

                        if (!spanLeft && pixel == pColorI)
                        {
                            stack.Push((((x - 1) << 16) | y1));

                            spanLeft = true;
                        }
                        else if (spanLeft && pixel != pColorI)
                        {
                            spanLeft = false;
                        }
                    }

                    if (x < width - 1)
                    {
                        pixel = fastBitmap.GetPixelInt(x + 1, y1);

                        if (!spanRight && pixel == pColorI)
                        {
                            stack.Push((((x + 1) << 16) | y1));
                            spanRight = true;
                        }
                        else if (spanRight && pixel != pColorI)
                        {
                            spanRight = false;
                        }
                    }
                    y1++;
                }
            }

            fastBitmap.Unlock();

            pictureBox.Invalidate();
            pictureBox.MarkModified();

            pictureBox.OwningPanel.UndoSystem.RegisterUndo(undoTask);
        }
    }
}