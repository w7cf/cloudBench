// Copyright (c) Microsoft Corporation.  All rights reserved.
namespace BenchLib
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    public class RandomBlobData
    {
        const int headroom = 10000;
        readonly long size;
        readonly byte[] randomData;
        Random randomizer = new Random((int)DateTime.Now.Ticks & 0x0000FFFF);

        public long Size { get { return this.size; } }

        public RandomBlobData(long minSize)
        {
            this.size = minSize;
            long totalLength = minSize + headroom;
            this.randomData = new byte[totalLength];
            for (int i = 0; i < totalLength; i++)
            {
                this.randomData[i] = (byte)this.randomizer.Next(0x100);
            }
        }

        public byte[] GetBlob()
        {
            byte[] data = new byte[this.size];
            Array.Copy(this.randomData, this.randomizer.Next(headroom), data, 0, this.size);
            return data;
        }
    }
}
