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

        [DllImport(dllDir)]
        public static extern unsafe void testParams(long start, long end);

        [DllImport(dllDir)]
        private static extern unsafe void init(byte* data, byte* helper, long stride, long height, double* kernel);

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

        public static void safeTestParams(byte[] data, byte[] helper,
            int start, int end, int stride, int height, double[] kernel)
        {
            unsafe
            {
                fixed (byte* dataP = data, helperP = helper)
                {
                    fixed (double* kernelP = kernel)
                    {
                        init(dataP, helperP, stride, height, kernelP);
                        testParams(start, end);
                    }
                }
            }
        }
    }
}
