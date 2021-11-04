using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace GaussBlur
{
    class AsmLib
    {
        private const string dllDir = @"..\GaussBlurAsm.dll";

        [DllImport(dllDir)]
        public static extern unsafe void testSIMD(double* firstArray, double* secondArray);

        public static void safeTestSIMD(double[] firstArray, double[] secondArray)
        {
            const int arrSize = 4;
            
            if (firstArray.Length != arrSize || secondArray.Length != arrSize)
            {
                throw new ArgumentException("Invalid array size.");
            }

            unsafe
            {
                fixed (double* firstP = firstArray, secondP = secondArray)
                {
                    testSIMD(firstP, secondP);
                }
            }
        }
    }
}
