using System;
using System.Collections.Generic;
using System.Text;

namespace GaussBlur.Threading
{
    unsafe class CFactory : ThreadFactory
    {
        public float* KernelP { get; private set; }
        
        public CFactory(double kernelSD) : base(kernelSD)
        {
            KernelType = ImageProc.Kernel.Type.NormalizedFloat;
        }
        
        public override unsafe BlurThread Create(int start, int end)
        {
            return new CThread(Task, HelperP, KernelP, start, end);
        }

        public override unsafe void Init(BlurTask task, byte* helperP, void* kernelP)
        {
            base.Init(task, helperP, kernelP);

            KernelP = (float*)kernelP;
        }
    }
}
