using UnityEngine;
using System.Collections;

public class SlowMotionManager : MonoBehaviour
{
    public static SlowMotionManager Instance;

    [SerializeField] private float slowTimeScale = 0.08f;

    private int activeRequests = 0;
    private Coroutine slowMoRoutine;

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

    public void TriggerSlowMotion(float duration)
    {
        if (!GameManager.Instance.IsPlaying())
            return;

        activeRequests++;

        if (slowMoRoutine == null)
        {
            slowMoRoutine = StartCoroutine(SlowMotionCoroutine(duration));
        }
    }

    private IEnumerator SlowMotionCoroutine(float duration)
    {
        ApplySlowMotion();

        yield return new WaitForSecondsRealtime(duration);

        activeRequests--;

        if (activeRequests <= 0)
        {
            ResetTime();
        }
        else
        {
            slowMoRoutine = StartCoroutine(SlowMotionCoroutine(duration));
        }
    }

    private void ApplySlowMotion()
    {
        if (!GameManager.Instance.IsPlaying())
            return;

        Time.timeScale = slowTimeScale;
        Time.fixedDeltaTime = 0.02f * Time.timeScale;
    }

    private void ResetTime()
    {
        activeRequests = 0;
        slowMoRoutine = null;

        // Restore based on game state
        if (GameManager.Instance.IsPlaying())
        {
            Time.timeScale = 1f;
            Time.fixedDeltaTime = 0.02f;
        }
        else
        {
            Time.timeScale = 0f;
        }
    }
}
