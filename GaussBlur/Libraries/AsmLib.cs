using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace GaussBlur.Libraries
{
    class AsmLib
    {
        public const string Directory = @"GaussBlurAsm.dll";

        [DllImport(Directory)]
        public static extern unsafe void Init(byte* data, byte* helper, long stride, long height, void* kernel);

        [DllImport(Directory)]
        public static extern void BlurX(long start, long end);

        [DllImport(Directory)]
        public static extern void BlurX_YMM(long start, long end);

        [DllImport(Directory)]
        public static extern void BlurY(long start, long end);
    }
}
