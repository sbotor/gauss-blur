using System;
using System.Collections.Generic;
using System.Text;

namespace GaussBlur.Testing
{
    class TestResult
    {
        public int ThreadCount { get; set; }

        public int ImageWidth { get; set; }

        public int ImageHeight { get; set; }

        public TimeSpan Time { get; set; }

        public string Type { get; set; }

        public ImageTest ParentTest { get; private set; }
        
        public TestResult(TimeSpan time, string type)
        {
            ThreadCount = ImageWidth = ImageHeight = 0;
            Time = time;
            Type = type;
        }

        public TestResult(TimeSpan time, string type, ImageTest test, int threadCount) : this(time, type)
        {
            ThreadCount = threadCount;
            ParentTest = test;
            ImageWidth = test.ImageWidth;
            ImageHeight = test.ImageHeight;
        }

        public static string GetCSVHeader(char separator)
        {
            StringBuilder builder = new StringBuilder();

            if (separator == ',')
            {
                builder.Append("\"Time, ms\"").Append(separator);
            }
            else
            {
                builder.Append("Time, ms").Append(separator);
            }

            builder.AppendJoin(separator,
                "Threads", "Type", "Width", "Height");
            
            return builder.ToString();
        }

        public static string GetCSVHeader()
        {
            return GetCSVHeader(',');
        }

        public string ToCSV(char separator)
        {
            StringBuilder builder = new StringBuilder();

            builder.AppendJoin(separator,
                Time.TotalMilliseconds, ThreadCount, Type, ImageWidth, ImageHeight);

            return builder.ToString();
        }

        public string ToCSV()
        {
            return ToCSV(',');
        }
    }
}
