using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace GaussBlur.Threading
{
    abstract class BlurThread
    {
        public int StartPos { get; protected set; }

        public int EndPos { get; protected set; }
        
        public Thread BoundThread { get; protected set; }

        public BlurTask Task { get; protected set; }
        
        public BlurThread(BlurTask task, int start, int end)
        {
            StartPos = start;
            EndPos = end;
            Task = task;

            BoundThread = new Thread(run);
        }

        protected abstract void runX();
        protected abstract void runY();

        protected virtual void run()
        {
            for (int i = 0; i < Task.TotalRepeats; i++)
            {
                runX();

                if (CheckIfCanceled())
                {
                    return;
                }
                SignalAndWait();

                runY();

                if (CheckIfCanceled())
                {
                    return;
                }
                SignalAndWait();
            }
        }

        public virtual bool CheckIfCanceled()
        {
            if (Task.Worker != null)
            {
                return Task.Worker.CancellationPending;
            }

            return false;
        }

        public virtual void SignalAndWait()
        {
            Task.SignalAndWait();
        }

        public virtual void Start()
        {
            BoundThread.Start();
        }

        public virtual void Join()
        {
            BoundThread.Join();
        }
    }
}
