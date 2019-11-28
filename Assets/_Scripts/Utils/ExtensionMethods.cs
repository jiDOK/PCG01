using UnityEngine;

public static class ExtensionMethods
{
    public static Vector3 ToXZ(this Vector2 v2)
    {
        return new Vector3(v2.x, 0f, v2.y);
    }
}
