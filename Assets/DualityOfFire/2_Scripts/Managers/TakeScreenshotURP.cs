using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class SplitScreenPause : MonoBehaviour
{
    [Header("UI References")]
    public Image leftHalf;
    public Image rightHalf;
    public GameObject transitionPanel;

    [Header("Settings")]
    public float splitSpeed = 0.5f;
    public float splitDistance = 500f;

    // public Image bgImage;
    private Texture2D screenshot;

    private Coroutine pauseCoroutine, resumeCoroutine;
    private Vector2 leftDefAnchorPos, rightDefAnchorPos;
    private RectTransform leftRect, rightRect;

    void Start()
    {
        if (transitionPanel != null)
            transitionPanel.SetActive(false);
    }

    // Call this from your pause button
    public void PauseButton(bool isPaused)
    {
        if (isPaused)
        {
            if (resumeCoroutine != null)
            {
                StopCoroutine(resumeCoroutine);
                resumeCoroutine = null;
            }
            if (pauseCoroutine == null)
            {
                pauseCoroutine = StartCoroutine(DoPauseEffect());
            }

        }
        else
        {

            if (pauseCoroutine != null)
            {
                StopCoroutine(pauseCoroutine);
                pauseCoroutine = null;
            }
            if (resumeCoroutine == null)
            {

                resumeCoroutine = StartCoroutine(DoResumeEffect());
            }
        }
    }

    private IEnumerator DoPauseEffect()
    {
        yield return new WaitForEndOfFrame();

        screenshot = TakeScreenshot();
        // bgImage.color = Color.black;
        UIManager.Instance.DisableGun();
        int halfWidth = screenshot.width / 2;
        int height = screenshot.height;

        // LEFT sprite
        Sprite leftSprite = Sprite.Create(
            screenshot,
            new Rect(0, 0, halfWidth, height),
            new Vector2(1f, 0.5f), // pivot (center)
            100f // pixels per unit
        );

        // RIGHT sprite
        Sprite rightSprite = Sprite.Create(
            screenshot,
            new Rect(halfWidth, 0, halfWidth, height),
            new Vector2(0f, 0.5f),
            100f
        );

        leftHalf.sprite = leftSprite;
        rightHalf.sprite = rightSprite;


        leftRect = leftHalf.GetComponent<RectTransform>();
        rightRect = rightHalf.GetComponent<RectTransform>();

        leftHalf.preserveAspect = true;
        rightHalf.preserveAspect = true;
        Canvas canvas = leftHalf.canvas;
        float canvasWidth = canvas.GetComponent<RectTransform>().rect.width;

        // LEFT
        leftRect.anchorMin = new Vector2(0f, 0f);
        leftRect.anchorMax = new Vector2(0f, 1f);
        leftRect.pivot = new Vector2(0f, 0.5f);
        leftRect.sizeDelta = new Vector2(canvasWidth / 2f, 0f);
        leftRect.anchoredPosition = Vector2.zero;

        // RIGHT
        rightRect.anchorMin = new Vector2(1f, 0f);
        rightRect.anchorMax = new Vector2(1f, 1f);
        rightRect.pivot = new Vector2(1f, 0.5f);
        rightRect.sizeDelta = new Vector2(canvasWidth / 2f, 0f);
        rightRect.anchoredPosition = Vector2.zero;

        transitionPanel.SetActive(true);
        CustomAnimations.Pulse(transitionPanel.transform.Find("PauseMenuSlabBackground").transform, 5f);
        UIManager.Instance.ShowPausePanel(true);

        leftDefAnchorPos = leftRect.anchoredPosition;
        rightDefAnchorPos = rightRect.anchoredPosition;

        float timeElapsed = 0f;

        while (timeElapsed < splitSpeed)
        {
            float progress = timeElapsed / splitSpeed; Debug.Log(progress);

            leftRect.anchoredPosition = Vector2.Lerp(leftDefAnchorPos, new Vector2(-splitDistance, 0), progress);
            rightRect.anchoredPosition = Vector2.Lerp(rightDefAnchorPos, new Vector2(splitDistance, 0), progress);

            // transitionPanel.transform.Find("PauseMenuSlabBackground/Neon").GetComponent<Image>().fillAmount += progress/20;

            yield return null;
            timeElapsed += Time.unscaledDeltaTime;
        }
        timeElapsed = 0f;

        while (timeElapsed < 2f)
        {
            float progress = timeElapsed / splitSpeed * 0.1f;
            Debug.Log(progress);
            transitionPanel.transform.Find("PauseMenuSlabBackground/Neon").GetComponent<Image>().fillAmount += progress;

            yield return null;
            timeElapsed += Time.unscaledDeltaTime;
        }


        pauseCoroutine = null;

    }



    private IEnumerator DoResumeEffect()
    {
        UIManager.Instance.ShowPausePanel(false);
        Time.timeScale = 0;
        float timeElapsed = 0f;
        UIManager.Instance.gamePlayPanel.SetActive(false);


        Vector2 currentLeft = leftRect.anchoredPosition;
        Vector2 currentRight = rightRect.anchoredPosition;

        transitionPanel.transform.Find("PauseMenuSlabBackground").transform.DOScale(0f, 0.5f).SetUpdate(true);

        while (timeElapsed < splitSpeed)
        {
            float progress = timeElapsed / splitSpeed;

            leftRect.anchoredPosition = Vector2.Lerp(currentLeft, leftDefAnchorPos, progress);
            rightRect.anchoredPosition = Vector2.Lerp(currentRight, rightDefAnchorPos, progress);


            yield return null;
            timeElapsed += Time.unscaledDeltaTime;
            Debug.Log(timeElapsed);
        }

        Time.timeScale = 1;
        transitionPanel.SetActive(false);
        UIManager.Instance.gamePlayPanel.SetActive(true);
        transitionPanel.transform.Find("PauseMenuSlabBackground/Neon").GetComponent<Image>().fillAmount = 0;

        //GameManager.Instance.guns.SetActive(true); -------- guns position...
        UIManager.Instance.EnableGun();
        resumeCoroutine = null;

    }

    private Texture2D TakeScreenshot()
    {
        Texture2D texture = new Texture2D(Screen.width, Screen.height, TextureFormat.ARGB32, false);

        texture.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);

        texture.Apply();

        // byte[] byteArray = texture.EncodeToPNG();
        // System.IO.File.WriteAllBytes(Application.dataPath + "/Screenshottt.png", byteArray);

        return texture;
    }

    private void OnDestroy()
    {
        if (screenshot != null)
            Destroy(screenshot);
    }
}
