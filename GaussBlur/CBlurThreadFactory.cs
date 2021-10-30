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

        public override BlurThread CreateThread(byte* dataP,
            byte* helperP, int startPos, int endPos, int imageStride,
            int imageHeight, double* kernelP)
        {   
            BlurThread newItem = new CBlurThread(this, dataP, helperP,
                startPos, endPos, imageStride,
                imageHeight, kernelP);

            Items.Add(newItem);

            return newItem;
        }
    }
}
