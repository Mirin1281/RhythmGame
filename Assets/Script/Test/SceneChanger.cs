using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneChanger : MonoBehaviour
{
    [SerializeField] string sceneName = "Test_RhythmGame";
    public void OnPress()
    {
        SceneManager.LoadScene(sceneName);
    }
}
