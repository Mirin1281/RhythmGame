using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LinePool : PoolBase<Line>
{
    public Line GetLine(int index = 0)
    {
        var line = GetInstance(index);
        if(index == 0)
        {
            line.transform.localScale = new Vector3(20f, 0.03f, 1);
        }
        else if(index == 1)
        {
            line.transform.localScale = new Vector3(13f, 0.2f, 1);
            line.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
        }
        line.transform.SetParent(this.transform);
        return line;
    } 
}
