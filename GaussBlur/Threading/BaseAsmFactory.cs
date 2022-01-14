using System;
using System.Collections.Generic;
using System.Text;

using GaussBlur.Libraries;

namespace GaussBlur.Threading
{
    abstract unsafe class BaseAsmFactory : ThreadFactory
    {
        public int* KernelP { get; private set; }

        public Action<long, long>? BlurX { get; protected set; }
        
        public Action<long, long>? BlurY { get; protected set; }

        public BaseAsmFactory(double kernelSD) : base(kernelSD)
        {
            BlurX = null;
            BlurY = null;
        }

        public override BlurThread Create(int start, int end)
        {
            return new AsmThread(Task, this, start, end);
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
