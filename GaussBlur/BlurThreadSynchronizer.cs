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

        public int RepeatsDone { get => (int)TaskBarrier.CurrentPhaseNumber / 2; }

        public int ThreadCount { get; private set; }

        protected int totalThreadRuns;

        public BlurThreadSynchronizer(int threadCount, int repeats)
        {
            if (repeats < 1 || threadCount < 1)
                throw new ArgumentException();

            ThreadCount = threadCount;
            TotalRepeats = repeats;

            totalThreadRuns = ThreadCount * TotalRepeats;
            
            Reset();
        }

        ~BlurThreadSynchronizer()
        {
            TaskBarrier.Dispose();
        }

        public virtual void SignalAndWait()
        {
            TaskBarrier.SignalAndWait();
        }

        public int PercentDoneInclusive()
        {
            return 100 * (ThreadCount - TaskBarrier.ParticipantsRemaining + RepeatsDone * ThreadCount) / totalThreadRuns;
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
