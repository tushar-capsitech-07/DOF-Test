using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class EnemyGun : GunController
{
    // ================= Fire Settings =================
    [Header("Fire Settings")]
    [SerializeField] private float fireCooldown = 0.25f;
    [SerializeField] private float minFireTime = 0.5f;
    [SerializeField] private float maxFireTime = 2f;

    // ================= Health Settings =================
    [Header("Health Settings")]
    [SerializeField] private int maxHealth = 100;
    [SerializeField] private Image enemyHealthImg;
    [SerializeField] private PlayerDataScriptableObject playerDataScriptableObject;

    private int currentHealth;
    private float nextFireTime;
    private bool canUpdate;
    private bool isDead;

    // ================= Unity =================
    protected override void Awake()
    {
        base.Awake();
    }

    private void Start()
    {
        currentHealth = maxHealth;
        UpdateHealthBar();

        if (playerDataScriptableObject.PlayerChoice == 1)
        {
            canUpdate = false;
            ScheduleNextShot();
        }
        else
        {

            GetComponent<Rigidbody2D>().gravityScale = -GetComponent<Rigidbody2D>().gravityScale;
            canUpdate = true;
        }
    }

    private void Update()
    {
        if (isDead || !canUpdate || Time.time < nextFireTime)
            return;

        if (HasValidTouch(touch => touch.y > Screen.height / 2f))
        {
            Shoot(-2, "Player", "Player");
            nextFireTime = Time.time + fireCooldown;
        }
    }

    // ================= AI Fire =================
    private void ScheduleNextShot()
    {
        Invoke(nameof(AIFire), Random.Range(minFireTime, maxFireTime));
    }

    private void AIFire()
    {
        if (isDead)
            return;

        if (Time.time < nextFireTime)
        {
            ScheduleNextShot();
            return;
        }

        Shoot(-2, "Player", "Player");
        nextFireTime = Time.time + fireCooldown;
        ScheduleNextShot();
    }

    // ================= Health =================
    public void TakeDamage(int damage)
    {
        if (isDead)
            return;

        currentHealth = Mathf.Clamp(currentHealth - damage, 0, maxHealth);
        UpdateHealthBar();

        if (currentHealth <= 0)
            Die();
    }

    private void UpdateHealthBar()
    {
        if (enemyHealthImg != null)
            enemyHealthImg.fillAmount = (float)currentHealth / maxHealth;
    }

    // ================= Death =================
    private void Die()
    {
        if (isDead)
            return;

        isDead = true;
        CancelInvoke();
        gameObject.GetComponent<SpriteRenderer>().enabled = false;
        destroyParticle.Play();
        Destroy(gameObject,1f);
        UIManager.Instance.ShowWinWithDelay();
    }

}
