using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace GaussBlur
{
    abstract unsafe class BlurThread
    {
        //protected readonly double* src;
        //protected readonly double* dest;
        protected readonly int start;
        protected readonly int end;
        //protected readonly int height;
        //protected readonly int stride;
        //protected readonly double* kernel;
        //protected readonly int kernelSize;
        protected BlurThreadFactory factory;

        //protected CountdownEvent blurXFinished;

        //protected BlurThread(double* srcArrayP, double* destArrayP,
        //    int startPos, int endPos, int imageStride,
        //    int imageHeight, double* kernP, int kernSize,
        //    CountdownEvent cde)
        //{
        //    src = srcArrayP;
        //    dest = destArrayP;
        //    start = startPos;
        //    end = endPos;
        //    stride = imageStride;
        //    height = imageHeight;
        //    kernel = kernP;
        //    kernelSize = kernSize;
        //    blurXFinished = cde;
        //}

        protected BlurThread(BlurThreadFactory parent, int startPos, int endPos)
        {
            factory = parent;
            start = startPos;
            end = endPos;
        }
    }
}
