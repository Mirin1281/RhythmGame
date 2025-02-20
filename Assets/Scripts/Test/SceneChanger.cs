using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneChanger : MonoBehaviour
{
    [SerializeField] string sceneName = "InGame";
    [SerializeField] SEType seType = SEType.None;

    public void LoadScene()
    {
        FadeLoadSceneManager.Instance.LoadScene(0.5f, sceneName, 0.5f);
        if (seType != SEType.None)
        {
            SEManager.Instance.PlaySE(seType);
        }
    }
}
