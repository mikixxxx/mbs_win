namespace mbs;

public static class Ext
{
    public static string LimitLength(this string source, int maxLength)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentOutOfRangeException.ThrowIfNegative(maxLength);
        return source.Length <= maxLength ? source : source[..maxLength];
    }

    public static T Clamp<T>(this T value, T min, T max) where T : IComparable<T>
    {
        if (min.CompareTo(max) > 0)
            throw new ArgumentException("Minimum must not exceed maximum.");
        if (value.CompareTo(min) < 0) return min;
        if (value.CompareTo(max) > 0) return max;
        return value;
    }

    public static float Map(this float x, float inputMin, float inputMax, float outputMin, float outputMax)
    {
        if (inputMin == inputMax)
            throw new ArgumentException("Input range must be nonzero.");
        return (x - inputMin) * (outputMax - outputMin) / (inputMax - inputMin) + outputMin;
    }
}
