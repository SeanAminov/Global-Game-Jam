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
    public Slider joyUsageBar;
    public Slider sadUsageBar;
    public Slider spiteUsageBar;
    public TextMeshProUGUI pickupText;
    public float pickupTextDuration = 2f;

    [Header("Usage Settings")]
    public float maxUsageTime = 5f;
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

    private readonly HashSet<MaskType> ownedMasks = new HashSet<MaskType>();
    private MaskType? equippedMask = null;
    private Dictionary<MaskType, float> maskUsageTimes = new Dictionary<MaskType, float>()
    {
        { MaskType.Joy, 0f },
        { MaskType.Sad, 0f },
        { MaskType.Spite, 0f }
    };
    private float pickupTextTimer = 0f;

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
        if (playerAnimator == null)
        {
            playerAnimator = GetComponent<Animator>();
        }

        if (playerAnimator != null && defaultController == null)
        {
            defaultController = playerAnimator.runtimeAnimatorController;
        }

        ApplyMaskController(equippedMask);
    }

    public void AddMask(MaskType type, string pickupMessage)
    {
        if (!ownedMasks.Contains(type))
        {
            ownedMasks.Add(type);
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

    private void ToggleMask(MaskType type)
    {
        if (!ownedMasks.Contains(type))
        {
            return;
        }

        if (equippedMask == type)
        {
            // Unequip
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
        // Update all masks
        foreach (MaskType maskType in maskUsageTimes.Keys)
        {
            if (equippedMask == maskType)
            {
                // Equipped: charge up
                maskUsageTimes[maskType] = Mathf.Min(maxUsageTime, maskUsageTimes[maskType] + Time.deltaTime);
            }
            else
            {
                // Not equipped: drain down
                maskUsageTimes[maskType] = Mathf.Max(0f, maskUsageTimes[maskType] - Time.deltaTime * rechargeSpeed);
            }
        }

        // Update UI bars
        UpdateBar(MaskType.Joy, joyUsageBar);
        UpdateBar(MaskType.Sad, sadUsageBar);
        UpdateBar(MaskType.Spite, spiteUsageBar);
    }

    private void UpdateBar(MaskType maskType, Slider bar)
    {
        if (bar == null)
            return;

        float normalized = maxUsageTime > 0f ? maskUsageTimes[maskType] / maxUsageTime : 0f;
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
