using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace GaussBlur
{
    unsafe class AsmTest
    {
        private const string dllDir = @"D:\OneDrive - Politechnika Śląska\Studia\JA\gauss-blur\x64\Debug\GaussBlurAsm.dll";

        [DllImport(dllDir)]
        private static extern void testSIMD(float* first, float* second, float* result);

        [DllImport(dllDir)]
        private static extern void testSIMDInPlace(float* first, float* second);

        [DllImport(dllDir)]
        public static extern char sanityTest();

        public static float[] TestSIMDS(float[] first, float[] second)
        {
            float[] result = new float[4];

            fixed (float* firstP = first, secondP = second, resultP = result)
            {
                testSIMD(firstP, secondP, resultP);
            }

            return result;
        }

        public static void TestSIMDInPlaceS(float[] first, float[] second)
        {
            fixed (float* firstP = first, secondP = second)
            {
                testSIMDInPlace(firstP, secondP);
            }
        }
    }
}
