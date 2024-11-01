using System.Collections.Generic;
using UnityEngine;

public class ArcLightOperator : MonoBehaviour
{
    [SerializeField] LightParticle[] lights;
    readonly Dictionary<ArcNote, LightParticle> lightDic = new(4);
    
    LightParticle GetLight(ArcNote arcNote)
    {
        if(lightDic.TryGetValue(arcNote, out var particle))
        {
            return particle;
        }
        else
        {
            foreach(var p in lights)
            {
                if(lightDic.ContainsValue(p)) continue;
                lightDic.Add(arcNote, p);
                return p;
            }
            return null;
        }
    }
    public void SetShowLight(ArcNote arcNote, Vector2 pos, bool enabled)
    {
        LightParticle light = GetLight(arcNote);
        if(light != null)
        {
            light.IsActive = enabled;
            if(enabled)
            {
                light.SetPos(pos);
            }
        }
    }
    public void RemoveLink(ArcNote arcNote)
    {
        if(lightDic.TryGetValue(arcNote, out var p) == false) return;
        p.IsActive = false;
        lightDic.Remove(arcNote);
    }
}
