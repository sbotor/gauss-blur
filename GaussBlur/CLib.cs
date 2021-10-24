using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace GaussBlur
{
    class CLib
    {
        //private const string dllDir = @"D:\OneDrive - Politechnika Śląska\Studia\JA\gauss-blur\x64\Debug\GaussBlurC.dll";
        private const string dllDir = @"..\GaussBlurC.dll";

        [DllImport(dllDir)]
        public static extern int addTest(int first, int second);

        [DllImport(dllDir)]
        public static unsafe extern void blurX(double* data, double* help,
            int start, int end, int imageStride, int imageHeight,
            double* kernel, int kernelSize);

        [DllImport(dllDir)]
        public static unsafe extern void blurY(double* data, double* help,
            int start, int end, int imageStride, int imageHeight,
            double* kernel, int kernelSize);
    }
}
