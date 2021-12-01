using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace GaussBlur
{
    unsafe class CThread : BlurThread
    {      
        public Thread CurrentThread { get; private set; }

        private byte* helper;

        private double* kernel;

        public CThread(BlurTask task, byte* helperP, double* kernelP, int start, int end)
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

                CLib.blurX(
                    imageData.Data,
                    helper,
                    StartPos,
                    EndPos,
                    imageData.Stride,
                    imageData.Height,
                    kernel);

                Task.SignalAndWait();

                CLib.blurY(
                    helper,
                    imageData.Data,
                    StartPos,
                    EndPos,
                    imageData.Stride,
                    imageData.Height,
                    kernel);

                Task.SignalAndWait();
            }
        }

        public override void Start()
        {
            CurrentThread.Start();
        }

        public override void Join()
        {
            CurrentThread.Join();
        }
    }
}
