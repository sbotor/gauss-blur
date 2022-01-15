using System;
using System.Threading;
using GaussBlur.Libraries;

namespace GaussBlur.Threading
{
    unsafe class AsmThread : BlurThread
    {
        public BaseAsmFactory? Factory { get; protected set; }

        protected Action<long, long> blurX;
        protected Action<long, long> blurY;

        public AsmThread(BlurTask task, BaseAsmFactory factory, int start, int end)
            : base(task, start, end)
        {
            blurX = Factory.BlurX;
            blurY = Factory.BlurY;
        }

        protected override void runX()
        {
            Factory?.BlurX?.Invoke(StartPos, EndPos);
        }

        protected override void runY()
        {
            Factory?.BlurY?.Invoke(StartPos, EndPos);
        }
    }
}
