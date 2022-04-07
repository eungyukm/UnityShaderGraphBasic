using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

public class JobSample : MonoBehaviour
{
    struct VelocityJob : IJobParallelFor
    {
        /// <summary>
        /// 읽기 전용으로 선언하면 여러 작업이 동시에 데이터에 액세스할 수 있습니다.
        /// </summary>
        [ReadOnly] public NativeArray<Vector3> velocity;
        
        // 기본적으로 컨테이너는 읽기 및 쓰기로 간주됩니다.
        public NativeArray<Vector3> position;
        
        // 일반적으로 job에는 프레임 개념이 없으므로 deltaTime 시간을 작업에 복사해야 합니다.
        // Main Thread에는 작업을 동일한 프레임 또는 다음 프레임을 기다리지만 작업은 결정적으로 작업해야 합니다.
        public float deltaTime;

        /// <summary>
        /// 워커 스레드에서 실행되는 함수
        /// </summary>
        /// <param name="index"></param>
        public void Execute(int index)
        {
            position[index] = position[index] + velocity[index] * deltaTime;
        }
    }

    public void Update()
    {
        var position = new NativeArray<Vector3>(500, Allocator.Persistent);
        var velocity = new NativeArray<Vector3>(500, Allocator.Persistent);

        for (var i = 0; i < velocity.Length; i++)
        {
            velocity[i] = new Vector3(0, 10, 0);
        }
        
        // Job Data 생성
        var job = new VelocityJob()
        {
            deltaTime = Time.deltaTime,
            position = position,
            velocity = velocity
        };

        JobHandle jobHandle = job.Schedule(position.Length, 64);
        jobHandle.Complete();
        
        Debug.Log(job.position[0]);
        
        // Native array들은 수동으로 dispose해야 합니다.
        position.Dispose();
        velocity.Dispose();
    }
}
