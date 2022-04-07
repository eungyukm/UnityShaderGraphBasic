using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

public class URPGerstnerWavesJobs : MonoBehaviour
{
    // 02. IJobParallelFor를 통해 Job System으로 Wave를 만들고 있습니다.
    [BurstCompile]
    private struct HeightJob : IJobParallelFor
    {
        public void Execute(int index)
        {
            throw new System.NotImplementedException();
        }
    }
}
