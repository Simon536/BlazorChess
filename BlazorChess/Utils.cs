namespace ChessEngine
{
    static class utils
    {
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

        public static ulong setBit(ulong x, int bit)
        {
            return x | (1ul << bit);
        }

        public static ulong clearBit(ulong x, int bit)
        {
            return x & ~(1ul << bit);
        }
    }
}
