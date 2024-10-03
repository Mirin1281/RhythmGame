using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneChanger : MonoBehaviour
{
    [SerializeField] string sceneName = "InGame";
    public void LoadScene()
    {
        FadeLoadSceneManager.Instance.LoadScene(0.5f, sceneName, 0.5f, Color.white);
    }
}
