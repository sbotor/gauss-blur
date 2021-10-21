using System;
using System.Collections.Generic;
using System.Text;

namespace GaussBlur
{
    class GaussKernel
    {
        private double[] data;
        public double[] Data
        {
            get => data;
        }

        private double stdDev;
        public double StdDev
        {
            get => stdDev;
        }

        public int Size
        {
            get => data.Length;
        }

        public GaussKernel(int kernelSize, double sd)
        {
            data = new double[kernelSize];
            stdDev = sd;

            int maxCenterOffset = (kernelSize - 1) / 2;
            double variance = sd * sd,
                constance = 1 / (Math.Sqrt(2.0 * Math.PI * variance)),
                kernelSum = 0;

            for (int offset = -maxCenterOffset; offset <= maxCenterOffset; offset++)
            {
                double gaussValue = constance * Math.Exp(-(offset * offset) / (2 * variance));

                data[offset + maxCenterOffset] = gaussValue;
                kernelSum += gaussValue;
            }

            for (int offset = -maxCenterOffset; offset <= maxCenterOffset; offset++)
            {
                data[offset + maxCenterOffset] /= kernelSum;
            }
        }
    }
}
