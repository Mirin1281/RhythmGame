using UnityEngine;

public class MaterialSetter : MonoBehaviour
{
    void Awake()
    {
        var meshRenderer = GetComponent<MeshRenderer>();
        meshRenderer.sharedMaterial = new Material(meshRenderer.sharedMaterial);
    }
}
