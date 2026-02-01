using UnityEngine;
using TMPro;
using System.Collections;

public class TextBoxTrigger : MonoBehaviour
{
    [Header("UI")]
    public GameObject textBoxPanel;
    public TextMeshProUGUI messageText;

    [Header("Message")]
    [TextArea(3, 5)]
    public string message = "Enter your message here...";
    public float displayDuration = 3f;
    public bool triggerOnce = true;

    [Header("Fade Settings")]
    public float fadeInDuration = 0.5f;
    public float fadeOutDuration = 0.5f;

    private CanvasGroup canvasGroup;
    private bool hasTriggered = false;

    private void Start()
    {
        if (textBoxPanel != null)
        {
            canvasGroup = textBoxPanel.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = textBoxPanel.AddComponent<CanvasGroup>();
            }
            canvasGroup.alpha = 0f;
            textBoxPanel.SetActive(false);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && (!triggerOnce || !hasTriggered))
        {
            hasTriggered = true;
            ShowMessage();
        }
    }

    private void ShowMessage()
    {
        if (textBoxPanel != null && messageText != null)
        {
            messageText.text = message;
            StartCoroutine(FadeInOutSequence());
        }
    }

    private IEnumerator FadeInOutSequence()
    {
        textBoxPanel.SetActive(true);

        // Fade in
        float elapsed = 0f;
        while (elapsed < fadeInDuration)
        {
            elapsed += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsed / fadeInDuration);
            yield return null;
        }
        canvasGroup.alpha = 1f;

        // Wait
        yield return new WaitForSeconds(displayDuration);

        // Fade out
        elapsed = 0f;
        while (elapsed < fadeOutDuration)
        {
            elapsed += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(1f, 0f, elapsed / fadeOutDuration);
            yield return null;
        }
        canvasGroup.alpha = 0f;
        textBoxPanel.SetActive(false);
    }
}
