using UnityEngine;

public static class GameObjectUtils
{
    public static Bounds CalculateBoundsRecursive(GameObject parentObject)
    {
        Renderer[] renderers = parentObject.GetComponentsInChildren<Renderer>();
        // As of Unity3d 6000.2.3f1, the center of Collider.bounds can be incorrect.
        // We can get enough information from just renderers for the one existing
        // use case (getting the size of wheel modules), so we disable Collider
        // bounds for now.
        Collider[] colliders = parentObject.GetComponentsInChildren<Collider>();
        if (renderers.Length == 0 && colliders.Length == 0)
        {
            return new Bounds(parentObject.transform.position, Vector3.zero);
        }

        // Start with the first child's bounds
        Bounds bounds = renderers.Length > 0 ? renderers[0].bounds : colliders[0].bounds;

        // Encapsulate all renderers' bounds
        for (int i = 0; i < renderers.Length; i++)
        {
            bounds.Encapsulate(renderers[i].bounds);
        }

        // Encapsulate all colliders' bounds
        for (int i = 0; i < colliders.Length; i++)
        {
            bounds.Encapsulate(colliders[i].bounds);
        }

        return bounds;
    }

    private static Bounds CalculateOrientedRendererBounds(Renderer renderer, Matrix4x4 frame_T_world)
    {
        var world_T_local = renderer.localToWorldMatrix;
        var frame_T_local = frame_T_world * world_T_local;
        var min = renderer.localBounds.min;
        var max = renderer.localBounds.max;

        Bounds bounds = new Bounds(frame_T_local.MultiplyPoint(min), Vector3.zero);
        bounds.Encapsulate(frame_T_local.MultiplyPoint(new Vector3(min.x, min.y, max.z)));
        bounds.Encapsulate(frame_T_local.MultiplyPoint(new Vector3(min.x, max.y, min.z)));
        bounds.Encapsulate(frame_T_local.MultiplyPoint(new Vector3(min.x, max.y, max.z)));
        bounds.Encapsulate(frame_T_local.MultiplyPoint(new Vector3(max.x, min.y, min.z)));
        bounds.Encapsulate(frame_T_local.MultiplyPoint(new Vector3(max.x, min.y, max.z)));
        bounds.Encapsulate(frame_T_local.MultiplyPoint(new Vector3(max.x, max.y, min.z)));
        bounds.Encapsulate(frame_T_local.MultiplyPoint(max));

        return bounds;
    }

    public static OrientedBounds CalculateOrientedRendererBoundsRecursive(GameObject parentObject)
    {
        Renderer[] renderers = parentObject.GetComponentsInChildren<Renderer>();
        if (renderers.Length == 0)
        {
            return new OrientedBounds(parentObject.transform.position, parentObject.transform.rotation, Vector3.zero);
        }

        Matrix4x4 frame_T_world = parentObject.transform.worldToLocalMatrix;

        // Start with the first child's bounds
        Bounds localBounds = CalculateOrientedRendererBounds(renderers[0], frame_T_world);

        // Encapsulate all renderers' bounds
        for (int i = 1; i < renderers.Length; i++)
        {
            localBounds.Encapsulate(CalculateOrientedRendererBounds(renderers[i], frame_T_world));
        }
        var unscaledSize = localBounds.size;
        var scale = parentObject.transform.localScale;
        return new OrientedBounds(
                parentObject.transform.TransformPoint(localBounds.center),
                parentObject.transform.rotation,
                new Vector3(unscaledSize.x * scale.x, unscaledSize.y * scale.y, unscaledSize.z * scale.z));
    }
}
