using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace GaussBlur.Libraries
{
    class AsmLib
    {
        private const string dllDir = @"..\GaussBlurAsm.dll";

        [DllImport(dllDir)]
        public static extern unsafe void Init(byte* data, byte* helper, long stride, long height, void* kernel);

        [DllImport(dllDir)]
        public static extern void BlurX(long start, long end);

        [DllImport(dllDir)]
        public static extern void BlurX_YMM(long start, long end);

        [DllImport(dllDir)]
        public static extern void BlurXAddDword_YMM(long start, long end);

        [DllImport(dllDir)]
        public static extern void BlurY(long start, long end);

        [DllImport(dllDir)]
        public static extern void BlurXAddDword(long start, long end);

        [DllImport(dllDir)]
        public static extern void BlurYAddDword(long start, long end);
    }
}
