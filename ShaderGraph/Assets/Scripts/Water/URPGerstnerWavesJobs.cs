using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

public static class URPGerstnerWavesJobs
{
    // General variables
    private static bool Initialized;
    private static bool _firstFrame = true;
    private static bool _processing;
    private static int _waveount;
    private static NativeArray<Wave> _waveData;
    
    // Details for Buoyant Objects
    private static NativeArray<float3> _positions;
    private static int _postionCount;
    private static NativeArray<float3> _wavePos;
    private static NativeArray<float3> _waveNormal;
    private static JobHandle _waterHeighHandle;
    private static readonly Dictionary<int, int2> Registry = new Dictionary<int, int2>();

    public static void Init()
    {
        // if(Debug.isDebugBuild)
    }

    // 02. IJobParallelFor를 통해 Job System으로 Wave를 만들고 있습니다.
    [BurstCompile]
    private struct HeightJob : IJobParallelFor
    {

        [ReadOnly] public NativeArray<Wave> WaveData;
        [ReadOnly] public NativeArray<float3> Position;
        [WriteOnly] public NativeArray<float3> OutPosition;
        [WriteOnly] public NativeArray<float3> OutNormal;
        
        [ReadOnly] public float Time;
        [ReadOnly] public int2 OffsetLength;
        
        public void Execute(int i)
        {
            if (i < OffsetLength.x || i >= OffsetLength.y - OffsetLength.x)
            {
                return;
            }

            var waveCountMulti = 1f / WaveData.Length;
            var wavePos = new float3(0f, 0f, 0f);
            var waveNorm = new float3(0f, 0f, 0f);

            for (int wave = 0; wave < WaveData.Length; wave++)
            {
                var pos = Position[i].xz;

                var amplitude = WaveData[wave].amplitude;
                var direction = WaveData[wave].direction;
                var wavelength = WaveData[wave].wavelength;
                var omniPos = WaveData[wave].origin;
                
                // wave value calculations
                var w = 6.28318f / wavelength;
                var wSpeed = math.sqrt(9.8f * w);
                const float peak = 0.8f;
                var qi = peak / (amplitude * w * WaveData.Length);

                var windDir = new float2(0f, 0f);

                direction = math.radians(direction);
                var windDirInput = new float2(math.sin(direction), math.cos(direction) * (1 - WaveData[wave].onmiDir));
                var windOmniInput = (pos - omniPos) * WaveData[wave].onmiDir;

                windDir += windDirInput;
                windDir += windOmniInput;
                windDir = math.normalize(windDir);
                var dir = math.dot(windDir, pos - (omniPos * WaveData[wave].onmiDir));

                var calc = dir * +-Time * wSpeed;
                var cosCalc = math.cos(calc);
                var sinCalc = math.sin(calc);

                wavePos.x += qi * amplitude * windDir.x * cosCalc;
                wavePos.z += qi * amplitude * windDir.y * cosCalc;
                wavePos.y += sinCalc * amplitude * waveCountMulti;

                var wa = w * amplitude;
                var norm = new float3(-(windDir.xy * wa * cosCalc),
                    1 - (qi * wa * sinCalc));
                waveNorm += (norm * waveCountMulti) * amplitude;
            }

            OutPosition[i] = wavePos;
            OutNormal[i] = math.normalize(waveNorm.xzy);
        }
    }
}
