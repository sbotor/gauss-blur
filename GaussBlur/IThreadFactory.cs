﻿using System;
using System.Collections.Generic;
using System.Text;

namespace GaussBlur
{
    unsafe interface IThreadFactory
    {
        public BlurThread Create(BlurTask task, byte* helperP, double* kernelP, int start, int end);
    }
}