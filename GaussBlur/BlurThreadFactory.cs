using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace GaussBlur
{
    abstract class BlurThreadFactory
    {
        public CountdownEvent BlurXFinished { get; private set; }

        public List<BlurThread> Items { get; private set; }

        public int ThreadCount { get; private set; }

        public BlurThreadFactory(int threadCount)
        {
            BlurXFinished = new CountdownEvent(threadCount);
            Items = new List<BlurThread>();
            ThreadCount = threadCount;
        }
        
        public unsafe abstract BlurThread CreateThread(double* dataP,
            double* helperP, int startPos, int endPos, int imageStride,
            int imageHeight, double* kernelP, int kernSize);
    }
}
