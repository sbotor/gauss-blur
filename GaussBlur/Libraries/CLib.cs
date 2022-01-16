using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace GaussBlur.Libraries
{
    class CLib
    {
        public const string Directory = @"GaussBlurC.dll";

        [DllImport(Directory)]
        public static extern int addTest(int first, int second);

        [DllImport(Directory)]
        public static unsafe extern void BlurX(byte* data, byte* help,
            int start, int end, int imageStride, int imageHeight,
            float* kernel);

        [DllImport(Directory)]
        public static unsafe extern void BlurY(byte* data, byte* help,
            int start, int end, int imageStride, int imageHeight,
            float* kernel);
    }
}
