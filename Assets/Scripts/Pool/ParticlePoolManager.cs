using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleManager : MonoBehaviour
{
    [SerializeField] ParticlePool perfectPool;
    [SerializeField] ParticlePool greatPool;
    [SerializeField] ParticlePool farPool;

    public void PlayParticle(NoteGrade grade, Vector2 pos)
    {
        var pool = grade switch
        {
            NoteGrade.Perfect => perfectPool,
            NoteGrade.FastGreat or NoteGrade.LateGreat => greatPool,
            NoteGrade.FastFar or NoteGrade.LateFar => farPool,
            _ => null
        };
        if(pool == null) return;
        pool.PlayParticle(pos);
    }
}
