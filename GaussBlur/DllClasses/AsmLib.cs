using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace GaussBlur.DLL
{
    class AsmLib
    {
        private const string dllDir = @"..\GaussBlurAsm.dll";

        [DllImport(dllDir)]
        public static extern unsafe void Init(byte* data, byte* helper, long stride, long height, float* kernel);

        [DllImport(dllDir)]
        public static extern unsafe void Init(byte* data, byte* helper, long stride, long height, int* fixedPointKernel);

        [DllImport(dllDir)]
        public static extern void BlurX(long start, long end);

        [DllImport(dllDir)]
        public static extern void BlurY(long start, long end);
    }
}
