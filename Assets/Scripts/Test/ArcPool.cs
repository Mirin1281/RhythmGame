using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArcPool : PoolBase<ArcNote>
{
    public ArcNote GetNote()
    {
        return GetInstance();
    }
}
