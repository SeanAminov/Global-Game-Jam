using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class CutsceneManager : MonoBehaviour
{
    [Header("Cutscene Settings")]
    public string gameSceneName = "SampleScene";
    public float cutsceneDuration = 16f;

    [Header("Visual Elements")]
    public TextMeshProUGUI storyTextUI;

    [Header("Story Text")]
    [TextArea(10, 20)]
    public string storyText = "When you're first taken out of your packaging, a child will give you a name. You are real now. You are alive. This is the only event in a doll's life that matters.\n\nYou can be dressed up. You can be carried around. You can be given attachments. You can be loved.\n\nI was loved.\n\nThere is only one other event that ever matters to a doll.\n\nWhen you are thrown away. It strips you of everything. There is no one to call you by your name.\n\nI've been thrown away.";

    private void Start()
    {
        if (storyTextUI != null)
        {
            storyTextUI.text = storyText;
        }
        
        StartCoroutine(PlayCutscene());
    }

    private IEnumerator PlayCutscene()
    {
        yield return new WaitForSeconds(cutsceneDuration);
        UnityEngine.SceneManagement.SceneManager.LoadScene(gameSceneName);
    }

    private void Update()
    {
        if (Input.anyKeyDown)
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(gameSceneName);
        }
    }
}
