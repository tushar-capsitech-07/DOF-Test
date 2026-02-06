using UnityEngine;

public abstract class BulletController : MonoBehaviour
{
    [SerializeField] private float speed = 10f;
    [SerializeField] private int damage = 20;
    [SerializeField] private GameObject BulletVisual;

    [Header("Particle Systems")]
    [SerializeField] private ParticleSystem hitParticleSystem;
    [SerializeField] private ParticleSystem wallHitParticleSystem;

    // 👇 THIS is the key
    protected abstract string TargetTag { get; }

    protected virtual Vector2 MoveDirection => Vector2.right;

    protected virtual void Update()
    {
        transform.Translate(MoveDirection * speed * Time.deltaTime);
    }

    
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag(TargetTag))
        {
            SlowMotionManager.Instance.TriggerSlowMotion(0.3f);

            if (hitParticleSystem != null)
                hitParticleSystem.Play();

            EnemyGun enemy = collision.gameObject.GetComponent<EnemyGun>();
            PlayerGun player = collision.gameObject.GetComponent<PlayerGun>();

            if (enemy != null)
            {
                enemy.TakeDamage(damage);
                            }
            if (player != null)
            {
                player.TakeDamage(damage);
            }

            BulletVisual.SetActive(false);
            GetComponent<BoxCollider2D>().enabled = false;
            Destroy(gameObject, 0.15f);
        }
        else if (collision.gameObject.CompareTag("Wall"))
        {
            wallHitParticleSystem.Play();
            Destroy(gameObject,0.15f);
            BulletVisual.SetActive(false);
        }
        else if (collision.gameObject.CompareTag("Bullet"))
        {
            SlowMotionManager.Instance.TriggerSlowMotion(0.5f);

            if (hitParticleSystem != null)
                hitParticleSystem.Play();

            GetComponent<BoxCollider2D>().enabled = false;
            BulletVisual.SetActive(false);
            Destroy(gameObject, 0.15f);
        }
    }
}
