using System;
using System.Collections.Generic;
using System.Text;

namespace GaussBlur.Threading
{
    unsafe interface IThreadFactory
    {
        public double[] Kernel { get; }

        public BlurThread Create(BlurTask task, byte* helperP, double* kernelP, int start, int end);
    }
}
