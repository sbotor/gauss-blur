using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace GaussBlur
{
    class CTest
    {
        private const string dllDir = @"D:\OneDrive - Politechnika Śląska\Studia\JA\gauss-blur\x64\Debug\GaussBlurC.dll";

        [DllImport(dllDir)]
        public static extern int addTest(int first, int second);
    }
}
