using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;

using GaussBlur.Testing;

namespace GaussBlur
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        [DllImport("Kernel32")]
        public static extern void AllocConsole();

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            string[] args = e.Args;
            int testIndex = Array.IndexOf(args, "test");
            if (testIndex != -1)
            {
                try
                {
                    AllocConsole();

                    TestParser parser = new TestParser(args, testIndex);
                    if (!parser.Parse())
                    {
                        Console.WriteLine($"ERROR: Invalid parameters.\n{parser.Message}");
                        return;
                    }

                    Console.WriteLine("Preparing tests.");
                    Console.WriteLine(parser.GetInfo() + "\n");
                    ImageTest test = new ImageTest(parser.InpDir, parser.TestCount,
                        parser.ThreadCounts, parser.RepeatCount);

                    if (parser.TestC)
                    {
                        Console.WriteLine("--- Testing C...");
                        test.TestC();
                        Console.WriteLine("Done.\n");
                    }

                    if (parser.TestAsm)
                    {
                        Console.WriteLine("--- Testing Asm...");
                        test.TestAsm();
                        Console.WriteLine("Done.\n");
                    }

                    string outDir = parser.OutDir;
                    if (outDir == "")
                    {
                        outDir = test.DefaultOutputFilename;
                    }
                    else if (Directory.Exists(outDir))
                    {
                        if (File.GetAttributes(outDir).HasFlag(FileAttributes.Directory))
                        {
                            outDir = Path.Combine(outDir, test.DefaultOutputFilename);
                        }
                    }

                    File.WriteAllText(outDir, test.ToCSV());
                    Console.WriteLine($"--- All tests done. Results saved to {outDir}.");

                }
                catch (Exception exc)
                {
                    Console.WriteLine($"Exception occurred during testing: \"{exc.Message}\".");
                }

                return;
            }

            new GUI.MainWindow().Show();
        }

        

        
    }
}
