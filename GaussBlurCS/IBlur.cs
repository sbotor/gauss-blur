using System;
using System.Collections.Generic;
using System.Text;

namespace GaussBlurCS
{
    interface IBlur
    {
        protected float[] CreateKernel(int size, float sd);

        public byte[] BlurImage(int start, int end);
    }
}
