using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace GaussBlur
{
    abstract unsafe class BlurThreadFactory
    {
        public CountdownEvent BlurXFinished { get; private set; }
        public double* Data { get; private set; }
        public double* Help { get; private set; }
        public int ImageStride { get; private set; }
        public int ImageHeight { get; private set; }
        public double* KernelP { get; private set; }
        public int KernelSize { get; private set; }

        public BlurThreadFactory(double* dataArrP,
            double* helpArrP, int imageStride,
            int imageHeight, double* kernP, int kernSize,
            int threadCount)
        {
            Data = dataArrP;
            Help = helpArrP;
            ImageStride = imageStride;
            ImageHeight = imageHeight;
            KernelP = kernP;
            KernelSize = kernSize;
            BlurXFinished = new CountdownEvent(threadCount);
        }
        
        public abstract IBlurThread CreateThread(int start, int end);
    }
}
