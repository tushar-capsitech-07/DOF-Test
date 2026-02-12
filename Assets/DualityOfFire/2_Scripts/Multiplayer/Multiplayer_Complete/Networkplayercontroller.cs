using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;
using System.Collections;

/// <summary>
/// UNIVERSAL MULTIPLAYER PLAYER CONTROLLER
/// 
/// ✅ GUN SPIN RECOIL – rotates the gun pivot when shooting
/// ✅ JUMP RECOIL – adds vertical impulse
/// ✅ TORQUE – whole player spins (optional)
/// ✅ ALL EFFECTS SYNCED – both phones see the same thing
/// </summary>
public class NetworkPlayerController : NetworkBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Transform firePoint;      // Where bullets come from
    [SerializeField] private Transform gunPivot;       // The gun sprite that should spin (assign this!)
    [SerializeField] private Transform forcePoint;     // Where recoil force is applied

    [Header("Fire Settings")]
    [SerializeField] private float fireCooldown = 0.25f;
    private float nextFireTime;

    [Header("Recoil")]
    [SerializeField] private float recoilForce = 2f;       // Backward push
    [SerializeField] private float jumpForce = 1.5f;       // Upward push (jumping)
    [SerializeField] private float torqueForce = 1.5f;     // Body spin (set 0 to disable)

    [Header("Gun Spin Recoil")]
    [SerializeField] private float maxGunRotation = 15f;   // How far the gun rotates back
    [SerializeField] private float gunReturnSpeed = 8f;    // How fast it returns

    [Header("Health Settings")]
    [SerializeField] private int maxHealth = 100;
    [SerializeField] private Image healthBar;

    [Header("Particle Systems")]
    [SerializeField] private ParticleSystem shootParticle;
    [SerializeField] private ParticleSystem destroyParticle;

    [Header("Shoot Sound Effect")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip[] audioShootClips;

    // Network health
    private NetworkVariable<int> currentHealth = new NetworkVariable<int>(
        100,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    private Rigidbody2D rb;
    private bool isDead = false;
    private int shootDirection = 1; // 1 = right, -1 = left

    // Gun spin state (local only, for smooth return)
    private float currentGunRotation = 0f;
    private Quaternion originalGunRotation;

    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (gunPivot != null)
            originalGunRotation = gunPivot.localRotation;
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (IsServer)
        {
            currentHealth.Value = maxHealth;
        }

        currentHealth.OnValueChanged += OnHealthChanged;

        // Determine shoot direction AFTER position is final
        StartCoroutine(SetShootDirectionAfterSpawn());

        UpdateHealthBar();
    }

    private IEnumerator SetShootDirectionAfterSpawn()
    {
        yield return null;
        shootDirection = transform.position.x < 0 ? 1 : -1;
        Debug.Log($"🎯 Player at {transform.position.x} → shootDirection = {shootDirection}");
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
        if (!IsOwner) return;
        if (isDead) return;

        HandleInput();

        // Smoothly return gun to original rotation (local effect)
        if (gunPivot != null && currentGunRotation != 0f)
        {
            currentGunRotation = Mathf.Lerp(currentGunRotation, 0f, Time.deltaTime * gunReturnSpeed);
            gunPivot.localRotation = originalGunRotation * Quaternion.Euler(0, 0, currentGunRotation);
        }
    }

    void HandleInput()
    {
        if (Time.time < nextFireTime) return;

        bool shouldShoot = false;

#if UNITY_EDITOR
        if (Input.GetMouseButtonDown(0))
            shouldShoot = true;
#endif

        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Began)
                shouldShoot = true;
        }

        if (shouldShoot)
        {
            Shoot();
            nextFireTime = Time.time + fireCooldown;
        }
    }

    void Shoot()
    {
        if (!IsSpawned) return;
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
        NetworkObject netObj = bullet.GetComponent<NetworkObject>();

        if (netObj == null)
        {
            Debug.LogError("❌ Bullet missing NetworkObject!");
            Destroy(bullet);
            return;
        }

        var bulletScript = bullet.GetComponent<NetworkBullet>();
        if (bulletScript != null)
        {
            Vector2 bulletDir = direction > 0 ? Vector2.right : Vector2.left;
            bulletScript.Initialize(OwnerClientId, bulletDir);
        }

        netObj.Spawn();

        // ===== TRIGGER EFFECTS ON ALL CLIENTS =====
        PlayShootEffectsClientRpc(direction);
    }

    [ClientRpc]
    void PlayShootEffectsClientRpc(int direction)
    {
        // --- PARTICLE ---
        if (shootParticle != null) shootParticle.Play();

        // --- SOUND ---
        if (audioSource != null && audioShootClips.Length > 0)
        {
            audioSource.clip = audioShootClips[Random.Range(0, audioShootClips.Length)];
            audioSource.Play();
        }

        // --- PHYSICS RECOIL (Body) ---
        if (rb != null)
        {
            // Backward push
            Vector2 recoilDir = direction > 0 ? Vector2.left : Vector2.right;
            rb.AddForce(recoilDir * recoilForce, ForceMode2D.Impulse);

            // 🦘 JUMP (vertical recoil)
            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);

            // 🌀 BODY SPIN (torque)
            rb.AddTorque(torqueForce * -direction, ForceMode2D.Impulse);
        }

        // --- 🔫 GUN SPIN RECOIL (Visual) ---
        if (gunPivot != null)
        {
            // Rotate gun upward (negative direction for left, positive for right?)
            float rotationAmount = -maxGunRotation; // always rotate "up" (counter-clockwise)
            currentGunRotation = rotationAmount;
            gunPivot.localRotation = originalGunRotation * Quaternion.Euler(0, 0, rotationAmount);
        }
    }

    public void TakeDamage(int damage)
    {
        if (!IsServer) return;
        if (isDead) return;

        currentHealth.Value = Mathf.Clamp(currentHealth.Value - damage, 0, maxHealth);
    }

    private void OnHealthChanged(int oldHealth, int newHealth)
    {
        UpdateHealthBar();
        if (newHealth <= 0 && !isDead) Die();
    }

    void UpdateHealthBar()
    {
        if (healthBar != null)
            healthBar.fillAmount = (float)currentHealth.Value / maxHealth;
    }

    void Die()
    {
        if (isDead) return;
        isDead = true;

        PlayDeathEffectsClientRpc();

        if (IsOwner)
        {
            if (UIManager.Instance != null) UIManager.Instance.ShowGameOverWithDelay();
        }
        else if (IsServer)
        {
            if (UIManager.Instance != null) UIManager.Instance.ShowWinWithDelay();
        }
    }

    [ClientRpc]
    private void PlayDeathEffectsClientRpc()
    {
        var sr = GetComponent<SpriteRenderer>();
        if (sr != null) sr.enabled = false;

        if (destroyParticle != null) destroyParticle.Play();

        if (IsServer) Destroy(gameObject, 1f);
    }
}