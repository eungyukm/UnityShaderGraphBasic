using UnityEngine;
using System;

[Serializable]
[CreateAssetMenu(menuName = "ScriptableObjects/OnValidate", fileName = "OnValidate")]
public class OnInspectorValidate : ScriptableObject
{
    public float volume;
    
    public void OnValidate()
    {
        Debug.Log("OnValidate Call!!");
        Debug.Log("volume : " + volume);
    }
}