using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GerstnerWave : MonoBehaviour
{
    private Vector3 startPos;

    // Start is called before the first frame update
    void Start()
    {
        startPos = gameObject.transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        var pos = gameObject.transform.position;
        float theta = startPos.x + Time.time;
        Debug.Log(theta);
        // Sin 0도 : 0, 90도 : 1
        // Cos 0도 : 1, 90도 : 0
        float sin = Mathf.Sin(theta);
        float cos = Mathf.Cos(theta);
        pos.y = startPos.y + sin;
        pos.x = startPos.x + cos;
        gameObject.transform.position = pos;
    }
}
