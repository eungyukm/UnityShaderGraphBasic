using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class SharkMovement : MonoBehaviour
{
    public Vector3 velocity = Vector3.forward;
    public float speed = 3;
    public float rotateSpeed = 90f;

    public float animSpeed = 0;

    public Animator Animator;

    public GameObject PivotGroup;
    public List<Transform> pivot = new List<Transform>();

    public int pivotIndex = 0;
    // Start is called before the first frame update
    void Start()
    {
        Animator = GetComponent<Animator>();
        animSpeed = 1;

        int childCout = PivotGroup.transform.childCount;
        // Debug.Log("Child : " + childCout);

        for (int i = 0; i < childCout; i++)
        {
            Transform pos = PivotGroup.transform.GetChild(i).transform;
            pivot.Add(pos); 
        }
    }
    
    // Update is called once per frame
    void Update()
    {
        Animator.SetFloat("speed", animSpeed);
        // transform.Translate(velocity * speed * Time.deltaTime);
        var step = speed * Time.deltaTime;
        transform.position = Vector3.MoveTowards(transform.position, pivot[pivotIndex].position, step);
        // transform.rotation = Quaternion.Slerp(
        //     transform.rotation, 
        //         Quaternion.LookRotation(pivot[pivotIndex].position)
        //     , rotateSpeed * Time.deltaTime);
        
        transform.LookAt(pivot[pivotIndex].position); 
        Debug.Log("Look At : "+ Quaternion.LookRotation(pivot[pivotIndex].position).eulerAngles);
        
        if (Vector3.Distance(transform.position, pivot[pivotIndex].position) < 0.1f)
        {
            if (pivotIndex >= pivot.Count -1)
            {
                pivotIndex = 0;
            }
            else
            {
                pivotIndex++;
            }
        }
    }
}
