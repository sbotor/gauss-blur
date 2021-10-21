using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace GaussBlur
{
    unsafe class ImgProcThread
    {
        private readonly double* srcArrP;
        private readonly double* destArrP;
        private readonly int start;
        private readonly int end;
        private readonly int stride;
        private readonly int height;
        private readonly double* kernelP;
        private readonly int kernelSize;

        private static CountdownEvent cde = new CountdownEvent(1);
        public static int ThreadCount
        {
            get => ThreadCount;
            
            set
            {
                cde = new CountdownEvent(value);
            }
        }

        public ImgProcThread(double* srcArrayP, double* destArrayP, int startPos, int endPos, int imageStride, int imageHeight, double* kernP, int kernSize)
        {
            srcArrP = srcArrayP;
            destArrP = destArrayP;
            start = startPos;
            end = endPos;
            stride = imageStride;
            height = imageHeight;
            kernelP = kernP;
            kernelSize = kernSize;
        }
        
        public void Run()
        {
            RunX();
            cde.Signal();
            cde.Wait();
            RunY();
        }
        
        public void RunX()
        {
            CLib.blurX(
                srcArrP,
                destArrP,
                start,
                end,
                stride,
                height,
                kernelP,
                kernelSize);
        }

        public void RunY()
        {
            CLib.blurY(
                destArrP,
                srcArrP,
                start,
                end,
                stride,
                height,
                kernelP,
                kernelSize);
        }
    }
}
