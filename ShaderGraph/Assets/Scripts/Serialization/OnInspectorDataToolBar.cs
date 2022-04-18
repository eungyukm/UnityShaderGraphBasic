using System;
using UnityEngine;

[Serializable]
public class OnInspectorDataToolBar : MonoBehaviour
{
    public ToolMode toolMode;
    public Texture2D mainTexture;
    public ToolValue toolValue;
}
[Serializable]
public enum ToolMode
{
    None,
    Debug,
    Release
}

[Serializable]
public class ToolValue
{
    public float m_Value = 0.1f;
    public LayerMask m_LayerMask = -1;
    public bool m_Enable = false;
}