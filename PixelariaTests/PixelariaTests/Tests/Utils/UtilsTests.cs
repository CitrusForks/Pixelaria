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

using System.Drawing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Pixelaria.Utils;
using PixelariaTests.PixelariaTests.Generators;

namespace PixelariaTests.PixelariaTests.Tests.Utils
{
    /// <summary>
    /// Tests the behavior of the Utils class and related components
    /// </summary>
    [TestClass]
    public class UtilsTests
    {
        [TestMethod]
        public void TestImagesAreIdentical()
        {
            // Generate the bitmaps
            Bitmap bitmap1 = FrameGenerator.GenerateRandomBitmap(64, 64, 10);
            Bitmap bitmap2 = FrameGenerator.GenerateRandomBitmap(64, 64, 10);

            // Test the equality
            Assert.IsTrue(ImageUtilities.ImagesAreIdentical(bitmap1, bitmap2), "ImagesAreIdentical should return true for images that are equal down to each pixel");

            // Generate a different random bitmap
            bitmap2 = FrameGenerator.GenerateRandomBitmap(64, 64, 11);

            Assert.IsFalse(ImageUtilities.ImagesAreIdentical(bitmap1, bitmap2), "ImagesAreIdentical should return false for images that are not equal down to each pixel");
        }
    }
}