using UnityEngine;
using UnityEngine.Rendering.Universal;

/// <summary>
/// 06. WaterSettingsData 반사 타입 지형 타입 등의 정보를 담고 있습니다.
/// </summary>
[System.Serializable]
[CreateAssetMenu(fileName = "WaterSettingsData", menuName = "WaterSystem/URP WaterSettings")]
public class URPWaterSettingsData : ScriptableObject
{
    public GeometryType waterGeomType;
    public ReflectionType refType = ReflectionType.PlanarReflection;

    public PlanarReflections.PlanarReflectionSettings planarSettings;
    public Cubemap cubemapRefType;

    public bool isInfinite;
    public Vector4 originOffset = new Vector4(0f, 0f, 500f, 500f);
}

[System.Serializable]
public enum ReflectionType
{
    Cubemap,
    ReflectionProbe,
    PlanarReflection
}
    
[System.Serializable]
public enum GeometryType
{
    VertexOffset,
    Tesselation
}
