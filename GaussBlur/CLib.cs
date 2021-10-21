using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace GaussBlur
{
    class CLib
    {
        private const string dllDir = @"D:\OneDrive - Politechnika Śląska\Studia\JA\gauss-blur\x64\Debug\GaussBlurC.dll";

        [DllImport(dllDir)]
        public static extern int addTest(int first, int second);

        //[DllImport(dllDir)]
        //public static unsafe extern void blur(double* pixelArray, int start, int end, int imageStride, int imageHeight, int kernelSize, double stdDev);

        [DllImport(dllDir)]
        public static unsafe extern void blurX(double* src, double* dest, int start, int end, int imageStride, int imageHeight, double* kernel, int kernelSize);

        [DllImport(dllDir)]
        public static unsafe extern void blurY(double* src, double* dest, int start, int end, int imageStride, int imageHeight, double* kernel, int kernelSize);
    }
}
