using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace GaussBlur.Testing
{
    class TestParser
    {
        public int Start { get; set; }

        public string[] Args { get; set; }
        
        public bool Valid { get; private set; }

        public bool TestC { get; private set; }

        public bool TestAsm { get; private set; }

        public string InpDir { get; private set; }

        public string OutDir { get; private set; }

        public int ThreadCount { get; private set; }

        public int RepeatCount { get; private set; }

        public int TestCount { get; private set; }

        public string Message { get; private set; }

        public TestParser(string[] args, int start)
        {
            Start = start;
            Args = args;

            Reset();
        }

        private int i;

        public TestParser(string[] args) : this(args, 0)
        {}

        public void Reset()
        {
            i = -1;
            ThreadCount = -1;
            RepeatCount = -1;
            TestCount = -1;

            Valid = false;

            TestC = false;
            TestAsm = false;

            InpDir = "";
            OutDir = "";

            Message = "";
        }
        
        private bool hasNext()
        {
            return Args.Length > i - 1;
        }

        private bool parseArg()
        {
            string arg = Args[i];
            int n;
            
            switch (arg)
            {
                case "-o":
                case "--out":
                    if (hasNext() && OutDir == "")
                    {
                        OutDir = Args[++i];
                    }
                    else
                    {
                        Message = "Invalid output file parameter.";
                        return false;
                    }
                    break;

                case "-C":
                    TestAsm = true;
                    break;

                case "-A":
                    TestC = true;
                    break;

                case "-B":
                    TestAsm = true;
                    TestC = true;
                    break;

                case "-t":
                case "--threads":
                    if (hasNext() && int.TryParse(Args[++i], out n))
                    {
                        ThreadCount = n;
                    }
                    else
                    {
                        Message = $"Invalid thread count parameter \"{Args[i - 1]}\".";
                        return false;
                    }
                    break;

                case "-c":
                case "--count":
                    if (hasNext() && int.TryParse(Args[++i], out n))
                    {
                        TestCount = n;
                    }
                    else
                    {
                        Message = $"Invalid test count parameter \"{Args[i - 1]}\".";
                        return false;
                    }
                    break;

                case "-r":
                case "--repeats":
                    if (hasNext() && int.TryParse(Args[++i], out n))
                    {
                        RepeatCount = n;
                    }
                    else
                    {
                        Message = $"Invalid repeat count parameter \"{Args[i - 1]}\".";
                        return false;
                    }
                    break;

                default:
                    Message = $"Unrecognized parameter \"{Args[i]}\".";
                    return false;
            }

            return true;
        }

        public bool Parse()
        {
            if (hasNext())
            {
                InpDir = Args[Start + 1];
                if (File.Exists(InpDir))
                {
                    for (i = Start + 2; i < Args.Length; i++)
                    {
                        Valid = parseArg();

                        if (!Valid)
                        {
                            break;
                        }
                    }

                    if (!Valid)
                    {
                        return false;
                    }

                    if (!TestC && !TestAsm)
                    {
                        TestC = true;
                        TestAsm = true;
                    }

                    return true;
                }

                Message = $"Input file \"{InpDir}\" does not exist.";
                return false;
            }

            Message = "No input file specified.";
            return false;
        }

        public string GetInfo()
        {
            StringBuilder builder = new StringBuilder();

            builder.Append("Libraries: ");
            if (TestC && TestAsm)
            {
                builder.Append("C, Asm");
            }
            else if (TestC)
            {
                builder.Append("C");
            }
            else
            {
                builder.Append("Asm");
            }
            builder.Append("\n");

            builder.Append($"Total tests: {RepeatCount} repeat(s), {TestCount} time(s).\n");
            builder.Append($"Threads: {ThreadCount}\n");

            return builder.ToString();
        }
    }
}
