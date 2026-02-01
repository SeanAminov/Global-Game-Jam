using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using TMPro;

public enum MaskType
{
    Joy,
    Sad,
    Spite
}

public class MaskManager : MonoBehaviour
{
    public static MaskManager Instance { get; private set; }

    // Events for mask equip/unequip
    public event Action<MaskType> OnMaskEquipped;
    public event Action<MaskType> OnMaskUnequipped;

    [Header("UI")]
    public Slider equippedUsageBar;
    public Image equippedFillImage;
    public Slider joyUnequippedUsageBar;
    public Image joyFillImage;
    public Slider sadUnequippedUsageBar;
    public Image sadFillImage;
    public Slider spiteUnequippedUsageBar;
    public Image spiteFillImage;
    public TextMeshProUGUI pickupText;
    public float pickupTextDuration = 2f;

    [Header("Bar Colors")]
    public Color joyColor = Color.yellow;
    public Color sadColor = Color.blue;
    public Color spiteColor = Color.red;

    [Header("Usage Settings")]
    public float maxUsageTime = 10f;
    public float rechargeSpeed = 1f;

    [Header("Equip Keys")]
    public Key joyKey = Key.Digit1;
    public Key sadKey = Key.Digit2;
    public Key spiteKey = Key.Digit3;

    [Header("Animator Override")]
    public Animator playerAnimator;
    public RuntimeAnimatorController defaultController;
    public AnimatorOverrideController joyController;
    public AnimatorOverrideController sadController;
    public AnimatorOverrideController spiteController;

    [Header("Audio")]
    public AudioClip maskPickupSound;
    private AudioSource audioSource;

    [Header("Debug")]
    public bool startWithAllMasks = false;

    private readonly HashSet<MaskType> ownedMasks = new HashSet<MaskType>();
    private MaskType? equippedMask = null;
    private Dictionary<MaskType, float> maskUsageTimes = new Dictionary<MaskType, float>()
    {
        { MaskType.Joy, 0f },
        { MaskType.Sad, 0f },
        { MaskType.Spite, 0f }
    };
    private float pickupTextTimer = 0f;
    private bool maskTakeoverTriggered = false;  // Prevent repeated death calls

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Update()
    {
        HandleInput();
        UpdateUsage();
        UpdatePickupText();
    }

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        if (playerAnimator == null)
        {
            playerAnimator = GetComponent<Animator>();
        }

        if (playerAnimator != null && defaultController == null)
        {
            defaultController = playerAnimator.runtimeAnimatorController;
        }

        if (startWithAllMasks)
        {
            ownedMasks.Add(MaskType.Joy);
            ownedMasks.Add(MaskType.Sad);
            ownedMasks.Add(MaskType.Spite);
        }

        ApplyMaskController(equippedMask);
    }

    public void AddMask(MaskType type, string pickupMessage)
    {
        if (!ownedMasks.Contains(type))
        {
            ownedMasks.Add(type);
            
            // Play pickup sound
            if (audioSource != null && maskPickupSound != null)
            {
                audioSource.PlayOneShot(maskPickupSound);
            }
            
            // Show the unequipped bar for this mask
            switch (type)
            {
                case MaskType.Joy:
                    if (joyUnequippedUsageBar != null)
                        joyUnequippedUsageBar.gameObject.SetActive(true);
                    break;
                case MaskType.Sad:
                    if (sadUnequippedUsageBar != null)
                        sadUnequippedUsageBar.gameObject.SetActive(true);
                    break;
                case MaskType.Spite:
                    if (spiteUnequippedUsageBar != null)
                        spiteUnequippedUsageBar.gameObject.SetActive(true);
                    break;
            }
        }

        if (pickupText != null)
        {
            pickupText.text = pickupMessage;
            pickupText.gameObject.SetActive(true);
            pickupTextTimer = pickupTextDuration;
        }
    }

    private void HandleInput()
    {
        if (Keyboard.current == null)
        {
            return;
        }

        if (Keyboard.current[Key.M].wasPressedThisFrame)
        {
            GiveAllMasks();
        }

        if (Keyboard.current[joyKey].wasPressedThisFrame)
        {
            ToggleMask(MaskType.Joy);
        }

        if (Keyboard.current[sadKey].wasPressedThisFrame)
        {
            ToggleMask(MaskType.Sad);
        }

        if (Keyboard.current[spiteKey].wasPressedThisFrame)
        {
            ToggleMask(MaskType.Spite);
        }
    }

    private void GiveAllMasks()
    {
        ownedMasks.Add(MaskType.Joy);
        ownedMasks.Add(MaskType.Sad);
        ownedMasks.Add(MaskType.Spite);

        if (joyUnequippedUsageBar != null)
            joyUnequippedUsageBar.gameObject.SetActive(true);
        if (sadUnequippedUsageBar != null)
            sadUnequippedUsageBar.gameObject.SetActive(true);
        if (spiteUnequippedUsageBar != null)
            spiteUnequippedUsageBar.gameObject.SetActive(true);

        if (pickupText != null)
        {
            pickupText.text = "All masks unlocked!";
            pickupText.gameObject.SetActive(true);
            pickupTextTimer = pickupTextDuration;
        }
    }

    private void ToggleMask(MaskType type)
    {
        if (!ownedMasks.Contains(type))
        {
            return;
        }

        if (equippedMask == type)
        {
            // Unequip
            maskTakeoverTriggered = false;  // Reset for next equip
            equippedMask = null;
            OnMaskUnequipped?.Invoke(type);
            ApplyMaskController(equippedMask);
        }
        else
        {
            // Unequip previous mask
            if (equippedMask.HasValue)
            {
                OnMaskUnequipped?.Invoke(equippedMask.Value);
            }

            // Equip new mask
            equippedMask = type;
            OnMaskEquipped?.Invoke(type);
            ApplyMaskController(equippedMask);
        }
    }
    public bool IsMaskEquipped(MaskType type)
    {
        return equippedMask == type;
    }
    private void UpdateUsage()
    {
        // Update all masks (copy keys to avoid modifying during enumeration)
        MaskType[] maskTypes = new MaskType[maskUsageTimes.Count];
        maskUsageTimes.Keys.CopyTo(maskTypes, 0);

        foreach (MaskType maskType in maskTypes)
        {
            if (equippedMask == maskType)
            {
                // Equipped: charge up
                maskUsageTimes[maskType] = Mathf.Min(maxUsageTime, maskUsageTimes[maskType] + Time.deltaTime);
                
                // Check if mask reached 100% - take 1 damage (only once)
                if (maskUsageTimes[maskType] >= maxUsageTime && !maskTakeoverTriggered)
                {
                    PlayerHealth playerHealth = GetComponent<PlayerHealth>();
                    if (playerHealth != null && !playerHealth.godMode)
                    {
                        maskTakeoverTriggered = true;
                        // Apply damage instead of death
                        Vector2 knockbackDir = Vector2.up;
                        playerHealth.ApplyDamage(1, knockbackDir, 5f);
                        
                        // Reset mask bar after damage
                        maskUsageTimes[maskType] = 0f;
                        maskTakeoverTriggered = false;  // Allow it to happen again
                    }
                }
            }
            else
            {
                // Not equipped: drain down
                maskUsageTimes[maskType] = Mathf.Max(0f, maskUsageTimes[maskType] - Time.deltaTime * rechargeSpeed);
            }
        }

        // Update equipped bar (shows only when mask is equipped)
        if (equippedMask.HasValue)
        {
            if (equippedUsageBar != null)
            {
                equippedUsageBar.gameObject.SetActive(true);
                UpdateBar(equippedMask.Value, equippedUsageBar);
                
                // Update equipped bar fill color to match the mask
                if (equippedFillImage != null)
                {
                    Color equipColor = equippedMask.Value == MaskType.Joy ? joyColor :
                                       equippedMask.Value == MaskType.Sad ? sadColor :
                                       spiteColor;
                    equippedFillImage.color = equipColor;
                }
            }
        }
        else
        {
            if (equippedUsageBar != null)
            {
                equippedUsageBar.gameObject.SetActive(false);
            }
        }

        // Update unequipped bars (show only owned masks)
        if (joyUnequippedUsageBar != null)
        {
            if (joyFillImage != null) joyFillImage.color = joyColor;
            joyUnequippedUsageBar.gameObject.SetActive(ownedMasks.Contains(MaskType.Joy));
            if (ownedMasks.Contains(MaskType.Joy))
                UpdateBar(MaskType.Joy, joyUnequippedUsageBar);
        }

        if (sadUnequippedUsageBar != null)
        {
            if (sadFillImage != null) sadFillImage.color = sadColor;
            sadUnequippedUsageBar.gameObject.SetActive(ownedMasks.Contains(MaskType.Sad));
            if (ownedMasks.Contains(MaskType.Sad))
                UpdateBar(MaskType.Sad, sadUnequippedUsageBar);
        }

        if (spiteUnequippedUsageBar != null)
        {
            if (spiteFillImage != null) spiteFillImage.color = spiteColor;
            spiteUnequippedUsageBar.gameObject.SetActive(ownedMasks.Contains(MaskType.Spite));
            if (ownedMasks.Contains(MaskType.Spite))
                UpdateBar(MaskType.Spite, spiteUnequippedUsageBar);
        }
    }

    private void UpdateBar(MaskType maskType, Slider bar)
    {
        if (bar == null)
            return;

        // Ensure max value is 1 for proper fill
        if (bar.maxValue != 1f)
            bar.maxValue = 1f;

        float normalized = maxUsageTime > 0f ? maskUsageTimes[maskType] / maxUsageTime : 0f;
        normalized = Mathf.Clamp01(normalized);  // Clamp between 0-1 to prevent over-fill issues
        bar.value = normalized;
    }

    private void UpdatePickupText()
    {
        if (pickupText == null || pickupTextTimer <= 0f)
        {
            return;
        }

        pickupTextTimer -= Time.deltaTime;
        if (pickupTextTimer <= 0f)
        {
            pickupText.gameObject.SetActive(false);
        }
    }

    private void ApplyMaskController(MaskType? maskType)
    {
        if (playerAnimator == null)
        {
            return;
        }

        RuntimeAnimatorController targetController = defaultController;

        if (maskType.HasValue)
        {
            switch (maskType.Value)
            {
                case MaskType.Joy:
                    targetController = joyController != null ? joyController : defaultController;
                    break;
                case MaskType.Sad:
                    targetController = sadController != null ? sadController : defaultController;
                    break;
                case MaskType.Spite:
                    targetController = spiteController != null ? spiteController : defaultController;
                    break;
            }
        }

        if (targetController != null && playerAnimator.runtimeAnimatorController != targetController)
        {
            playerAnimator.runtimeAnimatorController = targetController;
        }
    }
}
