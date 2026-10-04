using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    [Header("UI Elements")]
    public GameObject playButton;
    public GameObject quitButton;
    public GameObject cutscenePanel; // Twój obiekt 'Cutscene' z hierarchii

    [Header("Settings")]
    public float cutsceneDuration = 15f;
    public string nextSceneName = "SampleScene";

    void Start()
    {
        // Na starcie upewniamy się, że cutscene jest ukryty
        if (cutscenePanel != null)
        {
            cutscenePanel.SetActive(false);
        }
    }

    // Podpnij pod przycisk PLAY
    public void PlayGame()
{
    // Pokazujemy cutscenę (zakryje całe menu, jeśli jej Image jest na cały ekran)
    if (cutscenePanel != null)
    {
        cutscenePanel.SetActive(true);
    }

    // Startujemy odliczanie 15 sekund
    StartCoroutine(LoadSceneAfterDelay());
}
    // Podpnij pod przycisk QUIT
    public void QuitGame()
    {
        Debug.Log("Wyjście z gry!");
        Application.Quit();
    }

    private IEnumerator LoadSceneAfterDelay()
    {
        yield return new WaitForSeconds(cutsceneDuration);
        SceneManager.LoadScene(nextSceneName);
    }
}