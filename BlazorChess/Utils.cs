namespace ChessEngine
{
    static class utils
    {
        /// <summary>
        /// See https://www.chessprogramming.org/BitScan#Bitscanreverse
        /// </summary>
        static int[] index64 = {
            0, 47,  1, 56, 48, 27,  2, 60,
            57, 49, 41, 37, 28, 16,  3, 61,
            54, 58, 35, 52, 50, 42, 21, 44,
            38, 32, 29, 23, 17, 11,  4, 62,
            46, 55, 26, 59, 40, 36, 15, 53,
            34, 51, 20, 43, 31, 22, 10, 45,
            25, 39, 14, 33, 19, 30,  9, 24,
            13, 18,  8, 12,  7,  6,  5, 63
        };

        /// <summary>
        /// Use this method to count the number of set bits in a ulong
        /// </summary>
        public static int popCount(ulong x)
        {
            int count = 0;

            while (x > 0)
            {
                count++;
                x &= x - 1;  // Reset LS1B
            }

            return count;
        }

        public static int bitScanForward(ulong x)
        {
            if (x == 0)
            {
                return -1;
            }
            ulong twoscomp = (~x) + 1;
            return popCount((x & twoscomp) - 1);
        }

        /// <summary>
        /// See https://www.chessprogramming.org/BitScan#Bitscanreverse for explanation of how this works
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        public static int bitScanReverse(ulong x)
        {
            ulong DeBruijn = 0x03f79d71b4cb0a89;
            if (x > 0)
            {
                x |= x >> 1;
                x |= x >> 2;
                x |= x >> 4;
                x |= x >> 8;
                x |= x >> 16;
                x |= x >> 32;
                return index64[(x * DeBruijn) >> 58];
            }
            else { return -1; }
        }

        public static List<byte> getSetBitIndices(ulong x)
        {
            List<byte> indexList = new List<byte>();

            while (x > 0)
            {
                byte i = (byte)bitScanForward(x);
                indexList.Add(i);
                x = clearBit(x, i);
            }

            return indexList;
        }

        public static ulong setBit(ulong x, int bit)
        {
            return x | (1ul << bit);
        }

        public static ulong clearBit(ulong x, int bit)
        {
            return x & ~(1ul << bit);
        }

        /// <summary>
        /// This method returns a uint64 with a single bit set at the location of the first set bit in the input uint64
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        public static ulong maskFirst1Bit(ulong x)
        {
            return x ^ (x & (x - 1));
        }
    }
}
