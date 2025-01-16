using Cysharp.Threading.Tasks;
using UnityEngine;

namespace NoteCreating
{
    public class ParticlePool : PoolBase<NoteParticle>
    {
        [SerializeField] Transform parent;

        public void PlayParticle(Vector2 pos, NoteGrade noteGrade)
        {
            int index = noteGrade switch
            {
                NoteGrade.Perfect => 0,
                NoteGrade.FastGreat or NoteGrade.LateGreat => 1,
                NoteGrade.FastFar or NoteGrade.LateFar => 2,
                NoteGrade.Miss => -1,
                _ => throw new System.Exception()
            };
            if (index == -1) return;

            var p = GetInstance(index);

            p.transform.SetParent(parent);
            p.transform.localPosition = new Vector3(pos.x, pos.y);
            p.transform.localScale = Vector3.one;

            p.PlayAsync().Forget();
        }
    }
}