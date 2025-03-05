using UnityEngine;
using Cysharp.Threading.Tasks;
using UnityEngine.AddressableAssets;

public class MusicSelectManager : MonoBehaviour
{
    [SerializeField] MusicButtonManager musicButtonManager;
    [SerializeField] DifficultyGroup difficultyGroup;

    void Awake()
    {
        Init();
    }

    public void Init()
    {
        musicButtonManager.Init().Forget();
        difficultyGroup.Init();
    }
}
