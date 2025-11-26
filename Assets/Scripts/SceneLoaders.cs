using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    public string sceneToLoad = "Episode1";

    // Start the game → load Episode1
    public void StartGame()
    {
        SceneManager.LoadScene(sceneToLoad);
    }

    // Restart current scene
    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    // Quit application
    public void QuitGame()
    {
        Application.Quit();
        Debug.Log("Game quit requested"); 
    }
}
