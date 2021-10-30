using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Runtime.InteropServices;
using System.IO;

namespace GaussBlur
{
    public unsafe class RGBArray
    {
        public byte* Data { get; private set; }

        public int Width { get; private set; }

        public int Height { get; private set; }
        public int Stride { get; private set; }

        public int Length { get; private set; }

        public RGBArray(System.Drawing.Imaging.BitmapData imageData)
        {
            Width = imageData.Width;
            Height = imageData.Height;
            Stride = imageData.Stride;
            Length = Math.Abs(Stride) * Height;

            Data = (byte*)imageData.Scan0.ToPointer();
        }

        //private RGBArray(Bitmap image)
        //{
        //    Rectangle rect = new Rectangle(0, 0, image.Width, image.Height);
        //    System.Drawing.Imaging.BitmapData imageData = image.LockBits(
        //        rect, System.Drawing.Imaging.ImageLockMode.ReadOnly, image.PixelFormat);

        //    width = imageData.Width;
        //    height = imageData.Height;
        //    stride = imageData.Stride;

        //    int length = Math.Abs(imageData.Stride) * imageData.Height;
        //    byte[] bytes = new byte[length];

        //    Marshal.Copy(imageData.Scan0, bytes, 0, length);

        //    data = new double[length];
        //    Array.Copy(bytes, data, length);

        //    image.UnlockBits(imageData);
        //}

        //public byte[] ToByteArray()
        //{
        //    byte[] arr = System.Linq.Enumerable.ToArray(System.Linq.Enumerable.Select(data, Convert.ToByte));

        //    return arr;
        //}

        public int[] Slice(int n)
        {
            if (n == 1)
            {
                return new int[2] { 0, Length };
            }
            else if (n > 1 && n <= 64)
            {
                int fill = Width % 4,
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

                    indexes[i] = rightIntervalEndp * 3 + (rightIntervalEndp / Width) * fill;
                }

                return indexes;
            }
            else
            {
                throw new ArgumentOutOfRangeException("To few slices.");
            }
        }
    }
}
