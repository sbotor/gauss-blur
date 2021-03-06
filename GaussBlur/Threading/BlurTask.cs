using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading;

using GaussBlur.ImageProc;

namespace GaussBlur.Threading
{
    class BlurTask : BlurThreadSynchronizer
    {
        public List<BlurThread> Threads { get; private set; }

        public ImageData Data { get; private set; }

        public BackgroundWorker? Worker { get; private set; }

        public Stopwatch RuntimeStopwatch { get; private set; }

        public bool Finished { get => RepeatsDone == TotalRepeats; }

        private Mutex mutex;

        public BlurTask(System.Drawing.Imaging.BitmapData data, int threadCount, int repeats)
            : base(threadCount, repeats)
        {
            Data = new ImageData(data);

            Threads = new List<BlurThread>();

            mutex = new Mutex();
        }
        
        ~BlurTask()
        {
            if (Worker != null)
            {
                Worker.Dispose();
            }

            mutex.Dispose();

            foreach (BlurThread thread in Threads)
            {
                if (thread.BoundThread.IsAlive)
                {
                    thread.Join();
                }
            }
        }

        public void Clear()
        {
            Reset();
            Threads.Clear();
        }

        public override void SignalAndWait()
        {
            mutex.WaitOne();
            if (Worker != null)
            {
                Worker.ReportProgress(PercentDoneInclusive());
            }
            mutex.ReleaseMutex();

            base.SignalAndWait();
        }

        public void RunWorker(ThreadFactory factory, GUI.ProgressWindow progWindow)
        {
            if (Worker != null)
            {
                Worker.Dispose();
                Worker = null;
            }

            Worker = new BackgroundWorker();
            Worker.WorkerReportsProgress = true;
            Worker.WorkerSupportsCancellation = true;

            Worker.DoWork += new DoWorkEventHandler(
                delegate (object o, DoWorkEventArgs args)
                {
                    Run(args.Argument as ThreadFactory);
                });

            Worker.ProgressChanged += new ProgressChangedEventHandler(
                delegate (object o, ProgressChangedEventArgs args)
                {
                    progWindow.progressStatus.Value = args.ProgressPercentage;
                });

            Worker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(
                delegate (object o, RunWorkerCompletedEventArgs args)
                {
                    progWindow.progressStatus.Value = 100;
                    progWindow.Close();
                });

            Worker.RunWorkerAsync(factory);
        }
        
        public void Run(ThreadFactory factory)
        {
            unsafe
            {
                switch (factory.KernelType)
                {
                    case Kernel.Type.Float:
                        {
                            fixed (void* kernelP = factory.ImageKernel.Float)
                            {
                                runWithKernel(factory, kernelP);
                            }
                            break;
                        }

                    case Kernel.Type.NormalizedFloat:
                        {
                            fixed (void* kernelP = factory.ImageKernel.NormalizedFloat)
                            {
                                runWithKernel(factory, kernelP);
                            }
                            break;
                        }

                    case Kernel.Type.Fixed:
                        {
                            fixed (void* kernelP = factory.ImageKernel.Fixed)
                            {
                                runWithKernel(factory, kernelP);
                            }
                            break;
                        }

                    case Kernel.Type.NormalizedFixed:
                        {
                            fixed (void* kernelP = factory.ImageKernel.NormalizedFixed)
                            {
                                runWithKernel(factory, kernelP);
                            }
                            break;
                        }

                    case Kernel.Type.None:
                    default:
                        throw new ArgumentException("Unrecognized BlurFactory or kernel type unspecified.");
                }
            }
        }

        private unsafe void runWithKernel(ThreadFactory factory, void* kernelP)
        {
            Clear();
            int[] slices = Data.Slice(ThreadCount);
            byte[] helper = new byte[Data.Length];

            RuntimeStopwatch = new Stopwatch();
            RuntimeStopwatch.Start();

            fixed (byte* helperP = helper)
            {
                factory.Init(this, helperP, kernelP);

                for (int i = 0; i < ThreadCount; i++)
                {
                    BlurThread thread = factory.Create(slices[i], slices[i + 1]);
                    Threads.Add(thread);
                    thread.Start();
                }

                Threads.ForEach(t => t.Join());
            }

            RuntimeStopwatch.Stop();
        }
    }
}
