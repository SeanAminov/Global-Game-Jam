using UnityEngine;

public class Masks : MonoBehaviour, IITEm
{
    [Header("Mask Settings")]
    public MaskType maskType = MaskType.Joy;
    [TextArea(1, 2)]
    public string pickupMessage = "Picked up the joy mask - press 1 to equip";
    public Color maskTint = Color.yellow;

    [Header("Auto-Configure (set maskType and click context menu)")]
    [ContextMenuItem("Auto Configure Joy", "ConfigureJoy")]
    [ContextMenuItem("Auto Configure Sad", "ConfigureSad")]
    [ContextMenuItem("Auto Configure Spite", "ConfigureSpite")]
    public bool autoConfigured = false;

    [Header("Visual Pulse")]
    public Color colorA = Color.white;
    public Color colorB = Color.black;
    public float pulseSpeed = 2f;

    [Header("Pickup Tint")]
    public SpriteRenderer playerSprite;
    public Color pickupTint = Color.yellow;
    public float pickupTintDuration = 1f;

    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        if (spriteRenderer == null)
        {
            return;
        }

        float t = Mathf.PingPong(Time.time * pulseSpeed, 1f);
        spriteRenderer.color = Color.Lerp(colorA, colorB, t);
    }

    public void Collect()
    {
        if (MaskManager.Instance != null)
        {
            MaskManager.Instance.AddMask(maskType, pickupMessage);
        }

        if (playerSprite != null)
        {
            StartCoroutine(ApplyPickupTint());
        }

        Destroy(gameObject);
    }

    private System.Collections.IEnumerator ApplyPickupTint()
    {
        Color originalColor = playerSprite.color;
        playerSprite.color = maskTint;
        yield return new WaitForSeconds(pickupTintDuration);
        playerSprite.color = originalColor;
    }

    private void ConfigureJoy()
    {
        maskType = MaskType.Joy;
        pickupMessage = "Picked up the Joy mask - press 1 to equip";
        maskTint = Color.yellow;
        colorA = Color.yellow;
        colorB = new Color(1f, 0.8f, 0f);
        autoConfigured = true;
    }

    private void ConfigureSad()
    {
        maskType = MaskType.Sad;
        pickupMessage = "Picked up the Sadness mask - press 2 to equip";
        maskTint = Color.blue;
        colorA = Color.blue;
        colorB = Color.cyan;
        autoConfigured = true;
    }

    private void ConfigureSpite()
    {
        maskType = MaskType.Spite;
        pickupMessage = "Picked up the Spite mask - press 3 to equip";
        maskTint = Color.red;
        colorA = Color.red;
        colorB = new Color(0.5f, 0f, 0f);
        autoConfigured = true;
    }
}
