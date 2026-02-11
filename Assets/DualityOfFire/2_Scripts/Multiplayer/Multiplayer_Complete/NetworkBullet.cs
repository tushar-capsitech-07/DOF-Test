using UnityEngine;
using Unity.Netcode;

/// <summary>
/// UNIVERSAL MULTIPLAYER BULLET
/// Works for BOTH players - automatically detects enemies
/// Path: Assets/Scripts/Multiplayer/NetworkBullet.cs
/// 
/// SETUP:
/// 1. Create ONE bullet prefab
/// 2. Attach this script
/// 3. Add NetworkObject component
/// 4. Add to NetworkManager Prefabs List
/// 5. Assign to BOTH player prefabs (same bullet for everyone!)
/// </summary>
public class NetworkBullet : NetworkBehaviour
{
    [SerializeField] private float speed = 10f;
    [SerializeField] private int damage = 20;
    [SerializeField] private GameObject bulletVisual;

    [Header("Particle Systems")]
    [SerializeField] private ParticleSystem hitParticleSystem;
    [SerializeField] private ParticleSystem wallHitParticleSystem;

    // Who shot this bullet?
    private NetworkVariable<ulong> shooterClientId = new NetworkVariable<ulong>();

    // Bullet movement direction
    private Vector2 moveDirection = Vector2.right;

    public void Initialize(ulong shooterId, Vector2 direction)
    {
        if (IsServer)
        {
            shooterClientId.Value = shooterId;
            moveDirection = direction.normalized;
        }
    }

    protected virtual void Update()
    {
        // Only server moves bullets
        if (!IsServer) return;

        transform.Translate(moveDirection * speed * Time.deltaTime);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Only server handles collision
        if (!IsServer) return;

        // Check if hit a player
        var hitGun = collision.gameObject.GetComponent<NetworkPlayerController>();
        if (hitGun != null)
        {
            // Is this our own bullet? Don't damage ourselves!
            if (hitGun.OwnerClientId == shooterClientId.Value)
            {
                Debug.Log("⚠️ Hit own bullet - ignoring");
                return;
            }

            // Hit enemy player!
            Debug.Log($"💥 Bullet from {shooterClientId.Value} hit enemy {hitGun.OwnerClientId}");

            TriggerSlowMotionClientRpc(0.3f);

            if (hitParticleSystem != null)
                PlayHitParticleClientRpc();

            hitGun.TakeDamage(damage);
            DestroyBulletClientRpc();
        }
        else if (collision.gameObject.CompareTag("Wall"))
        {
            PlayWallHitParticleClientRpc();
            DestroyBulletClientRpc();
        }
        else if (collision.gameObject.CompareTag("Bullet"))
        {
            // Bullet vs bullet collision
            TriggerSlowMotionClientRpc(0.5f);

            if (hitParticleSystem != null)
                PlayHitParticleClientRpc();

            DestroyBulletClientRpc();
        }
    }

    [ClientRpc]
    private void TriggerSlowMotionClientRpc(float duration)
    {
        if (SlowMotionManager.Instance != null)
            SlowMotionManager.Instance.TriggerSlowMotion(duration);
    }

    [ClientRpc]
    private void PlayHitParticleClientRpc()
    {
        if (hitParticleSystem != null)
            hitParticleSystem.Play();
    }

    [ClientRpc]
    private void PlayWallHitParticleClientRpc()
    {
        if (wallHitParticleSystem != null)
            wallHitParticleSystem.Play();
    }

    [ClientRpc]
    private void DestroyBulletClientRpc()
    {
        if (bulletVisual != null)
            bulletVisual.SetActive(false);

        var collider = GetComponent<BoxCollider2D>();
        if (collider != null)
            collider.enabled = false;

        if (IsServer)
        {
            Destroy(gameObject, 0.15f);
        }
    }
}