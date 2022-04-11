using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaveGenerator : MonoBehaviour
{
    [SerializeField] private GameObject target;
    
    // Start is called before the first frame update
    void Start()
    {
        for (int i = 0; i < 100; i++)
        {
            Vector3 pos = target.transform.position;
            pos.x += (float) (i + 1) * 0.2f;
            var go = Instantiate(target, pos, Quaternion.identity);
        }
    }
}
