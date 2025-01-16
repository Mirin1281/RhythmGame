using UnityEngine;
using Cysharp.Threading.Tasks;

namespace NoteCreating
{
    public class NoteParticle : MonoBehaviour, IPoolable
    {
        [SerializeField] ParticleSystem particle;

        bool isActiveForPool;
        public bool IsActiveForPool => isActiveForPool;
        public bool IsActive => gameObject.activeInHierarchy;

        void OnEnable()
        {
            isActiveForPool = true;
        }

        // プール時に2フレーム開けるとバグらない
        async UniTaskVoid OnDisable()
        {
            await UniTask.DelayFrame(2, cancellationToken: destroyCancellationToken);
            isActiveForPool = false;
        }

        public void SetActive(bool enabled)
        {
            gameObject.SetActive(enabled);
        }

        public async UniTask PlayAsync()
        {
            particle.Play();
            await MyUtility.WaitSeconds(0.4f, destroyCancellationToken);
            if (this == null) return;
            gameObject.SetActive(false);
        }
    }
}