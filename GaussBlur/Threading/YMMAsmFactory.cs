using System;
using System.Collections.Generic;
using System.Text;

namespace GaussBlur.Threading
{
    class YMMAsmFactory : AsmFactory
    {
        public YMMAsmFactory(double kernelSD) : base(kernelSD)
        {
            BlurX = Libraries.AsmLib.BlurX_YMM;
            ImageKernel = new ImageProc.AsmKernelLong(kernelSD);
        }
    }
}
