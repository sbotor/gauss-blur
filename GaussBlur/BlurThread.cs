using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace GaussBlur
{
    abstract class BlurThread
    {
        public int StartPos { get; private set; }

        public int EndPos { get; private set; }
        
        public Thread CurrentThread { get; private set; }

        public BlurTask Task { get; private set; }
        
        public BlurThread(BlurTask task, int start, int end)
        {
            StartPos = start;
            EndPos = end;
            Task = task;

            CurrentThread = new Thread(Run);
        }

        protected abstract void Run();

        public abstract void Start();

        public abstract void Join();
    }
}
