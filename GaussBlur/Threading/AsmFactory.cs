using System;
using System.Collections.Generic;
using System.Text;

namespace GaussBlur.Threading
{
    class AsmFactory : BaseAsmFactory
    {
        public AsmFactory(double kernelSD) : base(kernelSD)
        {
            BlurX = Libraries.AsmLib.BlurX;
            BlurY = Libraries.AsmLib.BlurY;

            KernelType = ImageProc.Kernel.Type.NormalizedFloat;
            ImageKernel = new ImageProc.AsmKernel(KernelSD);
        }
    }
}
