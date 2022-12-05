namespace BitMono.Utilities.Extensions.Hex
{
    public static class ValueToHexExtensions
    {
        public static string ToHexString(this int source)
        {
            return string.Format("0x{0:X}", source);
        }
        public static string ToHexString(this long source)
        {
            return string.Format("0x{0:X}", source);
        }
    }
}