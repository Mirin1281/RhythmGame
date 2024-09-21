using UnityEngine;
using Cysharp.Threading.Tasks;

public class NoteParticle : PooledBase
{
    [SerializeField] ParticleSystem particle;

    public async UniTask PlayAsync()
    {
        particle.Play();
        await MyUtility.WaitSeconds(0.4f, destroyCancellationToken);
        if(this == null) return;
        gameObject.SetActive(false);
    }
}
