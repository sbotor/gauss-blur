using System;
using System.Collections.Generic;
using System.Text;

namespace GaussBlur.Threading
{
    class YMMAsmThreadFactory : BaseAsmThreadFactory
    {
        public YMMAsmThreadFactory(double kernelSD)
            : base(kernelSD)
        {
            BlurX = Libraries.AsmLib.BlurX;
            BlurY = Libraries.AsmLib.BlurY;
        }
    }
}
