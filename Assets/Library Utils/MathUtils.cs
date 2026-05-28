using UnityEngine;

public static class MathUtils
{
    public static float AbsMax(float a, float b)
    {
        if (Mathf.Abs(a) > Mathf.Abs(b))
        {
            return a;
        }
        else
        {
            return b;
        }
    }

    public static Vector2 Rotate(this ref Vector2 v, float angle)
    {
        float ca = Mathf.Cos(angle);
        float sa = Mathf.Sin(angle);
        return new Vector2(v.x * ca - v.y * sa, v.x * sa + v.y * ca);
    }

    public static float Angle(this ref Vector2 v)
    {
        return Mathf.Atan2(v.y, v.x);
    }
}
