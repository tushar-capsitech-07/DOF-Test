using UnityEngine;
using Unity.Netcode;

public class NetworkBullet : NetworkBehaviour
{
    [SerializeField] private float speed = 10f;
    [SerializeField] private int damage = 20;
    [SerializeField] private GameObject bulletVisual;

    [Header("Particle Systems")]
    [SerializeField] private ParticleSystem hitParticleSystem;
    [SerializeField] private ParticleSystem wallHitParticleSystem;

    private NetworkVariable<ulong> netShooterId = new NetworkVariable<ulong>(
        ulong.MaxValue,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    private NetworkVariable<Vector2> netMoveDirection = new NetworkVariable<Vector2>(
        Vector2.right,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    private ulong shooterOwnerId = ulong.MaxValue;
    private Vector2 localMoveDirection = Vector2.right;
    private Collider2D shooterColliderToIgnore = null;

    private bool isDestroyed = false;
    private Collider2D bulletCollider;

    private void Awake()
    {
        bulletCollider = GetComponent<Collider2D>();
    }

    public void Initialize(ulong shooterId, Vector2 direction, Collider2D shooterCollider = null)
    {
        shooterOwnerId = shooterId;
        localMoveDirection = direction.normalized;
        shooterColliderToIgnore = shooterCollider;

        Debug.Log($"🔫 Bullet pre-init → shooter={shooterId}, dir={direction}");
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (IsServer)
        {
            netShooterId.Value = shooterOwnerId;
            netMoveDirection.Value = localMoveDirection;

            if (shooterColliderToIgnore != null && bulletCollider != null)
            {
                Physics2D.IgnoreCollision(bulletCollider, shooterColliderToIgnore, true);
                Debug.Log($"Server: Physics2D ignoring shooter collider ({shooterOwnerId})");
            }
        }
    }

    protected virtual void Update()
    {
        if (isDestroyed) return;

        transform.Translate(netMoveDirection.Value * speed * Time.deltaTime, Space.World);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!IsServer) return;
        if (isDestroyed) return;

        var hitPlayer = collision.gameObject.GetComponent<NetworkPlayerController>();

        if (hitPlayer != null)
        {
            ulong currentShooterId = netShooterId.Value;

            Debug.Log($"Collision: bullet owner={currentShooterId}, hit player={hitPlayer.OwnerClientId}");

            if (hitPlayer.OwnerClientId == currentShooterId)
            {
                Debug.Log($"Self-hit ignored: {currentShooterId}");
                return;
            }

            Debug.Log($"HIT! {currentShooterId} → {hitPlayer.OwnerClientId}, damage={damage}");
            hitPlayer.TakeDamage(damage);
            TriggerSlowMotionClientRpc(0.3f);
            if (hitParticleSystem != null) PlayHitParticleClientRpc();
            DestroyBulletClientRpc();
        }
        else if (collision.gameObject.CompareTag("Wall"))
        {
            Debug.Log("Bullet hit wall");
            PlayWallHitParticleClientRpc();
            DestroyBulletClientRpc();
        }
        else if (collision.gameObject.CompareTag("Bullet"))
        {
            Debug.Log("Bullet vs Bullet!");
            TriggerSlowMotionClientRpc(0.5f);
            if (hitParticleSystem != null) PlayHitParticleClientRpc();
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
        if (hitParticleSystem != null) hitParticleSystem.Play();
    }

    [ClientRpc]
    private void PlayWallHitParticleClientRpc()
    {
        if (wallHitParticleSystem != null) wallHitParticleSystem.Play();
    }

    [ClientRpc]
    private void DestroyBulletClientRpc()
    {
        isDestroyed = true;
        if (bulletVisual != null) bulletVisual.SetActive(false);
        if (bulletCollider != null) bulletCollider.enabled = false;
        if (IsServer) Destroy(gameObject, 0.15f);
    }
}