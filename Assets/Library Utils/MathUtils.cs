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

    public static Pose GetPose(this Transform transform)
    {
        transform.GetPositionAndRotation(out Vector3 position, out Quaternion rotation);
        return new Pose(position, rotation);
    }

    public static Pose GetLocalPose(this Transform transform)
    {
        transform.GetLocalPositionAndRotation(out Vector3 localPosition, out Quaternion localRotation);
        return new Pose(localPosition, localRotation);
    }
}
