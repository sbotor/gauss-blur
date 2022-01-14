using System;
using System.Threading;
using GaussBlur.Libraries;

namespace GaussBlur.Threading
{
    unsafe class AsmThread : BlurThread
    {
        public BaseAsmThreadFactory? Factory { get; protected set; }

        public AsmThread(BlurTask task, BaseAsmThreadFactory factory, int start, int end)
            : base(task, start, end)
        {
            Factory = factory;
        }
        
        protected override void Run()
        {
            Action<long, long>? x = null;
            Action<long, long>? y = null;

            if (Factory != null)
            {
                x = Factory.BlurX;
                y = Factory.BlurY;
            }

            for (int i = 0; i < Task.TotalRepeats; i++)
            {
                x?.Invoke(StartPos, EndPos);

                if (CheckIfCanceled())
                {
                    return;
                }
                SignalAndWait();

                y?.Invoke(StartPos, EndPos);

                if (CheckIfCanceled())
                {
                    return;
                }
                SignalAndWait();
            }
        }
    }
}
