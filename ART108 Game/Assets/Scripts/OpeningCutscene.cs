using UnityEngine;
using TMPro;
using System.Collections;

public class OpeningCutscene : MonoBehaviour
{
    [Header("UI")]
    public CanvasGroup blackScreen;
    public TextMeshProUGUI storyText;

    [Header("Story Lines")]
    [TextArea(2, 4)]
    public string[] storyLines = new string[]
    {
        "When you're first taken out of your packaging, a child will give you a name. You are real now. You are alive. This is the only event in a doll's life that matters.",
        "You can be dressed up. You can be carried around. You can be given attachments. You can be loved.",
        "I was loved.",
        "There is only one other event that ever matters to a doll.",
        "When you are thrown away. It strips you of everything. There is no one to call you by your name.",
        "I've been thrown away."
    };

    [Header("Timing")]
    public float fadeInDuration = 1f;
    public float lineDuration = 3.5f;
    public float fadeOutDuration = 1f;
    public float timeBetweenLines = 0.5f;
    public float finalFadeDuration = 2f;

    [Header("Game Start")]
    public GameObject gameplayUI;  // Enable after cutscene
    public BackgroundMusic gameMusic;  // Start music after cutscene

    private void Start()
    {
        // Disable gameplay initially
        if (gameplayUI != null)
            gameplayUI.SetActive(false);

        // Ensure black screen is visible
        if (blackScreen != null)
            blackScreen.alpha = 1f;

        // Hide text initially
        if (storyText != null)
            SetTextAlpha(0f);

        // Start cutscene
        StartCoroutine(PlayCutscene());
    }

    private IEnumerator PlayCutscene()
    {
        // Wait a moment
        yield return new WaitForSeconds(1f);

        // Show each story line
        foreach (string line in storyLines)
        {
            yield return StartCoroutine(ShowLine(line));
            yield return new WaitForSeconds(timeBetweenLines);
        }

        // Fade to game
        yield return new WaitForSeconds(1f);
        yield return StartCoroutine(FadeToGame());
    }

    private IEnumerator ShowLine(string line)
    {
        storyText.text = line;

        // Fade in text
        float elapsed = 0f;
        while (elapsed < fadeInDuration)
        {
            elapsed += Time.deltaTime;
            SetTextAlpha(Mathf.Lerp(0f, 1f, elapsed / fadeInDuration));
            yield return null;
        }
        SetTextAlpha(1f);

        // Hold
        yield return new WaitForSeconds(lineDuration);

        // Fade out text
        elapsed = 0f;
        while (elapsed < fadeOutDuration)
        {
            elapsed += Time.deltaTime;
            SetTextAlpha(Mathf.Lerp(1f, 0f, elapsed / fadeOutDuration));
            yield return null;
        }
        SetTextAlpha(0f);
    }

    private IEnumerator FadeToGame()
    {
        // Fade out black screen
        float elapsed = 0f;
        while (elapsed < finalFadeDuration)
        {
            elapsed += Time.deltaTime;
            if (blackScreen != null)
                blackScreen.alpha = Mathf.Lerp(1f, 0f, elapsed / finalFadeDuration);
            yield return null;
        }

        if (blackScreen != null)
            blackScreen.alpha = 0f;

        // Enable gameplay
        if (gameplayUI != null)
            gameplayUI.SetActive(true);

        // Start music
        if (gameMusic != null && gameMusic.enabled == false)
        {
            gameMusic.enabled = true;
        }

        // Destroy cutscene UI
        Destroy(gameObject, 0.5f);
    }

    private void SetTextAlpha(float alpha)
    {
        if (storyText != null)
        {
            Color c = storyText.color;
            c.a = alpha;
            storyText.color = c;
        }
    }
}
