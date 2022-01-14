using System;
using System.Collections.Generic;
using System.Text;

using GaussBlur.Libraries;

namespace GaussBlur.Threading
{


    unsafe class AsmThreadFactory : YMMAsmThreadFactory
    {
        public AsmThreadFactory(double kernelSD) : base(kernelSD)
        {
        }
    }
}
