using System;
using System.Collections.Generic;
using System.Text;

namespace GaussBlur.ImageProc
{
    class Kernel
    {
        public double SD { get; protected set; }

        public float[] Normalized { get; protected set; }

        public float[] Raw { get; protected set; }

        public int[] NormalizedFixed { get; protected set; }

        public int[] RawFixed { get; protected set; }

        public float Sum { get; protected set; }

        public int SumFixed { get; protected set; }

        public int Precision { get; protected set; }

        public Kernel(double sd, int precision)
        {
            SD = sd;
            Precision = precision;

            createFloatKernel();
            calculateSum();

            createFixedKernel();
            calculateFixedSum();

            calculateNormalized();
        }

        public Kernel(double sd) : this(sd, 24)
        {}

        protected float[] getKernelVals()
        {
            float[] kernel = new float[3];
            double variance = SD * SD,
                constance = 1 / (Math.Sqrt(2.0 * Math.PI) * SD);

            kernel[2] = (float)(constance * Math.Exp(-2 / variance));
            kernel[1] = (float)(constance * Math.Exp(-0.5 / variance));
            kernel[0] = (float)constance;

            return kernel;
        }

        protected virtual void createFloatKernel()
        {
            Raw = getKernelVals();
        }

        private void calculateSum()
        {
            float[] kernelVals = getKernelVals();
            Sum = kernelVals[0] + kernelVals[1] * 2 + kernelVals[2] * 2;
        }

        private void calculateNormalized()
        {
            Normalized = new float[Raw.Length];
            NormalizedFixed = new int[RawFixed.Length];

            for (int i = 0; i < Raw.Length; i++)
            {
                Normalized[i] = Raw[i] / Sum;
                NormalizedFixed[i] = FloatToFixed(Normalized[i], Precision);
            }
        }

        private void createFixedKernel()
        {
            int len = Raw.Length;

            RawFixed = new int[len];

            for (int i = 0; i < len; i++)
            {
                RawFixed[i] = FloatToFixed(Raw[i], Precision);
            }
        }

        private void calculateFixedSum()
        {
            SumFixed = FloatToFixed(Sum);
        }

        public static int FloatToFixed(float value, int prec)
        {
            if (value < 0)
            {
                throw new ArgumentException("Float value must be non-negative.");
            }

            if (prec < 0 || prec > 32)
            {
                throw new ArgumentException($"Invalid precision ({prec} not in range <0, 32>).");
            }

            return (int)(value * Math.Pow(2, prec) + 0.5f);
        }

        public static int FloatToFixed(float value)
        {
            return FloatToFixed(value, 24);
        }
    }
}
