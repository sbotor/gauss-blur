using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;

namespace GaussBlur.Testing
{
    class TestParser
    {
        public int Start { get; set; }

        public string[] Args { get; set; }
        
        public bool Valid { get; private set; }

        public bool TestC { get; private set; }

        public bool TestAsm { get; private set; }

        public bool TestYMM { get; private set; }

        public string InpDir { get; private set; }

        public string OutDir { get; private set; }

        public int[] ThreadCounts { get; private set; }

        public int RepeatCount { get; private set; }

        public int TestCount { get; private set; }

        public string Message { get; private set; }

        private static Regex threadListRegex = new Regex(@"^(\d\d?(?:\s*,\s*\d\d?)*)$");

        private static Regex threadRangeRegex = new Regex(@"^(\d\d?)\s*-\s*(\d\d?)$");

        public static string HelpMessage
        {
            get
            {
                StringBuilder builder = new StringBuilder();

                builder.AppendLine("Usage:");
                builder.AppendLine("test <inputFile> [<testArgs>]\n\nTest arguments:");
                builder.AppendLine("[-o | --out <outputFile>]");
                builder.AppendLine("[<library>]");
                builder.AppendLine("[-t | --threads <threadCount>]");
                builder.AppendLine("[-c | --count <testCount>]");
                builder.AppendLine("[-r | --repeats <repeatCount>]");

                builder.AppendLine("\nLibrary options (one or more, defaults to '-A -C'): -[A]ssembly | -[C] | -[Y]MM");
                builder.AppendLine("Thread count options (numbers from 1 to 64): <number> | <start-end> (both inclusive) | <num1,num2,...>");

                return builder.ToString();
            }
        }

        public TestParser(string[] args, int start)
        {
            Start = start;
            Args = args;

            Reset();
        }

        private int i;

        public TestParser(string[] args) : this(args, 0)
        {}

        public static string Trim(string str)
        {
            return Regex.Replace(str, @"\s+", "");
        }

        public void Reset()
        {
            i = -1;
            ThreadCounts = new int[0];
            RepeatCount = -1;
            TestCount = -1;

            Valid = false;

            TestC = false;
            TestAsm = false;
            TestYMM = false;

            InpDir = "";
            OutDir = "";

            Message = "";
        }
        
        private bool hasNext()
        {
            return i + 1 < Args.Length;
        }

        private bool parseArg()
        {
            string arg = Args[i];
            
            switch (arg)
            {
                case "-o":
                case "--out":
                    if (!hasNext())
                    {
                        Message = "Invalid output file parameter.";
                        return false;
                    }
                    OutDir = Args[++i];
                    break;

                case "-A":
                    TestAsm = true;
                    break;

                case "-C":
                    TestC = true;
                    break;

                case "-Y":
                    TestYMM = true;
                    break;

                case "-t":
                case "--threads":
                    return parseThreads();

                case "-c":
                case "--count":
                    return parseTestCount();

                case "-r":
                case "--repeats":
                    return parseRepeats();

                default:
                    Message = $"Unrecognized parameter \"{Args[i]}\".";
                    return false;
            }

            return true;
        }

        private bool parseThreads()
        {
            if (ThreadCounts.Length > 0)
            {
                Message = "Duplicate thread count parameter.";
                return false;
            }
            if (!hasNext())
            {
                Message = "No thread count value specified.";
                return false;
            }

            string arg = Args[++i];
            int n;
            if (int.TryParse(arg, out n))
            {
                ThreadCounts = new int[] { n };
                return true;
            }

            Match match;

            match = threadListRegex.Match(arg);
            if (match.Success)
            {
                string valStr = match.Groups[1].Value;
                valStr = Trim(valStr);
                string[] values = valStr.Split(',');

                SortedSet<int> counts = new SortedSet<int>();
                for (int countIndex = 0; countIndex < values.Length; countIndex++)
                {
                    int num = int.Parse(values[countIndex]);
                    if (num < 0 || num > 64)
                    {
                        Message = $"Invalid thread count value {num}.";
                        return false;
                    }

                    counts.Add(num);
                }

                ThreadCounts = new int[counts.Count];
                int index = 0;
                foreach (int count in counts)
                {
                    ThreadCounts[index++] = count;
                }

                return true;
            }

            match = threadRangeRegex.Match(arg);
            if (match.Success)
            {
                string num1 = match.Groups[1].Value,
                    num2 = match.Groups[2].Value;

                int lower = int.Parse(num1), upper = int.Parse(num2);

                if (lower < 0 || upper > 64 || lower > upper)
                {
                    Message = $"Invalid thread count values [{lower}, {upper}].";
                    return false;
                }

                int count = upper - lower + 1, current = lower;
                ThreadCounts = new int[count];

                for (int i = 0; i < count; i++)
                {
                    ThreadCounts[i] = current++;
                }

                return true;
            }

            Message = $"Invalid thread count value {arg}.";
            return false;
        }

        private bool parseRepeats()
        {
            if (RepeatCount > 0)
            {
                Message = "Duplicate repeat count parameter.";
                return false;
            }
            if (!hasNext())
            {
                Message = "No repeat count value specified.";
                return false;
            }

            int n;
            if (!int.TryParse(Args[++i], out n) || n < 1)
            {
                Message = $"Invalid repeat count value {Args[i]}.";
                return false;
            }

            RepeatCount = n;
            return true;
        }

        private bool parseTestCount()
        {
            if (TestCount > 0)
            {
                Message = $"Duplicate test count parameter.";
                return false;
            }
            if (!hasNext())
            {
                Message = "No test count value specified.";
                return false;
            }

            int n;
            if (!int.TryParse(Args[++i], out n) || n < 1)
            {
                Message = $"Invalid test count value {Args[i]}.";
                return false;
            }

            TestCount = n;
            return true;
        }

        public bool Parse()
        {
            i = Start;
            
            if (hasNext())
            {
                InpDir = Args[i + 1];
                if (File.Exists(InpDir))
                {
                    Valid = true;
                    
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

                    checkAndSetDefaults();
                    return true;
                }

                Message = $"Input file \"{InpDir}\" does not exist.";
                return false;
            }

            Message = "No input file specified.";
            return false;
        }

        private void checkAndSetDefaults()
        {
            if (!TestC && !TestAsm && !TestYMM)
            {
                TestC = true;
                TestAsm = true;
            }

            if (ThreadCounts.Length < 1)
            {
                ThreadCounts = new int[] { 1 };
            }

            if (RepeatCount == -1)
            {
                RepeatCount = 1;
            }

            if (TestCount == -1)
            {
                TestCount = 1;
            }
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

            builder.Append($"Total tests: {RepeatCount} repeat(s), {TestCount} time(s).\nThreads: ");
            
            if (ThreadCounts.Length > 0)
            {
                builder.Append("[").Append(ThreadCounts[0]);
                for (int index = 1; index < ThreadCounts.Length; index++)
                {
                    builder.Append(", ").Append(ThreadCounts[index]);
                }

                builder.Append("]");
            }
            else
            {
                builder.Append("None");
            }
            builder.Append("\n");

            return builder.ToString();
        }
    }
}
