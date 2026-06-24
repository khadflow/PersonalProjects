using Cinemachine;
using UnityEngine;

public class CinemachineTargetGroupManager : MonoBehaviour
{
    public static CinemachineTargetGroupManager Instance { get; private set; }
    private CinemachineTargetGroup _targetGroup;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        _targetGroup = GetComponent<CinemachineTargetGroup>();
    }

    /// <summary>
    /// Call this from your Multiplayer Spawner script whenever a player is instantiated.
    /// </summary>
    public void AddPlayerToTargetGroup(Transform playerTransform)
    {
        if (_targetGroup == null || playerTransform == null) return;

        // In Cinemachine v2, the syntax is AddMember(Transform, weight, radius)
        _targetGroup.AddMember(playerTransform, 1f, 1.5f);
        Debug.Log($"[CAMERA] Dynamically tracked target added: {playerTransform.name}");
    }

    /// <summary>
    /// Call this if a player dies or leaves the match.
    /// </summary>
    public void RemovePlayerFromTargetGroup(Transform playerTransform)
    {
        if (_targetGroup == null || playerTransform == null) return;

        // In Cinemachine v2, the syntax is RemoveMember(Transform)
        _targetGroup.RemoveMember(playerTransform);
    }
}