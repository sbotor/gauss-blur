using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace GaussBlur
{
    abstract unsafe class BlurThread
    {
        protected readonly double* data;
        protected readonly double* helper;
        protected readonly int start;
        protected readonly int end;
        protected readonly int stride;
        protected readonly int height;
        protected readonly double* kernel;
        protected readonly int kernelSize;
        protected readonly BlurThreadFactory parentFactory;
        protected readonly Thread thread;

        protected BlurThread(BlurThreadFactory parent, double* dataP,
            double* helperP, int startPos, int endPos, int imageStride,
            int imageHeight, double* kernelP, int kernSize)
        {
            parentFactory = parent;
            data = dataP;
            helper = helperP;
            stride = imageStride;
            height = imageHeight;
            kernel = kernelP;
            kernelSize = kernSize;
            start = startPos;
            end = endPos;

            thread = new Thread(Run);
        }

        public abstract void Run();

        public void Start()
        {
            thread.Start();
        }

        public void Join()
        {
            thread.Join();
        }
    }
}
