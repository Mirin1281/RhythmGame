using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugSphere : MonoBehaviour
{
    public void SetColor(Color color)
    {
        var renderer = GetComponent<MeshRenderer>();
        var material = renderer.sharedMaterial;
        var newMat = new Material(material);
        newMat.color = color;
        renderer.material = newMat;
        
    }
}
