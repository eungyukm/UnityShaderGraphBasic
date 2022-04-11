using UnityEngine;

public class TransformWithAniCurve : MonoBehaviour
{
    public AnimationCurve ac;

    public Vector3 StartPos;
    public Vector3 EndPos;
    public float DelayTime = 0.2f;
    public float PlayTime = 1.0f;

    private Transform _trans = null;
    private float _delayTimer = 0.0f;
    private float _playerTimer = 0.0f;
    
    // Start is called before the first frame update
    void Start()
    {
        _trans = GetComponent<Transform>();
    }

    // Update is called once per frame
    void Update()
    {
        if (_trans == null || ac == null)
        {
            return;
        }

        if (_delayTimer <= DelayTime)
        {
            Debug.Log("Delay Call!!");
            _delayTimer += Time.deltaTime;
            return;
        }

        if (_playerTimer <= PlayTime)
        {
            Debug.Log("Move!!");
            float t = ac.Evaluate(_playerTimer / PlayTime);
            Debug.Log("T : " + t);
            _trans.position = Vector3.Lerp(StartPos, EndPos, t);
            _playerTimer += Time.deltaTime;
        }
    }
}
