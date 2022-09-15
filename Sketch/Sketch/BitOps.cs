using System.Numerics;

namespace Sketch
{
    internal class BitOps
    {
        public static int CeilingPowerOfTwo(int x)
        {
            return (int)CeilingPowerOfTwo((uint)x);
        }

        /// <summary>
        /// Calculate the smallest power of 2 greater than the input parameter.
        /// </summary>
        /// <param name="x">The input parameter.</param>
        /// <returns>Smallest power of two greater than or equal to x.</returns>
        public static uint CeilingPowerOfTwo(uint x)
        {
#if NETSTANDARD2_0
            // https://graphics.stanford.edu/~seander/bithacks.html#RoundUpPowerOf2
            --x;
            x |= x >> 1;
            x |= x >> 2;
            x |= x >> 4;
            x |= x >> 8;
            x |= x >> 16;
            return x + 1;
#else
            return 1u << -BitOperations.LeadingZeroCount(x - 1);
#endif

        }
    }
}