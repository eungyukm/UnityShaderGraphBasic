using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Object = UnityEngine.Object;

/// <summary>
/// This does the raycasting stuff using a octree.
/// We use the bounds octree from https://github.com/mcserep/UnityOctree
/// The TestIntersection is if from http://www.cs.virginia.edu/~gfx/Courses/2003/ImageSynthesis/papers/Acceleration/Fast%20MinimumStorage%20RayTriangle%20Intersection.pdf
/// </summary>
public class OctreeRaycast
{
    public static BoundsOctree<TriMesh> BuildOctree(Bounds mainBounds)
    {
        var o = new BoundsOctree<TriMesh>(0.1f, mainBounds.center, 0.01f, 1.5f);
        var meshFilters = Object.FindObjectsOfType<GameObject>().Select(x => x.GetComponent<MeshRenderer>()).Where(x => x != null).ToList();
        foreach (var meshFilter in meshFilters)
            o.Add(new TriMesh(meshFilter), meshFilter.bounds);
        Debug.Log("Number of leafs " + o.Count);
        return o;
    }

    public static OctreeRaycastHit[] RaycastAll(BoundsOctree<TriMesh> octree, Ray ray, float maxDistance, LayerMask mask)
    {
        var results = new List<TriMesh>();
        octree.GetColliding(results, ray, maxDistance);
        var hits = new List<OctreeRaycastHit>();
        foreach (var hit in results)
        {
            if ((1 << hit.MeshFilter.gameObject.layer & mask.value) != 1 << hit.MeshFilter.gameObject.layer)
                continue;
            var dist = 0f;
            var baryCoord = new Vector2();
            var normal = Vector3.zero;
            hits.AddRange(from t1 in hit.Triangles where TestIntersection(t1, ray, out dist, out baryCoord, out normal) select BuildRaycastHit(t1, dist, baryCoord, normal));
        }
        return hits.ToArray();
    }


    private static OctreeRaycastHit BuildRaycastHit(Triangle hitTriangle, float distance, Vector2 barycentricCoordinate, Vector3 normal)
    {
        var returnedHit = new OctreeRaycastHit(hitTriangle.Trans, distance, barycentricCoordinate, normal)
        {
            TextureCoord = hitTriangle.U + (hitTriangle.V - hitTriangle.U) * barycentricCoordinate.x +
                           (hitTriangle.W - hitTriangle.U) * barycentricCoordinate.y,
            Point = hitTriangle.Pt0 + ((hitTriangle.Pt1 - hitTriangle.Pt0) * barycentricCoordinate.x) +
                    ((hitTriangle.Pt2 - hitTriangle.Pt0) * barycentricCoordinate.y)
        };

        return returnedHit;
    }

    private static bool TestIntersection(Triangle triangle, Ray ray, out float dist, out Vector2 baryCoord, out Vector3 normal)
    {
        baryCoord = Vector2.zero;
        normal = Vector3.zero;
        dist = Mathf.Infinity;
        var edge1 = triangle.Pt1 - triangle.Pt0;
        var edge2 = triangle.Pt2 - triangle.Pt0;

        var pVec = Vector3.Cross(ray.direction, edge2);
        var det = Vector3.Dot(edge1, pVec);
        if (det < Mathf.Epsilon)
            return false;
        var tVec = ray.origin - triangle.Pt0;
        var u = Vector3.Dot(tVec, pVec);
        if (u < 0 || u > det)
            return false;
        var qVec = Vector3.Cross(tVec, edge1);
        var v = Vector3.Dot(ray.direction, qVec);
        if (v < 0 || u + v > det)
            return false;
        dist = Vector3.Dot(edge2, qVec);
        var invDet = 1 / det;
        dist *= invDet;
        baryCoord.x = u * invDet;
        baryCoord.y = v * invDet;
        normal = triangle.Normal;
        return true;
    }
}
