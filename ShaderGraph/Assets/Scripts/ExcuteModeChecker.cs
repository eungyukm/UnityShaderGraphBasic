using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class ExcuteModeChecker : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        if (Application.IsPlaying(gameObject))
        {
            Debug.Log("Play Mode!");
        }
        else
        {
            // Editor
            Debug.Log("Editor Mode!");
        }
    }
}