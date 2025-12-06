using UnityEngine;
using UnityEngine.SceneManagement;

public class HomeButton : MonoBehaviour
{
    public void GoHome()
    {
        SceneManager.LoadScene(0);  // грузим сцену с индексом 0
    }
}
