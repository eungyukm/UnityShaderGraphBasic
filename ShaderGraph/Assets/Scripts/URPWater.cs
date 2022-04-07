using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.PlayerLoop;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using WaterSystem;

// ExcuteAlways 속성은 스크립트가 Editor Tool의 일부로 특정 작업을 수행 할 때 사용할 수 있습니다.
[ExecuteAlways]
public class URPWater : MonoBehaviour
{
    // Singleton
    private static URPWater _instance;

    public static URPWater Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = (URPWater) FindObjectOfType(typeof(URPWater));
            }
            return _instance;
        }
    }

    private void OnEnable()
    {
        bool computeOverride = false;
        bool _useComputeBuffer;
        
        if (!computeOverride)
        {
            _useComputeBuffer = SystemInfo.supportsComputeShaders &&
                                Application.platform != RuntimePlatform.WebGLPlayer &&
                                Application.platform != RuntimePlatform.Android;
        }
        else
        {
            _useComputeBuffer = false;
        }

        Init();

        RenderPipelineManager.beginCameraRendering += BeginCameraRendering;

        WaterResources resources = null;
        
        // Water Resources는 Scripts > Data > WaterResources가 존재
        if (resources == null)
        {
            resources = Resources.Load("WaterResources") as WaterResources;
        }
    }

    // Script references
    // private PlanarReflections
    
    // 01. Camera의 Rendering을 시작하는 코드
    private void BeginCameraRendering(ScriptableRenderContext src, Camera cam)
    {
        if (cam.cameraType == CameraType.Preview)
        {
            return;
        }

        var roll = cam.transform.localEulerAngles.z;
        // Shader.SetGlobalFloat(CameraRo);
    }

    private void Init()
    {
        
    }
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
