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
            
            CurrentThread = new Thread(Run);
        }

        protected override void Run()
        {
            for (int i = 0; i < Task.TotalRepeats; i++)
            {
                ImageData imageData = Task.Data;

                CLib.BlurX(
                    imageData.Data,
                    helper,
                    StartPos,
                    EndPos,
                    imageData.Stride,
                    imageData.Height,
                    kernel);

                if (CheckIfCanceled())
                {
                    return;
                }
                SignalAndWait();

                CLib.BlurY(
                    helper,
                    imageData.Data,
                    StartPos,
                    EndPos,
                    imageData.Stride,
                    imageData.Height,
                    kernel);

                if (CheckIfCanceled())
                {
                    return;
                }
                SignalAndWait();
            }
        }
    }
}
