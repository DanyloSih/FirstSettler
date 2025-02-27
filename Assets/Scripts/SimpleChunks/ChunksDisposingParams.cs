﻿using System;

namespace SimpleChunks
{
    [Serializable]
    public struct ChunksDisposingParams
    {
        public int BatchLength;
        public int BatchDelay;

        public ChunksDisposingParams(int batchLength, int batchDelay)
        {
            BatchLength = batchLength;
            BatchDelay = batchDelay;
        }
    }
}
