﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Pixelaria.Data;

namespace PixelariaTests.Generators
{
    /// <summary>
    /// Contains methods related to bundle generation used in unit tests
    /// </summary>
    public static class BundleGenerator
    {
        /// <summary>
        /// Generates a bundle with all features used, with a seed used to generate the randomicity. The bundles and their respective
        /// inner objects generated with the same seed will be guaranteed to be considered equal by the respective equality unit tests
        /// </summary>
        /// <param name="seed">An integer to utilize as a seed for the random number generator used to fill in the bundle</param>
        /// <returns>A Bundle filled with randomized objects</returns>
        public static Bundle GenerateTestBundle(int seed)
        {
            Random r = new Random(seed);

            Bundle bundle = new Bundle("Bundle" + r.Next());

            for (int i = 0; i < 5; i++)
            {
                bundle.AddAnimationSheet(AnimationSheetGenerator.GenerateAnimationSheet("Sheet" + i, 5, r.Next(10, 128), r.Next(10, 128), r.Next(2, 5), r.Next()));
            }

            return bundle;
        }
    }
}