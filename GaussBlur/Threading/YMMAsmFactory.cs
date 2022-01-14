using System;
using System.Collections.Generic;
using System.Text;

namespace GaussBlur.Threading
{
    class YMMAsmFactory : XMMAsmFactory
    {
        public YMMAsmFactory(double kernelSD, bool addDword)
            : base(kernelSD, addDword)
        {
            if (AddDword)
            {
                BlurX = Libraries.AsmLib.BlurXAddDword_YMM;
            }
            else
            {
                BlurX = Libraries.AsmLib.BlurX_YMM;
            }

            KernelType = ImageProc.Kernel.Type.NormalizedFloat;
            ImageKernel = new ImageProc.AsmKernelLong(kernelSD);
        }

        public YMMAsmFactory(double kernelSD) : this(kernelSD, false)
        { }
    }
}
