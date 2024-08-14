using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticlePool : PoolBase<NoteParticle>
{
    public void PlayParticle(Vector2 pos)
    {
        var p = GetInstance();
        p.transform.localPosition = new Vector3(pos.x, pos.y);
        p.Play();
    }
}
