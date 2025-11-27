using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class SceneLoader : MonoBehaviour
{
    public string sceneToLoad = "Episode1";

    // Start the game → load Episode1
    public void StartGame()
    {
        StopAllCoroutines();
        StartCoroutine(LoadSceneNextFrame(sceneToLoad));
    }

    // Restart current scene
    public void RestartGame()
    {
        StartCoroutine(LoadSceneNextFrame(SceneManager.GetActiveScene().name));
    }

    // Coroutine to load scene safely
    private IEnumerator LoadSceneNextFrame(string sceneName)
    {
        yield return null; // wait one frame to let EventSystem finish
        SceneManager.LoadScene(sceneName);
    }

    // Quit application
    public void QuitGame()
    {
        Application.Quit();
        Debug.Log("Game quit requested");
    }
}
