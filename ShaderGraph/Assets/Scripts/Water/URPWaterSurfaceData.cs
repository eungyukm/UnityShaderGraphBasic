using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

[System.Serializable]
[CreateAssetMenu(fileName = "WaterSurfaceData", menuName = "WaterSystem/URP Surface Data", order = 0)]
public class URPWaterSurfaceData : ScriptableObject
{
    public float _waterMaxVisibility = 40.0f;
    public Gradient _absorptioRamp;
    public Gradient _scatterRamp;
    public List<Wave> _waves = new List<Wave>();
    public bool _customWaves = false;
    public int randomSeed = 3234;
    public BasicWaves _basicWaveSettings = new BasicWaves(1.5f, 45.0f, 5.0f);
    public FoamSettings _foamSettings = new FoamSettings();
    [SerializeField] public bool _init = false;
}

[System.Serializable]
public struct Wave
{
    public float amplitude;
    public float direction;
    public float wavelength;
    public float2 origin;
    public float onmiDir;

    public Wave(float amp, float dir, float length, float2 org, bool omni)
    {
        amplitude = amp;
        direction = dir;
        wavelength = length;
        origin = org;
        onmiDir = omni ? 1 : 0;
    }
}

[System.Serializable]
public class BasicWaves
{
    // Wave 개수
    public int numWaves = 6;
    // 
    public float amplitude;
    public float direction;
    public float wavelength;

    public BasicWaves(float amp, float dir, float len)
    {
        numWaves = 6;
        amplitude = amp;
        direction = dir;
        wavelength = len;
    }
}

[System.Serializable]
public class FoamSettings
{
    // 0 = defualt, 1=simple, 3=custom
    public int foamType;
    public AnimationCurve basicFoam;
    public AnimationCurve liteFoam;
    public AnimationCurve mediumFoam;
    public AnimationCurve denseFoam;

    public FoamSettings()
    {
        foamType = 0;
        basicFoam = new AnimationCurve(
            new Keyframe[2]
        {
            new Keyframe(0.25f, 0),
            new Keyframe(1f, 1f)
        });
        
        liteFoam = new AnimationCurve(
            new Keyframe[3]
            {
                new Keyframe(0.2f, 0),
                new Keyframe(0.4f, 1f),
                new Keyframe(0.7f, 0f)
            }
        );
        
        mediumFoam = new AnimationCurve(
            new Keyframe[3]
            {
                new Keyframe(0.4f, 0),
                new Keyframe(0.7f, 1f),
                new Keyframe(1f, 0f)
            }
        );

        denseFoam = new AnimationCurve(
            new Keyframe[2]
            {
                new Keyframe(0.7f, 1f),
                new Keyframe(1f, 1f)
            }
        );
    }
}