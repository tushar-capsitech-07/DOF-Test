using UnityEngine;
using System;


public abstract class GunController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] protected GameObject bulletPrefab;
    [SerializeField] protected Transform firePoint;
    [SerializeField] protected Transform forcePoint;

    [Header("Recoil")]
    [SerializeField] protected float recoilForce = 2f;
    [SerializeField] protected float torqueForce = 0.25f;

    [Header("Particle Systems")]
    [SerializeField] protected ParticleSystem shootParticle;
    [SerializeField] protected ParticleSystem destroyParticle;

    [Header("Shoot Sound Effect")]
    [SerializeField]protected AudioSource audioSources;
    [SerializeField]protected AudioClip[] audioShootClips;
    protected Rigidbody2D rb;



    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    // 🔹 Shared input helper
    protected bool HasValidTouch(Func<Vector2, bool> zoneCheck)
    {
        foreach (Touch touch in Input.touches)
        {
            if (touch.phase != TouchPhase.Began)
                continue;

            if (zoneCheck(touch.position))
                return true;
        }
        return false;
    }

    // 🔹 Shared shooting mechanics
    protected virtual void Shoot(int direction, string layer_gun, string opp)
    {
        Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);

        if (shootParticle != null)
            shootParticle.Play();

        // audioSources[UnityEngine.Random.Range(0,3)].Play();
        audioSources.resource = audioShootClips[UnityEngine.Random.Range(0,3)];
        audioSources.Play();
        Vector2 direct = direction * firePoint.right;

        RaycastHit2D hit = Physics2D.CapsuleCast(
            firePoint.position,
            new Vector2(0.2f, 0.4f),
            CapsuleDirection2D.Horizontal,
            0f,
            direct,
            8f,
            LayerMask.GetMask(layer_gun)
        );

        if (hit.collider != null && hit.collider.CompareTag(opp))
        {
            SlowMotionManager.Instance.TriggerSlowMotion(0.5f);
        }

        rb.AddForce(-direction * firePoint.right * recoilForce * 0.15f, ForceMode2D.Impulse);
        rb.AddForce(forcePoint.up * recoilForce * 0.75f, ForceMode2D.Impulse);
        rb.AddTorque(torqueForce * direction, ForceMode2D.Impulse);
    }
}
