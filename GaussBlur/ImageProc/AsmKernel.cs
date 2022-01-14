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
            Float = new float[17];

            Float[0] = Float[1] = Float[2] =
                Float[12] = Float[13] = Float[14] = kernelVals[2];

            Float[3] = Float[4] = Float[5] = 
                Float[9] = Float[10] = Float[11] = kernelVals[1];

            Float[6] = Float[7] = Float[8] = kernelVals[0];
        }
    }
}
