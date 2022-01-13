using System;
using System.Collections.Generic;
using System.Text;

namespace GaussBlur.ImageProc
{
    class AsmKernel : Kernel
    {
        public AsmKernel(double sd) : base(sd)
        { }

        protected override void createFloatKernel()
        {
            float[] kernelVals = getKernelVals();
            Raw = new float[17];

            Raw[0] = Raw[1] = Raw[2] =
                Raw[12] = Raw[13] = Raw[14] = kernelVals[2];

            Raw[3] = Raw[4] = Raw[5] = 
                Raw[9] = Raw[10] = Raw[11] = kernelVals[1];

            Raw[6] = Raw[7] = Raw[8] = kernelVals[0];
        }
    }
}
