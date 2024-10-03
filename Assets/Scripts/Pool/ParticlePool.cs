using Cysharp.Threading.Tasks;
using UnityEngine;

public class ParticlePool : PoolBase<NoteParticle>
{
    [SerializeField] Transform parent;

    public void PlayParticle(Vector2 pos)
    {
        var p = GetInstance();
        p.transform.SetParent(parent);
        p.transform.localPosition = new Vector3(pos.x, pos.y);
        p.transform.localScale = Vector3.one;
        p.PlayAsync().Forget();
    }
}
