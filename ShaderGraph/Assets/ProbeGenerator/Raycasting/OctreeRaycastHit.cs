using UnityEngine;

public class OctreeRaycastHit
{
    public float Distance;
    public Transform Transform;
    public Vector2 BarycentricCoordinate;
    public Vector2 TextureCoord;
    public Vector3 Point;
    public Vector3 Normal;

    public OctreeRaycastHit()
    {
        Distance = 0f;
        Transform = null;
        TextureCoord = Vector2.zero;
        BarycentricCoordinate = Vector2.zero;
        Point = Vector3.zero;
        Normal = Vector3.zero;
    }

    public OctreeRaycastHit(Transform transform, float distance, Vector2 barycentricCoordinate, Vector3 normal)
    {
        Distance = distance;
        Transform = transform;
        BarycentricCoordinate = barycentricCoordinate;
        TextureCoord = Vector2.zero;
        Point = Vector3.zero;
        Normal = normal;
    }
}