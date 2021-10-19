using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Runtime.InteropServices;
using System.IO;

namespace GaussBlur
{
    class RGBArray
    {
        private byte[] data;
        public byte[] Data
        {
            get { return data; }
        }

        private int width;
        public int Width
        {
            get => width;
        }

        private int height;
        public int Height
        {
            get => height;
        }

        private int stride;
        public int Stride
        {
            get => stride;
        }

        public int Length
        {
            get => data.Length;
        }

        public RGBArray(System.Drawing.Imaging.BitmapData imageData)
        {
            width = imageData.Width;
            height = imageData.Height;
            stride = imageData.Stride;

            int length = Math.Abs(imageData.Stride) * imageData.Height;
            data = new byte[length];

            Marshal.Copy(imageData.Scan0, data, 0, length);
        }

        public float[] ToFloat()
        {
            float[] converted = new float[Length];
            Array.Copy(data, converted, Length);
            
            return converted;
        }

        public void SaveB(string fileDir)
        {
            using (FileStream fs = File.Open(fileDir, FileMode.OpenOrCreate, FileAccess.Write))
            {
                StreamWriter sw = new StreamWriter(fs);
                Array.ForEach(data, (n) => sw.WriteLine(n));
            }
        }
    }
}
