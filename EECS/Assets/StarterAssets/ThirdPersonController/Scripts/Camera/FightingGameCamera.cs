using UnityEngine;
using Cinemachine;

public class FightingGameCamera : MonoBehaviour
{
    [Header("Strict Zoom Limits")]
    public float baseDistance = 4f;      // Force the camera exactly 4 units away
    public float zoomFactor = 0.5f;      // How aggressively it pushes out when they separate
    public float minDistance = 4f;
    public float maxDistance = 12f;

    private CinemachineVirtualCamera _vCam;
    private CinemachineFramingTransposer _transposer;

    private GameObject _player1;
    private GameObject _player2;

    void Start()
    {
        _vCam = GetComponent<CinemachineVirtualCamera>();
        if (_vCam != null)
        {
            _transposer = _vCam.GetCinemachineComponent<CinemachineFramingTransposer>();
        }
    }

    void LateUpdate()
    {
        if (_transposer == null) return;

        // Dynamic target tracking
        if (_player1 == null) _player1 = GameObject.FindWithTag("Player");
        if (_player2 == null) _player2 = GameObject.FindWithTag("Player2");

        if (_player1 != null && _player2 != null)
        {
            // 1. Calculate absolute physical distance between characters
            float playerDistance = Mathf.Abs(_player1.transform.position.x - _player2.transform.position.x);

            // 2. Mathematically clamp the value so it CANNOT exceed your tight limits
            float forcedDistance = baseDistance + (playerDistance * zoomFactor);
            forcedDistance = Mathf.Clamp(forcedDistance, minDistance, maxDistance);

            // 3. Bruteforce override the transposer distance values
            _transposer.m_CameraDistance = forcedDistance;
        }
        else
        {
            // If they haven't spawned yet, force lock it to 4
            _transposer.m_CameraDistance = 4f;
        }
    }
}