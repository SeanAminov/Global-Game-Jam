using UnityEngine;

public class Masks : MonoBehaviour, IITEm
{
    [Header("Mask Settings")]
    public MaskType maskType = MaskType.Joy;
    public string pickupMessage = "Picked up the joy mask - press 1 to equip";
    public Color maskTint = Color.yellow;

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
}
