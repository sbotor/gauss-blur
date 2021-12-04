using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace GaussBlur
{
    abstract class BlurThread
    {
        public int StartPos { get; protected set; }

        public int EndPos { get; protected set; }
        
        public Thread CurrentThread { get; protected set; }

        public BlurTask Task { get; protected set; }
        
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
