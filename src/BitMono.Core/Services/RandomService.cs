namespace BitMono.Core.Services;

public static class RandomService
{
    private static readonly Random Random = new Random();

    public static int RandomNext(int minValue, int maxValue)
    {
        return Random.Next(minValue, maxValue);
    }
}