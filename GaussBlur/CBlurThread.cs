using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace GaussBlur
{
    unsafe class CBlurThread : BlurThread
    {   
        public CBlurThread(CBlurThreadFactory parent, byte* dataP,
            byte* helperP, int startPos, int endPos, int imageStride,
            int imageHeight, double* kernelP) :
            base(parent, dataP, helperP, startPos, endPos, imageStride, imageHeight,
                kernelP)
        { }

        public override void Run()
        {
            CLib.blurX(
                data, helper,
                start, end,
                stride, height,
                kernel);

            parentFactory.BlurXFinished.Signal();
            parentFactory.BlurXFinished.Wait();

            CLib.blurY(
                helper, data,
                start, end,
                stride, height,
                kernel);
        }
    }
}
