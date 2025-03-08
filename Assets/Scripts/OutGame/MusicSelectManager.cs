using UnityEngine;
using Cysharp.Threading.Tasks;
using UnityEngine.AddressableAssets;

public class MusicSelectManager : MonoBehaviour
{
    [SerializeField] MusicButtonManager musicButtonManager;
    [SerializeField] DifficultyGroup difficultyGroup;
    [SerializeField] DarkSetter darkSetter;

    void Awake()
    {
        Init();
        darkSetter.InitOnAwake();
    }

    public void Init()
    {
        musicButtonManager.Init().Forget();
        difficultyGroup.Init();
    }
}
