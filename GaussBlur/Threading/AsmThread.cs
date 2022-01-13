using System.Threading;
using GaussBlur.DLL;

namespace GaussBlur.Threading
{
    unsafe class AsmThread : BlurThread
    {
        public AsmThread(BlurTask task, int start, int end)
            : base(task, start, end)
        {
            CurrentThread = new Thread(Run);
        }
        
        protected override void Run()
        {
            for (int i = 0; i < Task.TotalRepeats; i++)
            {
                AsmLib.BlurX(StartPos, EndPos);

                if (CheckIfCanceled())
                {
                    return;
                }
                SignalAndWait();

                //AsmLib.BlurY(StartPos, EndPos);

                if (CheckIfCanceled())
                {
                    return;
                }
                SignalAndWait();
            }
        }
    }
}
