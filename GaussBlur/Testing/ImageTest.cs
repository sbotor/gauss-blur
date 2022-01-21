using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.IO;

using GaussBlur.ImageProc;
using GaussBlur.Threading;
using System.Collections;

namespace GaussBlur.Testing
{
    class ImageTest
    {
        public ImageContainer Image { get; protected set; }

        public int ImageHeight { get; private set; }

        public int ImageWidth { get; private set; }

        public int[] ThreadCounts { get; set; }

        public int RepeatCount { get; set; }

        public int TestCount { get; set; }

        public List<TestResult> Results { get; protected set; }

        public double KernelSD { get; set; }

        public string DefaultOutputFilename
        {
            get
            {
                string prefix = "test_";
                if (testedC)
                {
                    prefix += "C_";
                }
                if (testedAsm)
                {
                    prefix += "Asm_";
                }
                if (testedYMM)
                {
                    prefix += "YMM_";
                }

                string now = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
                return $"{prefix}X{TestCount}_R{RepeatCount}_{ImageWidth}x{ImageHeight}_{now}.csv";
            }
        }

        private bool testedC;

        private bool testedAsm;

        private bool testedYMM;

        public ImageTest(string filename, int testCount, int[] threadCounts, int repeatCount)
        {
            TestCount = testCount;
            ThreadCounts = threadCounts;
            RepeatCount = repeatCount;

            Image = new ImageContainer();
            Image.LoadImage(filename);
            
            Image.LockImage();
            if (Image.ImageData != null)
            {
                ImageHeight = Image.ImageData.Height;
                ImageWidth = Image.ImageData.Width;
            }
            else
            {
                ImageHeight = -1;
                ImageWidth = -1;
            }
            Image.UnlockImage();

            Clear();
        }

        public ImageTest(string filename, int testCount, int threadCount, int repeatCount)
            : this(filename, testCount, new int[] { threadCount }, repeatCount)
        { }

        public ImageTest(string filename, int testCount) : this(filename, testCount, 1, 1)
        { }

        public ImageTest(string filename) : this(filename, 1, 1, 1)
        { }

        public List<TestResult> TestC()
        {
            testedC = true;
            return test(new CFactory(KernelSD), "C");
        }

        public List<TestResult> TestAsm()
        {
            testedAsm = true;
            return test(new AsmFactory(KernelSD), "Asm");
        }

        public List<TestResult> TestYMM()
        {
            testedYMM = true;
            return test(new YMMAsmFactory(KernelSD), "YMM");
        }
        
        public void Clear()
        {
            Results = new List<TestResult>(TestCount);

            testedC = false;
            testedAsm = false;
            testedYMM = false;
        }

        public List<TestResult> test(ThreadFactory factory, string type)
        {
            List<TestResult> results = new List<TestResult>(TestCount);

            if (Image.LockImage())
            {
                foreach (int threads in ThreadCounts)
                {
                    BlurTask task = new BlurTask(Image.ImageData, threads, RepeatCount);

                    for (int i = 0; i < TestCount; i++)
                    {
                        task.Clear();
                        task.Run(factory);

                        TimeSpan time = task.RuntimeStopwatch.Elapsed;
                        results.Add(new TestResult(time, type, this, threads));
                    }
                }
            }
            Image.UnlockImage();

            Results.Capacity += results.Count;
            Results.AddRange(results);

            return results;
        }

        public string ToCSV(char separator)
        {
            if (Results == null)
            {
                return "";
            }

            StringBuilder builder = new StringBuilder();
            builder.Append(TestResult.GetCSVHeader(separator)).Append("\n");

            foreach (TestResult result in Results)
            {
                builder.Append(result.ToCSV(separator)).Append("\n");
            }

            return builder.ToString();
        }

        public string ToCSV()
        {
            return ToCSV(',');
        }
    }
}
