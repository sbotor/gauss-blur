using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace GaussBlur.Libraries
{
    class CLib
    {
        private const string dllDir = @"..\GaussBlurC.dll";

        [DllImport(dllDir)]
        public static extern int addTest(int first, int second);

        [DllImport(dllDir)]
        public static unsafe extern void BlurX(byte* data, byte* help,
            int start, int end, int imageStride, int imageHeight,
            float* kernel);

        [DllImport(dllDir)]
        public static unsafe extern void BlurY(byte* data, byte* help,
            int start, int end, int imageStride, int imageHeight,
            float* kernel);
    }
}
