using System;
using System.Collections.Generic;
using System.Text;

namespace GaussBlur.Threading
{
    class XMMAsmFactory : BaseAsmFactory
    {
        public bool AddDword { get; protected set; }

        public XMMAsmFactory(double kernelSD, bool addDword) : base(kernelSD)
        {
            AddDword = addDword;

            if (AddDword)
            {
                BlurX = Libraries.AsmLib.BlurXAddDword;
                BlurY = Libraries.AsmLib.BlurYAddDword;
            }
            else
            {
                BlurX = Libraries.AsmLib.BlurX;
                BlurY = Libraries.AsmLib.BlurY;
            }

            KernelType = ImageProc.Kernel.Type.NormalizedFloat;
            ImageKernel = new ImageProc.AsmKernel(KernelSD);
        }

        public XMMAsmFactory(double kernelSd) : this(kernelSd, false)
        {}
    }
}
