using UnityEngine;
using Cysharp.Threading.Tasks;

public class NoteParticle : PooledBase
{
    [SerializeField] ParticleSystem particle;

    public void Play() => PlayAsync().Forget();
    async UniTask PlayAsync()
    {
        particle.Play();
        await MyStatic.WaitSeconds(0.5f);
        if(this == null) return;
        gameObject.SetActive(false);
    }
}
