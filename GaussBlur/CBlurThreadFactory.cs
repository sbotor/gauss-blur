using System;
using System.Collections.Generic;
using System.Text;

namespace GaussBlur
{
    unsafe class CBlurThreadFactory : BlurThreadFactory
    {
        public CBlurThreadFactory(int threadCount) :
            base(threadCount)
        { }

        public override BlurThread CreateThread(double* dataP,
            double* helperP, int startPos, int endPos, int imageStride,
            int imageHeight, double* kernelP, int kernSize)
        {   
            BlurThread newItem = new CBlurThread(this, dataP, helperP,
                startPos, endPos, imageStride,
                imageHeight, kernelP, kernSize);

            Items.Add(newItem);

            return newItem;
        }
    }
}
