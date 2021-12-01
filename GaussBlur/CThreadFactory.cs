using System;
using System.Collections.Generic;
using System.Text;

namespace GaussBlur
{
    unsafe class CThreadFactory : IThreadFactory
    {
        public BlurThread Create(BlurTask task, byte* helperP, double* kernelP, int start, int end)
        {
            return new CThread(task, helperP, kernelP, start, end);
        }
    }
}
