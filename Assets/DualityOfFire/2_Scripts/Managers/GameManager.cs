using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public GameState currentState = GameState.Stop;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Application.targetFrameRate = 120;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        SetState(GameState.Stop);
    }

    public void SetState(GameState newState)
    {
        currentState = newState;

        switch (currentState)
        {
            case GameState.Playing:
                Time.timeScale = 1f;
                Time.fixedDeltaTime = 0.02f;
                break;

            case GameState.Stop:
                Time.timeScale = 0f;
                break;
        }
    }

    public bool IsPlaying()
    {
        return currentState == GameState.Playing;
    }

    private bool isRestart = false;
    public void IsRestartGame(bool isEnable) => isRestart = isEnable;
    public bool GetIsRestartGame() => isRestart;

    public enum GameState
    {
        Playing,
        Stop
    }
}

