using System;
using System.Threading;
using GaussBlur.Libraries;

namespace GaussBlur.Threading
{
    unsafe class AsmThread : BlurThread
    {
        protected Action<long, long> blurX;
        protected Action<long, long> blurY;

        public AsmThread(BlurTask task, BaseAsmFactory factory, int start, int end)
            : base(task, start, end)
        {
            blurX = factory.BlurX;
            blurY = factory.BlurY;
        }

        protected override void runX()
        {
            blurX.Invoke(StartPos, EndPos);
        }

        protected override void runY()
        {
            blurY.Invoke(StartPos, EndPos);
        }
    }
}
