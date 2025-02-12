using System.Collections.Generic;
using UnityEngine;

namespace NoteCreating
{
    // TODO: アークに依存しない設計
    public class ArcLightOperator : MonoBehaviour
    {
        [SerializeField] GameObject[] lights;
        readonly Dictionary<ArcNote, GameObject> lightDic = new(4);

        GameObject GetLight(ArcNote arcNote)
        {
            if (lightDic.TryGetValue(arcNote, out var particle))
            {
                return particle;
            }
            else
            {
                foreach (var p in lights)
                {
                    if (lightDic.ContainsValue(p)) continue;
                    lightDic.Add(arcNote, p);
                    return p;
                }
                return null;
            }
        }
        public void SetShowLight(ArcNote arcNote, Vector2 pos, bool enabled)
        {
            GameObject light = GetLight(arcNote);
            if (light != null)
            {
                light.SetActive(enabled);
                if (enabled)
                {
                    light.transform.localPosition = pos;
                }
            }
        }
        public void RemoveLink(ArcNote arcNote)
        {
            if (lightDic.TryGetValue(arcNote, out var p) == false) return;
            p.SetActive(false);
            lightDic.Remove(arcNote);
        }
    }
}