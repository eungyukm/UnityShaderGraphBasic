using UnityEngine;


public static class ProbeExtensions
{
    public static Ray ToRay(this Vector3 @this)
    {
        return new Ray(@this, Vector3.up);
    }

    public static Ray ToRay(this Vector3 @this, Vector3 direction)
    {
        return new Ray(@this, direction);
    }

}
