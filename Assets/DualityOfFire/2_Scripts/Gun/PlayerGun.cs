// Player-controlled gun

using UnityEngine;
using UnityEngine.UI;

public class PlayerGun : GunController
{
    // ========================= Fire Settings =========================
    [Header("Fire Settings")]
    [SerializeField] private float fireCooldown = 0.25f;
    private float nextFireTime;

    // ========================= Health Settings =========================
    [Header("Health Settings")]
    [SerializeField] private int maxHealth = 100;
    [SerializeField] private Image playerHealthImg;
    [SerializeField] private PlayerDataScriptableObject playerDataScriptableObject;
    private int currentHealth;

    // ========================= Unity Lifecycle =========================
    protected override void Awake()
    {
        base.Awake();
    }

    private void Start()
    {
        currentHealth = maxHealth;
        UpdateHealthBar();
    }

    void Update()
    {
        PlayerInput();
    }

    void PlayerInput()
    {
        if (playerDataScriptableObject.PlayerChoice ==1)
        {
            if (HasValidTouch(pos =>true ) && Time.time >= nextFireTime)
            {
                base.Shoot(1, "Enemy", "AIGun");
                nextFireTime = Time.time + fireCooldown;
            }    
        }
        else if(playerDataScriptableObject.PlayerChoice==2)
        {
            if (HasValidTouch(pos=>pos.y <Screen.height /2f) && Time.time >= nextFireTime)
            {
                base.Shoot(1, "Enemy", "AIGun");
                nextFireTime = Time.time + fireCooldown;
            }
        }

        
    }

    // ========================= Health Logic =========================
    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        UpdateHealthBar();

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void UpdateHealthBar()
    {
        if (playerHealthImg != null)
        {
            playerHealthImg.fillAmount = (float)currentHealth / maxHealth;
        }
    }

    // ========================= Death =========================
    void Die()
    {
        gameObject.GetComponent<SpriteRenderer>().enabled = false;
        destroyParticle.Play();
        Destroy(gameObject,1f);
        UIManager.Instance.ShowGameOverWithDelay();
    }
}
