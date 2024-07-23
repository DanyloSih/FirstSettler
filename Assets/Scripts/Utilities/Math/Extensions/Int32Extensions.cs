namespace Utilities.Math.Extensions
{
    public static class Int32Extensions
    {
        public static int NextPowerOfTwo(this int n)
        {
            n--;
            n |= n >> 1;
            n |= n >> 2;
            n |= n >> 4;
            n |= n >> 8;
            n |= n >> 16;
            return n + 1;
        }
    }
}