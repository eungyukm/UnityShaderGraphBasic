using UnityEngine;

public class OnInspectorData : MonoBehaviour
{
    public int value = 2;

    public Transform tr;

    private void OnEnable()
    {
        Debug.Log("value : " + value);
        // Debug.Log("Transform Pos : " + tr.position);
    }
}
