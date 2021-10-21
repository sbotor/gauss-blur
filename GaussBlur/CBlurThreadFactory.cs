using System;
using System.Collections.Generic;
using System.Text;

namespace GaussBlur
{
    unsafe class CBlurThreadFactory : BlurThreadFactory
    {
        public CBlurThreadFactory(double* dataArrP,
            double* helpArrP, int imageStride,
            int imageHeight, double* kernP, int kernSize,
            int threadCount) :
            base(dataArrP, helpArrP, imageStride, imageHeight,
                kernP, kernSize, threadCount)
        { }

        public override IBlurThread CreateThread(int start, int end)
        {
            return new CBlurThread(this, start, end);
        }
    }
}
