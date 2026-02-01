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

    [Header("UI")]
    public Slider equippedUsageBar;
    public Slider unequippedUsageBar;
    public TextMeshProUGUI pickupText;
    public float pickupTextDuration = 2f;

    [Header("Usage Settings")]
    public float maxUsageTime = 5f;
    public float rechargeSpeed = 1f;

    [Header("Equip Keys")]
    public Key joyKey = Key.Digit1;
    public Key sadKey = Key.Digit2;
    public Key spiteKey = Key.Digit3;

    private readonly HashSet<MaskType> ownedMasks = new HashSet<MaskType>();
    private MaskType? equippedMask = null;
    private float usageTime = 0f;
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
            equippedMask = null;
        }
        else
        {
            equippedMask = type;
        }
    }
    public bool IsMaskEquipped(MaskType type)
    {
        return equippedMask == type;
    }
    private void UpdateUsage()
    {
        if (equippedMask.HasValue)
        {
            usageTime = Mathf.Min(maxUsageTime, usageTime + Time.deltaTime);
            SetBarVisibility(true);
        }
        else
        {
            usageTime = Mathf.Max(0f, usageTime - Time.deltaTime * rechargeSpeed);
            SetBarVisibility(false);
        }

        float normalized = maxUsageTime > 0f ? usageTime / maxUsageTime : 0f;

        if (equippedUsageBar != null)
        {
            equippedUsageBar.value = normalized;
        }

        if (unequippedUsageBar != null)
        {
            unequippedUsageBar.value = normalized;
        }
    }

    private void SetBarVisibility(bool equipped)
    {
        if (equippedUsageBar != null)
        {
            equippedUsageBar.gameObject.SetActive(equipped);
        }

        if (unequippedUsageBar != null)
        {
            unequippedUsageBar.gameObject.SetActive(!equipped);
        }
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
}
