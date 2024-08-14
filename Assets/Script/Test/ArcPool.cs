using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArcPool : PoolBase<ArcNote>
{
    public ArcNote GetNote(ArcCreateData[] datas, float bpm, float speed)
    {
        var arc = GetNote();
        arc.CreateNewArc(datas, bpm, speed);
        return arc;
    }
    public ArcNote GetNote()
    {
        return GetInstance();
    }
}
