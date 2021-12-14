using System;
using System.Collections.Generic;
using System.Text;

namespace GaussBlur.Threading
{
    class CThreadFactory : BlurThreadFactory
    {
        public CThreadFactory(double kernelSD) : base(kernelSD)
        {
        }
        
        public override unsafe BlurThread Create(int start, int end)
        {
            return new CThread(task, helperP, kernelP, start, end);
        }

        public override unsafe void Init(BlurTask task, byte* helperP, float* kernelP)
        {
            base.Init(task, helperP, kernelP);
        }
    }
}
