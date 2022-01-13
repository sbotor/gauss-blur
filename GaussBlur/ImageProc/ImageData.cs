using System;

namespace GaussBlur.ImageProc
{
    public unsafe class ImageData
    {
        public byte* Data { get; private set; }

        public int Width { get; private set; }

        public int Height { get; private set; }

        public int Stride { get; private set; }

        public int Length { get; private set; }

        public ImageData(System.Drawing.Imaging.BitmapData imageData)
        {
            Width = imageData.Width;
            Height = imageData.Height;
            Stride = imageData.Stride;
            Length = Math.Abs(Stride) * Height;

            Data = (byte*)imageData.Scan0.ToPointer();
        }

        public int[] Slice(int n)
        {
            if (n == 1)
            {
                return new int[2] { 0, Length };
            }
            else if (n > 1)
            {
                int padding = Width % 4,
                    rightIntervalEndp = 0,
                    chunkSize = (Width * Height) / n,
                    remainder = (Width * Height) % n;
                int[] indexes = new int[n + 1];
                indexes[0] = 0;

                for (int i = 1; i <= n; i++)
                {
                    rightIntervalEndp += chunkSize;
                    if (remainder > 0)
                    {
                        rightIntervalEndp++;
                        remainder--;
                    }

                    indexes[i] = rightIntervalEndp * 3 + (rightIntervalEndp / Width) * padding;
                }

                return indexes;
            }
            else
            {
                throw new ArgumentOutOfRangeException("Too few slices.");
            }
        }
    }
}
