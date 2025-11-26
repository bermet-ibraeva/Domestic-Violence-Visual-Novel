using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class SceneLoader : MonoBehaviour
{
    public string sceneToLoad = "Episode1";

    // Start the game → load Episode1
    public void StartGame()
    {
        Debug.Log("StartGame called! Loading scene: " + sceneToLoad);
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
        yield return null; // Wait one frame so EventSystem finishes
        SceneManager.LoadScene(sceneName);
    }

    // Quit application
    public void QuitGame()
    {
        Application.Quit();
        Debug.Log("Game quit requested"); 
    }
}
