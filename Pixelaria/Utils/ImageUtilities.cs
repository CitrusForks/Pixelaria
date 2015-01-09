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
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

using Pixelaria.Data;
using Pixelaria.Properties;

namespace Pixelaria.Utils
{
    /// <summary>
    /// Contains static image-related utility methods
    /// </summary>
    public static class ImageUtilities
    {
        /// <summary>
        /// Checkers pattern used as the default tile
        /// </summary>
        private static readonly Image CheckersPattern = Resources.checkers_pattern;

        /// <summary>
        /// Resizes this Frame so it matches the given dimensions, scaling with the given scaling method, and interpolating
        /// with the given interpolation mode.
        /// Note that trying to resize a frame while it's inside an animation, and that animation's dimensions don't match
        /// the new size, an exception is thrown.
        /// This method disposes of the current frame texture
        /// </summary>
        /// <param name="image">The image to resize</param>
        /// <param name="newWidth">The new width of this animation</param>
        /// <param name="newHeight">The new height of this animation</param>
        /// <param name="scalingMethod">The scaling method to use to match this frame to the new size</param>
        /// <param name="interpolationMode">The interpolation mode to use when drawing the new frame</param>
        public static Image Resize(Image image, int newWidth, int newHeight, PerFrameScalingMethod scalingMethod, InterpolationMode interpolationMode)
        {
            if (image.Width == newWidth && image.Height == newHeight)
                return image;

            Rectangle currentBounds = new Rectangle(0, 0, image.Width, image.Height);
            Rectangle newBounds = new Rectangle(0, 0, newWidth, newHeight);

            // New bounds calculation
            if (scalingMethod == PerFrameScalingMethod.PlaceAtTopLeft)
            {
                newBounds = currentBounds;
            }
            else if (scalingMethod == PerFrameScalingMethod.PlaceAtCenter)
            {
                // Center the sprite
                currentBounds.X = newBounds.Width / 2 - currentBounds.Width / 2;
                currentBounds.Y = newBounds.Height / 2 - currentBounds.Height / 2;

                newBounds = currentBounds;
            }
            else if (scalingMethod == PerFrameScalingMethod.Zoom)
            {
                // If the target size is smaller than the image
                if (newBounds.Width < currentBounds.Width || newBounds.Height < currentBounds.Height)
                {
                    Rectangle rec = newBounds;

                    float num = Math.Min((float)newBounds.Width / currentBounds.Width, (float)newBounds.Height / currentBounds.Height);
                    newBounds.Width = (int)(currentBounds.Width * num);
                    newBounds.Height = (int)(currentBounds.Height * num);
                    newBounds.X = (rec.Width - newBounds.Width) / 2;
                    newBounds.Y = (rec.Height - newBounds.Height) / 2;
                }
                // If the image is smaller than the target size
                else
                {
                    // Center the sprite
                    currentBounds.X = newBounds.Width / 2 - currentBounds.Width / 2;
                    currentBounds.Y = newBounds.Height / 2 - currentBounds.Height / 2;

                    newBounds = currentBounds;
                }
            }

            // New texture creation
            Bitmap newTexture = new Bitmap(newWidth, newHeight, PixelFormat.Format32bppArgb);

            Graphics graphics = Graphics.FromImage(newTexture);

            graphics.InterpolationMode = interpolationMode;
            graphics.DrawImage(image, newBounds);

            graphics.Flush();
            graphics.Dispose();

            return newTexture;
        }

        /// <summary>
        /// Returns an Image object that represents the default tile image to be used in the background of controls
        /// that display any type of transparency
        /// </summary>
        /// <returns>
        /// An Image object that represents the default tile image to be used in the background of controlsthat display
        /// any type of transparency
        /// </returns>
        public static Image GetDefaultTile()
        {
            return CheckersPattern;
        }

        /// <summary>
        /// Returns a Rectangle that specifies the minimum image area, clipping out all the alpha pixels
        /// </summary>
        /// <param name="image">The image to find the mimimum image area</param>
        /// <returns>A Rectangle that specifies the minimum image area, clipping out all the alpha pixels</returns>
        public static Rectangle FindMinimumImageArea(Bitmap image)
        {
            int minImageX = 0;
            int minImageY = 0;

            int maxImageX = 0;
            int maxImageY = 0;

            int width = image.Width;
            int height = image.Height;

            using (FastBitmap fastBitmap = image.FastLock())
            {
                // Scan vertically - 1st pass
                int x;
                int y;

                for (x = 0; x < width; x++)
                {
                    for (y = 0; y < height; y++)
                    {
                        if (fastBitmap.GetPixelInt(x, y) >> 24 != 0)
                        {
                            minImageX = x;
                            goto skipx;
                        }
                        // All pixels scanned, non are opaque
                        if (x == width - 1 && y == height - 1)
                        {
                            return new Rectangle(0, 0, 0, 0);
                        }
                    }
                } skipx:

                // Scan horizontally - 1st pass
                for (y = 0; y < height; y++)
                {
                    for (x = minImageX; x < width; x++)
                    {
                        minImageY = y;

                        if (fastBitmap.GetPixelInt(x, y) >> 24 != 0)
                        {
                            goto skipy;
                        }
                    }
                } skipy:

                // Scan vertically - 2nd pass
                for (x = width - 1; x >= minImageX; x--)
                {
                    for (y = height - 1; y >= minImageY; y--)
                    {
                        if (fastBitmap.GetPixelInt(x, y) >> 24 != 0)
                        {
                            maxImageX = x;
                            goto skipw;
                        }
                    }
                } skipw:

                // Scan horizontally - 2nd pass
                for (y = height - 1; y >= minImageY; y--)
                {
                    for (x = minImageX; x <= maxImageX; x++)
                    {
                        if (fastBitmap.GetPixelInt(x, y) >> 24 != 0)
                        {
                            maxImageY = y;
                            goto skiph;
                        }
                    }
                } skiph:
                ;
            }

            return new Rectangle(minImageX, minImageY, maxImageX - minImageX + 1, maxImageY - minImageY + 1);
        }

        /// <summary>
        /// Draws a bitmap on top of a target bitmap using color blending.
        /// If the two images don't have the same dimensions or don't have a 32bpp ARGB, an exception is raised
        /// </summary>
        /// <param name="target">The bitmap to draw the foreground onto</param>
        /// <param name="foreBitmap">The foreground bitmap to draw onto the target bitmap</param>
        /// <param name="highQuality">Whether to use high quality image composition. This composition mode is considerably slower than low quality</param>
        /// <exception cref="Exception">The size of the bitmaps must be equal and both bitmaps must have a 32bpp ARGB pixel format</exception>
        public static void FlattenBitmaps(Bitmap target, Bitmap foreBitmap, bool highQuality)
        {
            if (target.Size != foreBitmap.Size || target.PixelFormat != PixelFormat.Format32bppArgb || foreBitmap.PixelFormat != PixelFormat.Format32bppArgb)
            {
                throw new Exception("The size of the bitmaps must be equal and both bitmaps must have a 32bpp ARGB pixel format");
            }

            if (!highQuality)
            {
                using(Graphics gfx = Graphics.FromImage(target))
                {
                    gfx.CompositingMode = CompositingMode.SourceOver;
                    gfx.DrawImageUnscaled(foreBitmap, 0, 0);

                    gfx.Flush();
                }

                return;
            }

            using (FastBitmap fastTarget = target.FastLock(), fastForeBitmap = foreBitmap.FastLock())
            {
                for (int y = 0; y < target.Height; y++)
                {
                    for (int x = 0; x < target.Width; x++)
                    {
                        fastTarget.SetPixel(x, y, Utilities.FlattenColor(fastTarget.GetPixel(x, y), fastForeBitmap.GetPixel(x, y)));
                    }
                }
            }
        }
    }
}