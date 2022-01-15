using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

using GaussBlur.ImageProc;
using GaussBlur.Libraries;

namespace GaussBlur.Threading
{
    unsafe class CThread : BlurThread
    {      
        private byte* helper;

        private float* kernel;

        public CThread(BlurTask task, byte* helperP, float* kernelP, int start, int end)
            : base(task, start, end)
        {
            helper = helperP;
            kernel = kernelP;
            
            BoundThread = new Thread(run);
        }

        protected override void runX()
        {
            CLib.BlurX(
                    Task.Data.Data,
                    helper,
                    StartPos,
                    EndPos,
                    Task.Data.Stride,
                    Task.Data.Height,
                    kernel);
        }

        protected override void runY()
        {
            CLib.BlurY(
                    helper,
                    Task.Data.Data,
                    StartPos,
                    EndPos,
                    Task.Data.Stride,
                    Task.Data.Height,
                    kernel);

        }
    }
}
