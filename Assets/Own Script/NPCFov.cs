using UnityEngine;
using UnityEngine.Animations.Rigging;

public class NPCFov : MonoBehaviour
{
    [Header("References")]
    public Transform player;
    public Rig npcRig;
    public SkinnedMeshRenderer avatarMesh;
    public GameObject eyeIcon;

    [Header("FOV Settings")]
    [Range(0f, 180f)]
    public float fieldOfView = 60f;
    public Transform npcHead;
    public float viewRange = 8f; // how far NPC can see

    [Header("Rig Weight Settings")]
    public float activeWeight = 1f;
    public float blendSpeed = 3f;

    [Header("Eye Look Settings")]
    public string eyesLookUpBlendShapeName = "eyesLookUp";
    public float eyesLookUpCloseValue = 0.035f;
    public float closeDistance = 2f;

    private float targetWeight;
    private int eyesLookUpIndex = -1;

    protected virtual void Start()
    {
        // Find the blend shape index by name
        if (avatarMesh != null && !string.IsNullOrEmpty(eyesLookUpBlendShapeName))
        {
            eyesLookUpIndex = avatarMesh.sharedMesh.GetBlendShapeIndex(eyesLookUpBlendShapeName);
            if (eyesLookUpIndex == -1)
                Debug.LogWarning($"Blend shape '{eyesLookUpBlendShapeName}' not found on {avatarMesh.name}");
        }

        if (eyeIcon != null)
            eyeIcon.SetActive(false);
    }

    protected virtual void Update()
    {
        if (player == null || npcRig == null || npcHead == null) return;

        // --- Distance check ---
        float distance = Vector3.Distance(player.position, npcHead.position);
        if (distance > viewRange)
        {
            // Player is too far → NPC can't see
            targetWeight = 0f;
            npcRig.weight = Mathf.Lerp(npcRig.weight, targetWeight, Time.deltaTime * blendSpeed);
            if (eyeIcon != null) eyeIcon.SetActive(false);
            avatarMesh?.SetBlendShapeWeight(eyesLookUpIndex, 0f);
            return;
        }

        // --- FOV logic ---
        Vector3 toPlayer = (player.position - npcHead.position).normalized;
        float angle = Vector3.Angle(npcHead.forward, toPlayer);

        bool inFov = angle <= fieldOfView;

        // Set rig weight
        targetWeight = inFov ? activeWeight : 0f;
        npcRig.weight = Mathf.Lerp(npcRig.weight, targetWeight, Time.deltaTime * blendSpeed);

        // Toggle eye icon
        if (eyeIcon != null)
            eyeIcon.SetActive(inFov);

        // --- Eye look-up logic ---
        if (avatarMesh != null && eyesLookUpIndex != -1)
        {
            if (distance <= closeDistance)
                avatarMesh.SetBlendShapeWeight(eyesLookUpIndex, eyesLookUpCloseValue * 100f); // 0–100 scale
            else
                avatarMesh.SetBlendShapeWeight(eyesLookUpIndex, 0f);
        }
    }

    void OnDrawGizmosSelected()
    {
        if (npcHead == null) return;

        // Draw FOV cone
        Gizmos.color = Color.yellow;
        Vector3 forward = npcHead.forward;
        Quaternion leftRayRotation = Quaternion.AngleAxis(-fieldOfView, Vector3.up);
        Quaternion rightRayRotation = Quaternion.AngleAxis(fieldOfView, Vector3.up);

        Gizmos.DrawRay(npcHead.position, leftRayRotation * forward * viewRange);
        Gizmos.DrawRay(npcHead.position, rightRayRotation * forward * viewRange);

        // Show view range sphere
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(npcHead.position, viewRange);

        // Show close distance sphere
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(npcHead.position, closeDistance);
    }
}
