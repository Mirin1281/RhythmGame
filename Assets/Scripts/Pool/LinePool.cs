using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LinePool : PoolBase<Line>
{
    public Line GetLine(int index = 0)
    {
        var line = GetInstance();

        if(index == 0)
        {
            line.SetWidth(20f);
            line.SetHeight(0.06f);
            line.transform.localRotation = default;
        }
        else if(index == 1)
        {
            line.SetWidth(13f);
            line.SetHeight(0.2f);
            line.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
        }
        line.transform.SetParent(this.transform);
        line.SetRendererEnabled(true);
        line.SetAlpha(0.25f);
        
        return line;
    } 

    public List<Line> GetAllLine(int index = 0)
    {
        return PooledTable[index];
    }
}
