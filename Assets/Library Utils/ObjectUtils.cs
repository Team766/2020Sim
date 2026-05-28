using System;

public class ObjectUtils
{
    public static T Ensure<T>(T? value, string errorMessage) where T : struct
    {
        if (value == null)
        {
            throw new ArgumentNullException(errorMessage);
        }
        return (T)value;
    }

    public static T Ensure<T>(T value, string errorMessage)
    {
        if (value == null)
        {
            throw new ArgumentNullException(errorMessage);
        }
        return (T)value;
    }
}
