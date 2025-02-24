using Novel;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Mirin.Story
{
    public class StorySceneManager : MonoBehaviour
    {
        [SerializeField] bool isCreateManagar;

        void Awake()
        {
            var managerPrefab = Addressables.LoadAssetAsync<GameObject>(nameof(NovelManager)).WaitForCompletion();
            var novelManager = Instantiate(managerPrefab).GetComponent<NovelManager>();
            novelManager.name = managerPrefab.name;
            DontDestroyOnLoad(novelManager);
            novelManager.CreateManagers();
        }
    }
}