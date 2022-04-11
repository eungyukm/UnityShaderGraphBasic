using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

public class JobDependency : MonoBehaviour
{
    void Start()
    {
        NativeArray<float> result = new NativeArray<float>(1, Allocator.TempJob);
        // Job 1 데이터 셋업
        MyJob jobData = new MyJob();
        jobData.a = 10;
        jobData.b = 3;
        jobData.result = result;
        
        // Job 1 예약
        JobHandle firstHandle = jobData.Schedule();

        // Job 2에 대한 데이터 셋업
        AddOneJob incJobData = new AddOneJob();
        incJobData.result = result;
        
        // Job 2 예약
        JobHandle secondHandle = incJobData.Schedule(firstHandle);
        
        // Job 2가 완료되기를 기다립니다.
        secondHandle.Complete();
        
        // 결과 출력
        float job2 = result[0];
        Debug.Log("Result 2 : " + job2);
        result.Dispose();
    }
}

public struct MyJob : IJob
{
    public float a;
    public float b;
    public NativeArray<float> result;

    public void Execute()
    {
        result[0] = a + b;
    }
}

public struct AddOneJob : IJob
{
    public NativeArray<float> result;

    public void Execute()
    {
        result[0] = result[0] + 1;
    }
}