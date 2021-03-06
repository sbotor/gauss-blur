using System;
using System.Collections.Generic;
using System.Text;
using GaussBlur.ImageProc;

namespace GaussBlur.Threading
{
    unsafe abstract class ThreadFactory
    {
        public BlurTask Task { get; set; }
        public byte* HelperP { get; set; }

        public double KernelSD { get; set; }

        public Kernel ImageKernel { get; set; }

        public Kernel.Type KernelType { get; protected set; }

        public ThreadFactory(double kernelSD)
        {
            Task = null;
            HelperP = null;
            KernelSD = kernelSD;

            ImageKernel = new Kernel(kernelSD);
            KernelType = Kernel.Type.None;
        }

        public abstract BlurThread Create(int start, int end);

        public virtual void Init(BlurTask task, byte* helperP, void* kernelP)
        {
            Task = task;
            HelperP = helperP;
        }
    }
}
