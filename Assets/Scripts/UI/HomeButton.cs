using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class HomeButton : MonoBehaviour
{
    public string sceneName = "MainMenu"; // сюда впиши имя твоей сцены

    public void GoHome()
    {
        SceneManager.LoadScene(sceneName);
    }
}
