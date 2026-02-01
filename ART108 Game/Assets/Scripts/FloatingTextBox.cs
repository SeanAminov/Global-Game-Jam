using UnityEngine;
using TMPro;
using System.Collections;

public class FloatingTextBox : MonoBehaviour
{
    [Header("Message")]
    [TextArea(2, 4)]
    public string message = "Press WASD to move";
    public float displayDuration = 4f;
    public bool triggerOnce = true;

    [Header("Fade Settings")]
    public float fadeInDuration = 0.5f;
    public float fadeOutDuration = 0.5f;
    public float bobAmount = 0.3f;  // Floating bob animation
    public float bobSpeed = 2f;

    [Header("Text Appearance")]
    public Color textColor = Color.white;
    public int fontSize = 36;
    public TextAlignmentOptions alignment = TextAlignmentOptions.Center;

    private TextMeshPro textMesh;
    private bool hasTriggered = false;
    private Vector3 originalPosition;

    private void Start()
    {
        // Create text mesh if not already there
        if (textMesh == null)
        {
            textMesh = GetComponent<TextMeshPro>();
            if (textMesh == null)
            {
                textMesh = gameObject.AddComponent<TextMeshPro>();
            }
        }

        // Setup text
        textMesh.text = message;
        textMesh.fontSize = fontSize;
        textMesh.alignment = alignment;
        textMesh.color = new Color(textColor.r, textColor.g, textColor.b, 0f);  // Start invisible

        originalPosition = transform.position;
    }

    private void Update()
    {
        // Floating bob animation
        float bob = Mathf.Sin(Time.time * bobSpeed) * bobAmount;
        transform.position = originalPosition + Vector3.up * bob;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && (!triggerOnce || !hasTriggered))
        {
            hasTriggered = true;
            StartCoroutine(FadeInOutSequence());
        }
    }

    private IEnumerator FadeInOutSequence()
    {
        // Fade in
        float elapsed = 0f;
        while (elapsed < fadeInDuration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(0f, 1f, elapsed / fadeInDuration);
            SetTextAlpha(alpha);
            yield return null;
        }
        SetTextAlpha(1f);

        // Wait
        yield return new WaitForSeconds(displayDuration);

        // Fade out
        elapsed = 0f;
        while (elapsed < fadeOutDuration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, elapsed / fadeOutDuration);
            SetTextAlpha(alpha);
            yield return null;
        }
        SetTextAlpha(0f);
    }

    private void SetTextAlpha(float alpha)
    {
        if (textMesh != null)
        {
            Color c = textMesh.color;
            c.a = alpha;
            textMesh.color = c;
        }
    }
}
