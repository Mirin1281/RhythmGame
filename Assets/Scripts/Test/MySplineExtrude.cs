using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Splines;
using UnityEditor;
using UnityEngine.SceneManagement;
using Cysharp.Threading.Tasks;

public class MySplineExtrude : MonoBehaviour
{
    [SerializeField] SplineContainer m_Container;

    [SerializeField] int m_RebuildFrequency = 30;

    [SerializeField] int m_Sides = 8;

    [SerializeField] float m_SegmentsPerUnit = 4f;

    [SerializeField] bool m_Capped = true;

    [SerializeField] float m_Radius = 0.25f;

    [SerializeField] Vector2 m_Range = new Vector2(0f, 1f);

    Mesh m_Mesh;
    static readonly string k_EmptyContainerError = "Spline Extrude does not have a valid SplineContainer set.";
    static readonly string k_EmptyMeshFilterError = "SplineExtrude.createMeshInstance is disabled, but there is no valid mesh assigned. Please create or assign a writable mesh asset.";


    public Spline Spline => m_Container?.Spline;

    public IReadOnlyList<Spline> Splines => m_Container?.Splines;

    void Reset()
    {
        if (TryGetComponent<MeshFilter>(out var component))
        {
            component.sharedMesh = (m_Mesh = CreateMeshAsset());
        }

        if (TryGetComponent<MeshRenderer>(out var component2) && component2.sharedMaterial == null)
        {
            GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
            Material sharedMaterial = obj.GetComponent<MeshRenderer>().sharedMaterial;
            UnityEngine.Object.DestroyImmediate(obj);
            component2.sharedMaterial = sharedMaterial;
        }

        RebuildAsync().Forget();
    }

    bool IsNullOrEmptyContainer()
    {
        int num;
        if (!(m_Container == null) && m_Container.Spline != null)
        {
            num = ((m_Container.Splines.Count == 0) ? 1 : 0);
            if (num == 0)
            {
                goto IL_0046;
            }
        }
        else
        {
            num = 1;
        }

        if (Application.isPlaying)
        {
            Debug.LogError(k_EmptyContainerError, this);
        }

        goto IL_0046;
    IL_0046:
        return (byte)num != 0;
    }

    bool IsNullOrEmptyMeshFilter()
    {
        bool num = (m_Mesh = GetComponent<MeshFilter>().sharedMesh) == null;
        if (num)
        {
            Debug.LogError(k_EmptyMeshFilterError, this);
        }
        return num;
    }

    public async UniTask RebuildAsync()
    {
        if (IsNullOrEmptyContainer() || IsNullOrEmptyMeshFilter()) return;
        await MySplineMesh.ExtrudeAsync(m_Container.Splines, m_Mesh, m_Radius, m_Sides, m_SegmentsPerUnit, m_Capped, m_Range, destroyCancellationToken);
    }

    Mesh CreateMeshAsset()
    {
        Mesh mesh = new Mesh
        {
            name = base.name
        };
        Scene activeScene = SceneManager.GetActiveScene();
        string text = "Assets";
        if (!string.IsNullOrEmpty(activeScene.path))
        {
            text = Path.GetDirectoryName(activeScene.path) + "/" + Path.GetFileNameWithoutExtension(activeScene.path);
            if (!Directory.Exists(text))
            {
                Directory.CreateDirectory(text);
            }
        }

        string path = AssetDatabase.GenerateUniqueAssetPath(text + "/SplineExtrude_" + mesh.name + ".asset");
        AssetDatabase.CreateAsset(mesh, path);
        EditorGUIUtility.PingObject(mesh);
        return mesh;
    }
}