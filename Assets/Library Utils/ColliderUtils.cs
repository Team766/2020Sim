using UnityEngine;
using System.Collections.Generic;

public static class ColliderUtils
{
    public static bool WorldSpaceBoundsContains(Collider collider, GameObject go)
    {
        var bounds = collider.bounds;
        foreach (var c in go.GetComponentsInChildren<Collider>())
        {
            foreach (var p in c.bounds.GetVertices())
            {
                if (!bounds.Contains(p))
                {
                    return false;
                }
            }
        }
        return true;
    }

    public static IEnumerable<Vector3> GetVertices(this Bounds bounds)
    {
        var min = bounds.min;
        var max = bounds.max;
        yield return min;
        yield return new Vector3(min.x, min.y, max.z);
        yield return new Vector3(min.x, max.y, min.z);
        yield return new Vector3(min.x, max.y, max.z);
        yield return new Vector3(max.x, min.y, min.z);
        yield return new Vector3(max.x, min.y, max.z);
        yield return new Vector3(max.x, max.y, min.z);
        yield return max;
    }
}