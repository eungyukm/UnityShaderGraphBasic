using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

public class CreateJobSystem : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        NativeArray<float> result = new NativeArray<float>(1, Allocator.TempJob);

        AdditionJob jobData = new AdditionJob();
        jobData.a = 10;
        jobData.b = 10;
        jobData.result = result;
        
        // 작업 예약
        JobHandle handle = jobData.Schedule();
        handle.Complete();
        
        // NativeArray의 모든 사본이 동일한 메모리를 가리킨다면, "본인"의 결과값이 접근이 가능합니다.    
        float aPlusB = result[0];
        Debug.Log("reulst : " + aPlusB);
        
        result.Dispose();
    }
}

public struct AdditionJob : IJob
{
    public float a;
    public float b;
    public NativeArray<float> result;
    public void Execute()
    {
        result[0] = a + b;
    }
}