using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;
using System;

/// <summary>
/// UNIVERSAL MULTIPLAYER PLAYER CONTROLLER
/// Works for BOTH Host and Client
/// Automatically detects which side to be on and who is enemy
/// Path: Assets/Scripts/Multiplayer/NetworkPlayerController.cs
/// 
/// SETUP:
/// 1. Create ONE player prefab
/// 2. Attach this script
/// 3. Add NetworkObject + NetworkTransform
/// 4. Assign bullet prefab (same for everyone)
/// 5. Add to NetworkManager Prefabs List
/// 6. Done! Use same prefab for all players
/// </summary>
public class NetworkPlayerController : NetworkBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private Transform forcePoint;

    [Header("Fire Settings")]
    [SerializeField] private float fireCooldown = 0.25f;
    private float nextFireTime;

    [Header("Recoil")]
    [SerializeField] private float recoilForce = 2f;
    [SerializeField] private float torqueForce = 0.25f;

    [Header("Health Settings")]
    [SerializeField] private int maxHealth = 100;
    [SerializeField] private Image healthBar;

    [Header("Particle Systems")]
    [SerializeField] private ParticleSystem shootParticle;
    [SerializeField] private ParticleSystem destroyParticle;

    [Header("Shoot Sound Effect")]
    [SerializeField] private AudioSource audioSources;
    [SerializeField] private AudioClip[] audioShootClips;

    // Network synced health
    private NetworkVariable<int> currentHealth = new NetworkVariable<int>(
        100,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    private Rigidbody2D rb;
    private bool isDead = false;
    private int shootDirection = 1; // 1 for right, -1 for left

    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        // Initialize health
        if (IsServer)
        {
            currentHealth.Value = maxHealth;
        }

        currentHealth.OnValueChanged += OnHealthChanged;

        // Determine shoot direction based on which side we're on
        // If we're on the left (x < 0), shoot right
        // If we're on the right (x > 0), shoot left
        if (transform.position.x < 0)
        {
            shootDirection = 1; // Shoot RIGHT
            Debug.Log($"🎯 Player at x={transform.position.x} will shoot RIGHT");
        }
        else
        {
            shootDirection = -1; // Shoot LEFT
            Debug.Log($"🎯 Player at x={transform.position.x} will shoot LEFT");
        }

        UpdateHealthBar();
    }

    public override void OnNetworkDespawn()
    {
        currentHealth.OnValueChanged -= OnHealthChanged;
        base.OnNetworkDespawn();
    }

    private void Start()
    {
        UpdateHealthBar();
    }

    void Update()
    {
        // Only owner controls this player
        if (!IsOwner) return;
        if (isDead) return;

        HandleInput();
    }

    void HandleInput()
    {
        if (Time.time < nextFireTime) return;

        bool shouldShoot = false;

        // Editor testing with mouse
#if UNITY_EDITOR
        if (Input.GetMouseButtonDown(0))
        {
            shouldShoot = true;
        }
#endif

        // Touch input for mobile
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Began)
            {
                shouldShoot = true;
            }
        }

        if (shouldShoot)
        {
            Shoot();
            nextFireTime = Time.time + fireCooldown;
        }
    }

    void Shoot()
    {
        // Check if network is ready
        if (NetworkManager.Singleton == null || !NetworkManager.Singleton.IsListening)
        {
            Debug.LogWarning("⚠️ Network not ready!");
            return;
        }

        if (!IsSpawned)
        {
            Debug.LogWarning("⚠️ Not spawned yet!");
            return;
        }

        // Request server to shoot
        ShootServerRpc(shootDirection);
    }

    [ServerRpc]
    void ShootServerRpc(int direction)
    {
        if (bulletPrefab == null || firePoint == null)
        {
            Debug.LogError("❌ Missing bullet prefab or fire point!");
            return;
        }

        // Spawn bullet
        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);

        var netObj = bullet.GetComponent<NetworkObject>();
        if (netObj == null)
        {
            Debug.LogError("❌ Bullet missing NetworkObject!");
            Destroy(bullet);
            return;
        }

        // Initialize bullet with shooter info
        var bulletScript = bullet.GetComponent<NetworkBullet>();
        if (bulletScript != null)
        {
            Vector2 bulletDir = direction > 0 ? Vector2.right : Vector2.left;
            bulletScript.Initialize(OwnerClientId, bulletDir);
        }

        netObj.Spawn();

        // Play effects on all clients
        PlayShootEffectsClientRpc(direction);
    }

    [ClientRpc]
    void PlayShootEffectsClientRpc(int direction)
    {
        // Particle
        if (shootParticle != null)
            shootParticle.Play();

        // Sound
        if (audioSources != null && audioShootClips != null && audioShootClips.Length > 0)
        {
            audioSources.clip = audioShootClips[UnityEngine.Random.Range(0, audioShootClips.Length)];
            audioSources.Play();
        }

        // Recoil
        if (rb != null && firePoint != null)
        {
            Vector2 recoilDir = direction > 0 ? Vector2.left : Vector2.right;
            rb.AddForce(recoilDir * recoilForce * 0.15f, ForceMode2D.Impulse);
            rb.AddForce(forcePoint.up * recoilForce * 0.75f, ForceMode2D.Impulse);
            rb.AddTorque(torqueForce * direction, ForceMode2D.Impulse);
        }
    }

    // Server-only damage
    public void TakeDamage(int damage)
    {
        if (!IsServer) return;
        if (isDead) return;

        currentHealth.Value = Mathf.Clamp(currentHealth.Value - damage, 0, maxHealth);
    }

    private void OnHealthChanged(int oldHealth, int newHealth)
    {
        UpdateHealthBar();

        if (newHealth <= 0 && !isDead)
        {
            Die();
        }
    }

    void UpdateHealthBar()
    {
        if (healthBar != null)
        {
            healthBar.fillAmount = (float)currentHealth.Value / maxHealth;
        }
    }

    void Die()
    {
        if (isDead) return;
        isDead = true;

        PlayDeathEffectsClientRpc();

        // Show game over for owner
        if (IsOwner)
        {
            if (UIManager.Instance != null)
                UIManager.Instance.ShowGameOverWithDelay();
        }
        // Show win for opponent
        else if (IsServer)
        {
            if (UIManager.Instance != null)
                UIManager.Instance.ShowWinWithDelay();
        }
    }

    [ClientRpc]
    private void PlayDeathEffectsClientRpc()
    {
        var spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
            spriteRenderer.enabled = false;

        if (destroyParticle != null)
            destroyParticle.Play();

        if (IsServer)
        {
            Destroy(gameObject, 1f);
        }
    }
}