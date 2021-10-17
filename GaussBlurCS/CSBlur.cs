using System;
using System.Collections.Generic;
using System.Text;

namespace GaussBlurCS
{
    class CSBlur : IBlur
    {
        float[] IBlur.CreateKernel(int size, float sd)
        {
            return new float[1]; // TODO
        }

        byte[] IBlur.BlurImage(int start, int finish)
        {
            return new byte[1]; // TODO
        }
    }
}
