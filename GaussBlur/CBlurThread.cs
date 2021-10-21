using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace GaussBlur
{
    unsafe class CBlurThread : BlurThread, IBlurThread
    {
        //public CBlurThread(double* srcArrayP, double* destArrayP,
        //    int startPos, int endPos, int imageStride,
        //    int imageHeight, double* kernP, int kernSize,
        //    CountdownEvent cde) :
        //    base(srcArrayP, destArrayP,
        //    startPos, endPos, imageStride,
        //    imageHeight, kernP, kernSize, cde)
        //{}
        
        public CBlurThread(BlurThreadFactory parent, int startPos, int endPos) :
            base(parent, startPos, endPos)
        { }

        public void Run()
        {
            CLib.blurX(
                factory.Data,
                factory.Help,
                start,
                end,
                factory.ImageStride,
                factory.ImageHeight,
                factory.KernelP,
                factory.KernelSize);

            factory.BlurXFinished.Signal();
            factory.BlurXFinished.Wait();

            CLib.blurY(
                factory.Help,
                factory.Data,
                start,
                end,
                factory.ImageStride,
                factory.ImageHeight,
                factory.KernelP,
                factory.KernelSize);
        }
    }
}
