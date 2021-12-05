using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace GaussBlur.DLL
{
    class CLib
    {
        //private const string dllDir = @"D:\OneDrive - Politechnika Śląska\Studia\JA\gauss-blur\x64\Debug\GaussBlurC.dll";
        private const string dllDir = @"..\GaussBlurC.dll";

        [DllImport(dllDir)]
        public static extern int addTest(int first, int second);

        [DllImport(dllDir)]
        public static unsafe extern void blurX(byte* data, byte* help,
            int start, int end, int imageStride, int imageHeight,
            double* kernel);

        [DllImport(dllDir)]
        public static unsafe extern void blurY(byte* data, byte* help,
            int start, int end, int imageStride, int imageHeight,
            double* kernel);
    }
}
