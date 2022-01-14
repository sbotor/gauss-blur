using System;
using System.Collections.Generic;
using System.Text;

using GaussBlur.Libraries;

namespace GaussBlur.Threading
{
    unsafe class AsmThreadFactory : BlurThreadFactory
    {   
        public int* KernelP { get; private set; }

        public AsmThreadFactory(double kernelSD) : base(kernelSD)
        {
            ImageKernel = new ImageProc.AsmKernel(kernelSD);
            UsedKernel = ImageProc.Kernel.Type.NormalizedFixed;
        }

        public override BlurThread Create(int start, int end)
        {   
            return new AsmThread(Task, start, end);
        }

        public override void Init(BlurTask task, byte* helperP, void* kernelP)
        {
            base.Init(task, helperP, kernelP);

            KernelP = (int*)kernelP;
            
            AsmLib.Init(task.Data.Data, helperP, task.Data.Stride,
                task.Data.Height, kernelP);
        }
    }
}
