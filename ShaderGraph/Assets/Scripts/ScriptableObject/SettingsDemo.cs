using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(menuName = "ScriptableObjects/SettingsDemo", fileName = "SettingsDemo")]
public class SettingsDemo : ScriptableObject
{
    public SettingType settingType;
    public int value;
}

[Serializable]
public enum SettingType
{
    None,
    Debug,
    Release
}