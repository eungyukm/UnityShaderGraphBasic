using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Simple triangle class
/// </summary>
public class Triangle
{
	public Vector3 Pt0;
	public Vector3 Pt1;
	public Vector3 Pt2;
	
	public Vector2 U;
	public Vector2 V;
	public Vector2 W;
    public Vector3 Normal;
	public Transform Trans;
	
	public Triangle (Vector3 pt0, Vector3 pt1, Vector3 pt2, Vector2 u, Vector2 v, Vector2 w, Transform trans)
	{
		Pt0 = pt0;
		Pt1 = pt1;
		Pt2 = pt2;
		U = u;
		V = v;
		W = w;
		Trans = trans;
		UpdateVerts();
	    UpdateNormal();
	}

    private void UpdateNormal()
    {
        var u = Pt1 - Pt0;
        var v = Pt2 - Pt0;
        Normal = Vector3.Cross(u, v);
    }

    public void UpdateVerts(){
		Pt0 = Trans.TransformPoint(Pt0);
		Pt1 = Trans.TransformPoint(Pt1);
		Pt2 = Trans.TransformPoint(Pt2);
	}
}

public class TriMesh
{
    public List<Triangle> Triangles = new List<Triangle>();
    public MeshFilter MeshFilter;
    
    public TriMesh(MeshFilter meshFilter)
    {
        MeshFilter = meshFilter;
        Init(meshFilter);
    }

    public TriMesh(MeshRenderer meshRenderer)
    {
        MeshFilter = meshRenderer.GetComponent<MeshFilter>();
        Init(MeshFilter);
    }


    private void Init(MeshFilter meshFilter)
    {
        var mesh = meshFilter.sharedMesh;
        var vIndex = mesh.triangles;
        var verts = mesh.vertices;
        var uvs = mesh.uv;
        var i = 0;
        while (i < vIndex.Length)
        {
            Triangles.Add(
                new Triangle(
                    verts[vIndex[i + 0]],
                    verts[vIndex[i + 1]],
                    verts[vIndex[i + 2]],
                    uvs[vIndex[i + 0]],
                    uvs[vIndex[i + 1]],
                    uvs[vIndex[i + 2]],
                    meshFilter.transform));
            i += 3;
        }
    }
}