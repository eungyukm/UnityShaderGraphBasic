using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wobble : MonoBehaviour
{
    public Renderer _rend;
    private Vector3 _lastPos;
    private Vector3 _velocity;
    private Vector3 _lastRot;
    private Vector3 _angularVelocity;

    public float MaxWobble = 0.03f;
    public float WobbleSpeed = 1f;
    public float Recovery = 1f;

    private float _wobbleAmountX;
    private float _wobbleAmountZ;
    private float _wobbleAmountToAddX;
    private float _wobbleAmountToAddZ;

    private float _pulse;
    private float _time = 0.5f;


    // Start is called before the first frame update
    void Start()
    {
        _rend = GetComponent<Renderer>();
    }

    // Update is called once per frame
    void Update()
    {
        _time += Time.deltaTime;

        _wobbleAmountToAddX = Mathf.Lerp(_wobbleAmountToAddX, 0, Time.deltaTime * Recovery);
        _wobbleAmountToAddZ = Mathf.Lerp(_wobbleAmountToAddZ, 0, Time.deltaTime * Recovery);

        _pulse = 2 * Mathf.PI * WobbleSpeed;
        _wobbleAmountX = _wobbleAmountToAddX * Mathf.Sin(_pulse * _time);
        _wobbleAmountZ = _wobbleAmountToAddZ * Mathf.Sin(_pulse * _time);

        _rend.material.SetFloat("_WobbleX", _wobbleAmountX);
        _rend.material.SetFloat("_WobbleZ", _wobbleAmountZ);

        _velocity = (_lastPos - transform.position) / Time.deltaTime;
        _angularVelocity = transform.rotation.eulerAngles - _lastRot;

        _wobbleAmountToAddX += Mathf.Clamp((_velocity.x + (_angularVelocity.z * 0.2f)) * MaxWobble, -MaxWobble, MaxWobble);
        _wobbleAmountToAddZ += Mathf.Clamp((_velocity.z + (_angularVelocity.x * 0.2f)) * MaxWobble, -MaxWobble, MaxWobble);

        _lastPos = transform.position;
        _lastRot = transform.rotation.eulerAngles;
    }
}
