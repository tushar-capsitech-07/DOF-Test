using UnityEngine;

public class HealthController : MonoBehaviour
{
    public static HealthController Instance;

    [SerializeField] private int damage = 20;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public int ApplyDamage(int currentHealth)
    {
        Debug.Log("Taking damage: " + damage);
        return currentHealth - damage;
    }
}


