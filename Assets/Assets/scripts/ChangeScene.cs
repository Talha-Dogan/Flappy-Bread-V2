using UnityEngine;
using UnityEngine.SceneManagement; // Sahne yönetimi için gerekli

public class ChangeScene : MonoBehaviour
{
    public void LoadNewScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }
}
