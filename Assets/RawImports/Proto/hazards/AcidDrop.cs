using UnityEngine;

[DisallowMultipleComponent]
public sealed class AcidDropletLoop : MonoBehaviour
{
    [Header("Collision filter")]
    [Tooltip("Only collisions with these layers will reset the droplet.")]
    [SerializeField] private LayerMask resetOnLayers;

    [Header("Reset behavior")]
    [Tooltip("Small upward offset after reset to avoid immediate re-collision.")]
    [SerializeField] private float respawnYOffset = 0.05f;

    [Tooltip("If true, freeze one physics step after teleport to keep it stable.")]
    [SerializeField] private bool sleepAfterReset = true;

    private Rigidbody _rb;
    private Vector3 _originPos;
    private Quaternion _originRot;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        if (_rb == null)
        {
            Debug.LogError($"{nameof(AcidDropletLoop)} requires a Rigidbody on the same GameObject.", this);
            enabled = false;
            return;
        }

        // Capture origin in Awake so it works both in editor play + runtime spawn.
        _originPos = transform.position;
        _originRot = transform.rotation;
    }

    private void OnEnable()
    {
        // If this object is pooled / re-enabled, re-capture origin if needed.
        // Comment this out if you want origin to remain the first-ever spawn.
        _originPos = transform.position;
        _originRot = transform.rotation;
    }

    private void OnCollisionEnter(Collision collision)
    {
        // Layer filter
        int otherLayerMask = 1 << collision.gameObject.layer;
        if ((resetOnLayers.value & otherLayerMask) == 0)
            return;

        ResetToOrigin();
    }

    private void ResetToOrigin()
    {
        // Stop motion first (prevents weird post-teleport momentum)
        _rb.linearVelocity = Vector3.zero;
        _rb.angularVelocity = Vector3.zero;

        // Teleport
        transform.SetPositionAndRotation(_originPos + Vector3.up * respawnYOffset, _originRot);

        // Optional: force physics to settle
        if (sleepAfterReset)
            _rb.Sleep();

        // Wake so gravity resumes next fixed update
        _rb.WakeUp();
    }
}
