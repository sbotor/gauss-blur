using System;
using System.Collections.Generic;
using System.Text;

using GaussBlur.Libraries;

namespace GaussBlur.Threading
{
    unsafe class AsmFactory : XMMAsmFactory
    {
        public AsmFactory(double kernelSD) : base(kernelSD)
        {
        }
    }
}
