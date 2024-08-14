using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneChanger : MonoBehaviour
{
    [SerializeField] string sceneName = "InGame";
    public void LoadScene()
    {
        SceneManager.LoadScene(sceneName);
    }
}
