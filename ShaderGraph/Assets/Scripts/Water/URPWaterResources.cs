using UnityEngine;
using UnityEngine.Serialization;

[System.Serializable]
[CreateAssetMenu(fileName = "WaterResources", menuName = "WaterSystem/URPWaterResource", order = 0)]
public class URPWaterResources : ScriptableObject
{
    public Texture2D defaultFoamRamp;
    public Texture2D defaultFoamMap;
    [FormerlySerializedAs("defaultSurfaceMpa")] public Texture2D defaultSurfaceMap;
    public Material defaultSeaMaterial;
    public Mesh[] defaultWaterMeshes;
}