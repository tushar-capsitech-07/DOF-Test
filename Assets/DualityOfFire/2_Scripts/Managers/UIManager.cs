using UnityEngine;
using UnityEngine.SceneManagement;
using static GameManager;
using System.Collections;
using Unity.VisualScripting;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [Header("UI Delays")]
    private float resultPanelDelay = 1.5f;

    // ========================= Main Panels =========================
    [Header("Main Panels")]
    [SerializeField] private GameObject startPanel;
    [SerializeField] public GameObject gamePlayPanel;
    [SerializeField] public GameObject multiplayerPanel;
    [SerializeField] public GameObject multiplayerGamePlayPanel;

    // ========================= PopUp Panels =========================
    [Header("PopUp Panels")]
    [SerializeField] private GameObject settingPanel;
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private GameObject winPanel;
    [SerializeField] private GameObject gameOverPanel;

    // ========================= Gameplay Objects =========================
    [Header("Gameplay Objects")]
    [SerializeField] private GameObject gun;
    [SerializeField] private GameObject split;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip[] audioClips;

    // ========================= ScriptableObjects =========================
    [Header("ScriptableObjects")]
    [SerializeField] private PlayerDataScriptableObject playerChoice;

    // ========================= Unity Lifecycle =========================
    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    private void Start()
    {
        ShowStartPanel();
    }

    // ========================= Panel Control =========================
    public void ShowStartPanel()
    {
        DisableAllPanels();
        DisableGun();

        if (!GameManager.Instance.GetIsRestartGame())
        {
            startPanel.SetActive(true);
            SetGameState(GameState.Stop);
        }
        else
        {
            ShowGamePlayPanel(playerChoice.PlayerChoice);
            GameManager.Instance.IsRestartGame(false);
        }
    }

    public void ShowGamePlayPanel(int choice)
    {
        playerChoice.PlayerChoice = choice;

        DisableAllPanels();
        gamePlayPanel.SetActive(true);
        split.SetActive(choice == 2);

        EnableGun();

        SetGameState(GameState.Playing);
    }

    public void ShowmultiplayerPanel()
    {
        DisableAllPanels();
        multiplayerPanel.SetActive(true);
        Time.timeScale = 1f;
    }

       
    public void ShowmultiplayerGamePlayPanel()
    {
        DisableAllPanels();
        multiplayerGamePlayPanel.SetActive(true);
        Time.timeScale = 1f;

    }

    public void ShowSettingPanel(bool isActive)
    {
        settingPanel.SetActive(isActive);
        HandlePause(isActive);
    }

    public void ShowPausePanel(bool isActive)
    {
        pausePanel.SetActive(isActive);
        gamePlayPanel.SetActive(!isActive);
        HandlePause(isActive);
    }

    public void ShowWinPanel()
    {
        DisableAllPanels();
        DisableGun();
        winPanel.SetActive(true);
        SetGameState(GameState.Stop);
    }

    public void ShowGameOverPanel()
    {
        DisableAllPanels();
        DisableGun();
        gameOverPanel.SetActive(true);
        SetGameState(GameState.Stop);
    }

    // ========================= Restart & Exit =========================
    public void RestartGame()
    {
        Time.timeScale = 1f;
        SetGameState(GameState.Stop);
        GameManager.Instance.IsRestartGame(true);
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void ExitGame()
    {
        SetGameState(GameState.Stop);
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    // ========================= Helpers =========================
    private void DisableAllPanels()
    {
        startPanel.SetActive(false);
        gamePlayPanel.SetActive(false);
        settingPanel.SetActive(false);
        pausePanel.SetActive(false);
        winPanel.SetActive(false);
        gameOverPanel.SetActive(false);
        multiplayerPanel.SetActive(false);
        multiplayerGamePlayPanel.SetActive(false);
    }

    public void EnableGun()
    {
        if (gun != null)
            gun.SetActive(true);
    }

    public void DisableGun()
    {
        if (gun != null)
            gun.SetActive(false);
    }

    private void HandlePause(bool pause)
    {
        SetGameState(pause ? GameState.Stop : GameState.Playing);
    }

    private void SetGameState(GameState state)
    {
        if (GameManager.Instance != null)
            GameManager.Instance.SetState(state);
    }

    // ========================= Coroutines =========================
    public void ShowWinWithDelay()
    {
        StartCoroutine(ShowWinRoutine());
    }

    public void ShowGameOverWithDelay()
    {
        StartCoroutine(ShowGameOverRoutine());
    }

    private IEnumerator ShowWinRoutine()
    {
        audioSource.resource = audioClips[0];
        audioSource.Play();
        SlowMotionManager.Instance.TriggerSlowMotion(1f);
        yield return new WaitForSecondsRealtime(resultPanelDelay);
        ShowWinPanel();

    }

    private IEnumerator ShowGameOverRoutine()
    {
        audioSource.resource = audioClips[1];
        audioSource.Play();
        SlowMotionManager.Instance.TriggerSlowMotion(1f);
        yield return new WaitForSecondsRealtime(resultPanelDelay);
        ShowGameOverPanel();
    }
}
