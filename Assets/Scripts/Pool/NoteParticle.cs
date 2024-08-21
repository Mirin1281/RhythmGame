using UnityEngine;
using Cysharp.Threading.Tasks;

public class NoteParticle : PooledBase
{
    [SerializeField] ParticleSystem particle;

    public async UniTask PlayAsync()
    {
        particle.Play();
        await UniTask.Delay(System.TimeSpan.FromSeconds(0.5f), cancellationToken: destroyCancellationToken);
        if(this == null) return;
        gameObject.SetActive(false);
    }
}
