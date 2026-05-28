using UnityEngine;

public static class GameObjectUtils
{
    public static Bounds CalculateBoundsRecursive(GameObject parentObject)
    {
        Renderer[] renderers = parentObject.GetComponentsInChildren<Renderer>();
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
}
