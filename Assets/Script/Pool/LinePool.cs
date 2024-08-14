using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LinePool : PoolBase<Line>
{
    public Line GetLine(int index = 0) => GetInstance(index);
}
