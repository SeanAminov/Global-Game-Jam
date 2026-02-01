using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class StartScreenManager : MonoBehaviour
{
    [Header("UI References")]
    public Button startButton;
    
    [Header("Scene Names")]
    public string gameSceneName = "GameScene";  // Change to your game scene name

    private void Start()
    {
        // Auto-find button if not assigned
        if (startButton == null)
        {
            startButton = GetComponentInChildren<Button>();
        }

        // Hook up button click
        if (startButton != null)
        {
            startButton.onClick.AddListener(StartGame);
            Debug.Log("Start button ready!");
        }
        else
        {
            Debug.LogWarning("Start button not found!");
        }
    }

    public void StartGame()
    {
        Debug.Log($"Loading scene: {gameSceneName}");
        SceneManager.LoadScene(gameSceneName);
    }
}
