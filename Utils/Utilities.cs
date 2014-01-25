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
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;

using Pixelaria.Views.Controls;

namespace Pixelaria.Utils
{
    /// <summary>
    /// Contains static utility methods
    /// </summary>
    public static class Utilities
    {
        /// <summary>
        /// The hashing algorithm used for hashing the bitmaps
        /// </summary>
        private static HashAlgorithm shaM = new SHA256Managed();

        /// <summary>
        /// Returns a hash for the given Bitmap object
        /// </summary>
        /// <param name="bitmap">The bitmap to get the hash of</param>
        /// <returns>The hash of the given bitmap</returns>
        public static byte[] GetHashForBitmap(Bitmap bitmap)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                bitmap.Save(stream, ImageFormat.Png);

                stream.Position = 0;

                // Compute a hash for the image
                byte[] hash = GetHashForStream(stream);

                return hash;
            }
        }

        /// <summary>
        /// Returns a hash for the given Stream object
        /// </summary>
        /// <param name="stream">The stream to get the hash of</param>
        /// <returns>The hash of the given stream</returns>
        public static byte[] GetHashForStream(Stream stream)
        {
            // Compute a hash for the image
            return shaM.ComputeHash(stream);
        }

        /// <summary>
        /// Returns the memory usage of the given image, in bytes
        /// </summary>
        /// <returns>Total memory usage, in bytes</returns>
        public static long MemoryUsageOfImage(Image image)
        {
            long bytes = 0;

            bytes = image.Width * image.Height * Utilities.BitsPerPixelForFormat(image.PixelFormat) / 8;

            return bytes;
        }

        /// <summary>
        /// Helper method used to create relative paths when saving sheet paths down to the .xml file
        /// </summary>
        /// <param name="filespec">The file path</param>
        /// <param name="folder">The base folder to create the relative path</param>
        /// <returns>A relative path between folder and filespec</returns>
        public static string GetRelativePath(string filespec, string folder)
        {
            Uri pathUri = new Uri(filespec);
            
            // Folders must end in a slash
            if (!folder.EndsWith(Path.DirectorySeparatorChar.ToString()))
            {
                folder += Path.DirectorySeparatorChar;
            }

            return Uri.UnescapeDataString(new Uri(folder).MakeRelativeUri(pathUri).ToString().Replace('/', Path.DirectorySeparatorChar));
        }

        /// <summary>
        /// Returns a formated sting that contains the most significant magnitude
        /// representation of the given number of bytes
        /// </summary>
        /// <param name="bytes">The number of bytes</param>
        /// <returns>A formated string with the byte count converted to the most significant magnitude</returns>
        public static string FormatByteSize(long bytes)
        {
            int magnitude = 0;
            string[] sulfixes = new string[] { "b", "kb", "mb", "gb", "tb", "pt", "eb", "zb", "yb" };

            float b = bytes;

            while (b > 1024)
            {
                b /= 1024;
                magnitude++;
            }

            if (magnitude >= sulfixes.Length)
            {
                magnitude = sulfixes.Length - 1;
            }

            return Math.Round(b * 100) / 100 + sulfixes[magnitude];
        }

        /// <summary>
        /// Returns the total bits per pixel used by the given PixelFormat type
        /// </summary>
        /// <param name="pixelFormat">The PixelFormat to get the pixel usage from</param>
        /// <returns>The total bits per pixel used by the given PixelFormat type</returns>
        public static int BitsPerPixelForFormat(PixelFormat pixelFormat)
        {
            // Fetch the bits per pixel for the texture
            switch (pixelFormat)
            {
                // 1 bit (monochrome black and white)
                case PixelFormat.Format1bppIndexed:
                    return 1;

                // 4 bits
                case PixelFormat.Format4bppIndexed:
                    return 4;

                // 8 bits
                case PixelFormat.Format8bppIndexed:
                    return 16;

                // 16 bits
                case PixelFormat.Format16bppArgb1555:
                case PixelFormat.Format16bppGrayScale:
                case PixelFormat.Format16bppRgb555:
                case PixelFormat.Format16bppRgb565:
                    return 16;

                // 24 bits
                case PixelFormat.Format24bppRgb:
                    return 24;

                // 32 bits
                case PixelFormat.Format32bppArgb:
                case PixelFormat.Format32bppPArgb:
                case PixelFormat.Format32bppRgb:
                    return 32;

                // 48 bits
                case PixelFormat.Format48bppRgb:
                    return 48;

                // 64 bits
                case PixelFormat.Format64bppArgb:
                case PixelFormat.Format64bppPArgb:
                    return 64;
            }

            return 1;
        }

        /// <summary>
        /// Returns whether the two given images are identical to the pixel level.
        /// If the image dimensions are mis-matched, the method returns false.
        /// </summary>
        /// <param name="image1">The first image to compare</param>
        /// <param name="image2">The second image to compare</param>
        /// <returns>True whether the two images are identical, false otherwise</returns>
        public static bool ImagesAreIdentical(Image image1, Image image2)
        {
            if (image1.Size != image2.Size)
                return false;

            Bitmap bit1 = null;
            Bitmap bit2 = null;

            try
            {
                bit1 = (image1 is Bitmap ? (Bitmap)image1 : new Bitmap(image1));
                bit2 = (image2 is Bitmap ? (Bitmap)image2 : new Bitmap(image2));

                bool result = CompareMemCmp(bit1, bit2);

                return result;
            }
            finally
            {
                if (bit1 != image1)
                    bit1.Dispose();
                if (bit2 != image2)
                    bit2.Dispose();
            }
        }

        /// <summary>
        /// Compares two memory sections and returns 0 if the memory segments are identical
        /// </summary>
        /// <param name="b1">The pointer to the first memory segment</param>
        /// <param name="b2">The pointer to the second memory segment</param>
        /// <param name="count">The number of bytes to compare</param>
        /// <returns>0 if the memory segments are identical</returns>
        [DllImport("msvcrt.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern int memcmp(IntPtr b1, IntPtr b2, long count);

        /// <summary>
        /// Compares two arrays of bytes and returns 0 if they are memory identical
        /// </summary>
        /// <param name="b1">The first array of bytes</param>
        /// <param name="b2">The second array of bytes</param>
        /// <param name="count">The number of bytes to compare</param>
        /// <returns>0 if the byte arrays are identical</returns>
        [DllImport("msvcrt.dll", CallingConvention = CallingConvention.Cdecl)]
        static extern int memcmp(byte[] b1, byte[] b2, long count);

        /// <summary>
        /// Compares two arrays of bytes and returns true if they are identical
        /// </summary>
        /// <param name="b1">The first array of bytes</param>
        /// <param name="b2">The second array of bytes</param>
        /// <returns>True if the byte arrays are identical</returns>
        public static bool ByteArrayCompare(byte[] b1, byte[] b2)
        {
            // Validate buffers are the same length.
            // This also ensures that the count does not exceed the length of either buffer.  
            return b1.Length == b2.Length && memcmp(b1, b2, b1.Length) == 0;
        }

        /// <summary>
        /// Compares the memory portions of the two Bitmaps 
        /// </summary>
        /// <param name="b1"></param>
        /// <param name="b2"></param>
        /// <returns></returns>
        private static bool CompareMemCmp(Bitmap b1, Bitmap b2)
        {
            if ((b1 == null) != (b2 == null)) return false;

            BitmapData bd1 = b1.LockBits(new Rectangle(new Point(0, 0), b1.Size), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
            BitmapData bd2 = b2.LockBits(new Rectangle(new Point(0, 0), b2.Size), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);

            try
            {
                IntPtr bd1scan0 = bd1.Scan0;
                IntPtr bd2scan0 = bd2.Scan0;

                int stride = bd1.Stride;
                int len = stride * b1.Height;

                return memcmp(bd1scan0, bd2scan0, len) == 0;
            }
            finally
            {
                b1.UnlockBits(bd1);
                b2.UnlockBits(bd2);
            }
        }

        /// <summary>
        /// Converts a Color instance into an AHSL color
        /// </summary>
        /// <param name="color">The Color to convert to AHSL</param>
        /// <returns>An AHSL (alpha hue saturation and lightness) color</returns>
        public static AHSL ToAHSL(this Color color)
        {
            float r = color.R;
            float g = color.G;
            float b = color.B;

            if (r < 0) r = 0;
            if (g < 0) g = 0;
            if (b < 0) b = 0;
            if (r > 255) r = 255;
            if (g > 255) g = 255;
            if (b > 255) b = 255;

            r /= 255;
            g /= 255;
            b /= 255;

            float M = Math.Max(r, Math.Max(g, b));
            float m = Math.Min(r, Math.Min(g, b));
            float d = M - m;

            float h = 0;
            float s = 0;
            float l = 0;

            if (d == 0)
            {
                h = 0;
            }
            else if (M == r)
            {
                h = ((g - b) / d) % 6;
            }
            else if (M == g)
            {
                h = (b - r) / d + 2;
            }
            else
            {
                h = (r - g) / d + 4;
            }

            h *= 60;

            if (h < 0)
            {
                h += 360;
            }

            l = (M + m) / 2;

            if (d == 0)
            {
                s = 0;
            }
            else
            {
                s = d / (1 - Math.Abs(2 * l - 1));
            }

            s *= 100;

            return new AHSL(color.A, (int)h, (int)s, (int)(l * 100));
        }

        /// <summary>
        /// Fades the first color with the second, using the given factor to decide
        /// how much of each color will be used. The alpha channel is optionally changed
        /// </summary>
        /// <param name="color">The color to fade</param>
        /// <param name="fadeColor">The color to fade the first color to</param>
        /// <param name="factor">A number from [0 - 1] that decides how much the first color will fade into the second</param>
        /// <param name="blendAlpha">Whether to fade the alpha channel as well. If left false, the first color's alpha channel will be used</param>
        /// <returns>The faded color</returns>
        public static Color Fade(this Color color, Color fadeColor, float factor = 0.5f, bool blendAlpha = false)
        {
            float from = 1 - factor;

            int A = (int)(blendAlpha ? (color.A * from + fadeColor.A * factor) : color.A);
            int R = (int)(color.R * from + fadeColor.R * factor);
            int G = (int)(color.G * from + fadeColor.G * factor);
            int B = (int)(color.B * from + fadeColor.B * factor);
	        
	        return Color.FromArgb(Math.Abs(A), Math.Abs(R), Math.Abs(G), Math.Abs(B));
        }

        /// <summary>
        /// Blends the specified colors together
        /// </summary>
        /// <param name="color">Color to blend onto the background color.</param>
        /// <param name="backColor">Color to blend the other color onto.</param>
        /// <param name="factor">The factor to blend the two colors on. 0.0 will return the first color, 1.0 will return the back color, any values in between will blend the two colors accordingly</param>
        /// <returns>The blended color</returns>
        public static Color Blend(this Color color, Color backColor, float factor = 0.5f)
        {
            if (factor == 1 || color.A == 0)
                return backColor;
            if (factor == 0 || backColor.A == 0)
                return color;
            if (color.A == 255)
                return color;

            int Alpha = Convert.ToInt32(color.A) + 1;

            int B = Alpha * color.B + (255 - Alpha) * backColor.B >> 8;
            int G = Alpha * color.G + (255 - Alpha) * backColor.G >> 8;
            int R = Alpha * color.R + (255 - Alpha) * backColor.R >> 8;

            float alphaFg = (float)color.A / 255;
            float alphaBg = (float)backColor.A / 255;

            int A = (int)((alphaBg + alphaFg - alphaBg * alphaFg) * 255);

            if (backColor.A == 255)
            {
                A = 255;
            }
            if (A > 255)
            {
                A = 255;
            }
            if (R > 255)
            {
                R = 255;
            }
            if (G > 255)
            {
                G = 255;
            }
            if (B > 255)
            {
                B = 255;
            }

            return Color.FromArgb(Math.Abs(A), Math.Abs(R), Math.Abs(G), Math.Abs(B));
        }
    }
}