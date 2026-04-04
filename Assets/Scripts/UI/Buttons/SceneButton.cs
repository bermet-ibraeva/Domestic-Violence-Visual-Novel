using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneButton : MonoBehaviour
{
    [Tooltip("Имя или индекс сцены, которую нужно загрузить")]
    public string sceneName;
    public int sceneIndex = -1;

    public void GoToScene()
    {
        // Если указан индекс — используем его
        if (sceneIndex >= 0)
        {
            SceneManager.LoadScene(sceneIndex);
            return;
        }

        // Иначе пробуем загрузить сцену по имени
        if (!string.IsNullOrEmpty(sceneName))
        {
            SceneManager.LoadScene(sceneName);
            return;
        }

        Debug.LogWarning("SceneButton: Ни имя, ни индекс сцены не указаны!");
    }
}
