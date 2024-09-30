using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// アークの取得時に出るエフェクト
/// </summary>
public class LightParticle : MonoBehaviour
{
    bool isActive;
    public bool IsActive
    {
        set
        {
            if(isActive == value) return;
            isActive = value;
            gameObject.SetActive(isActive);
        }
    }

    public void SetPos(Vector2 pos)
    {
        transform.localPosition = pos + ArcNote.Height * Vector2.up;
    }
}
