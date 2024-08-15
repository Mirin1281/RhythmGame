using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArcPool : PoolBase<ArcNote>
{
    [SerializeField] GameObject lane3D;

    public ArcNote GetNote()
    {
        var arc = GetInstance();
        arc.transform.parent = lane3D.transform;
        return arc;
    }
}
