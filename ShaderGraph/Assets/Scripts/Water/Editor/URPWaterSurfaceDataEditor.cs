using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

[CustomEditor(typeof(URPWaterSurfaceData))]
public class URPWaterSurfaceDataEditor : Editor
{
    [SerializeField] private ReorderableList waveList;

    private void OnValidate()
    {
        var init = serializedObject.FindProperty("_init");
        if (init?.boolValue == false)
        {
            Debug.Log("UWSD Init is false");
            Setup();
        }

        var standardHeight = EditorGUIUtility.singleLineHeight;
        var standardLine = standardHeight + EditorGUIUtility.standardVerticalSpacing;

        waveList = new ReorderableList(serializedObject, serializedObject.FindProperty("_waves"), true, true, true,
            true);

        waveList.drawElementCallback = (rect, index, isActive, isFocused) =>
        {

        };
    }

    private void Setup()
    {
        URPWaterSurfaceData uwsd = (URPWaterSurfaceData) target;
        uwsd._init = true;
        uwsd._absorptionRamp = DefaultAbsorptionGrad();
        uwsd._scatterRamp = DefaultScatterGrad();
        EditorUtility.SetDirty(uwsd);
    }
    
    Gradient DefaultAbsorptionGrad() // Preset for absorption
    {
        Gradient g = new Gradient();
        GradientColorKey[] gck = new GradientColorKey[5];
        GradientAlphaKey[] gak = new GradientAlphaKey[1];
        gak[0].alpha = 1;
        gak[0].time = 0;
        gck[0].color = Color.white;
        gck[0].time = 0f;
        gck[1].color = new Color(0.22f, 0.87f, 0.87f);
        gck[1].time = 0.082f;
        gck[2].color = new Color(0f, 0.47f, 0.49f);
        gck[2].time = 0.318f;
        gck[3].color = new Color(0f, 0.275f, 0.44f);
        gck[3].time = 0.665f;
        gck[4].color = Color.black;
        gck[4].time = 1f;
        g.SetKeys(gck, gak);
        return g;
    }
    
    Gradient DefaultScatterGrad() // Preset for scattering
    {
        Gradient g = new Gradient();
        GradientColorKey[] gck = new GradientColorKey[4];
        GradientAlphaKey[] gak = new GradientAlphaKey[1];
        gak[0].alpha = 1;
        gak[0].time = 0;
        gck[0].color = Color.black;
        gck[0].time = 0f;
        gck[1].color = new Color(0.08f, 0.41f, 0.34f);
        gck[1].time = 0.15f;
        gck[2].color = new Color(0.13f, 0.55f, 0.45f);
        gck[2].time = 0.42f;
        gck[3].color = new Color(0.21f, 0.62f, 0.6f);
        gck[3].time = 1f;
        g.SetKeys(gck, gak);
        return g;
    }
}
