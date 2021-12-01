using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace GaussBlur
{
    class BlurThreadSynchronizer
    {
        public Barrier TaskBarrier { get; private set; }

        public int TotalRepeats { get; private set; }

        public long RepeatsDone{ get => TaskBarrier.CurrentPhaseNumber / 2; }

        public int ThreadCount { get; private set; }

        public BlurThreadSynchronizer(int threadCount, int repeats)
        {
            if (repeats < 1 || threadCount < 1)
                throw new ArgumentException();

            ThreadCount = threadCount;
            TotalRepeats = repeats;
            Reset();
        }

        public void SignalAndWait()
        {
            TaskBarrier.SignalAndWait();
        }

        public void Reset()
        {
            TaskBarrier = new Barrier(ThreadCount, (b) =>
            {
                if (RepeatsDone > TotalRepeats)
                    throw new BarrierPostPhaseException();
            });
        }
    }
}
