using UnityEngine;

public class VictoryTrigger : MonoBehaviour
{
    public GameObject victoryUI;  // Assign victory panel
    
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            ShowVictory();
        }
    }
    
    private void ShowVictory()
    {
        if (victoryUI != null)
        {
            victoryUI.SetActive(true);
            Time.timeScale = 0f;  // Pause game
        }
    }
}
