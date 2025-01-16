using System.IO;
using UnityEngine;
using UnityEngine.Splines;
using UnityEngine.SceneManagement;
using Cysharp.Threading.Tasks;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class MySplineExtrude : MonoBehaviour
{
    [SerializeField] SplineContainer m_Container;

    [SerializeField] int m_Sides = 8;

    [SerializeField] float m_SegmentsPerUnit = 4f;

    [SerializeField] bool m_Capped = true;

    [SerializeField] float m_Radius = 0.25f;
    public float Radius
    {
        get => m_Radius;
        set => m_Radius = value;
    }

    [SerializeField] Vector2 m_Range = new Vector2(0f, 1f);

    Mesh m_Mesh;
    static readonly string k_EmptyContainerError = "Spline Extrude does not have a valid SplineContainer set.";
    static readonly string k_EmptyMeshFilterError = "SplineExtrude.createMeshInstance is disabled, but there is no valid mesh assigned. Please create or assign a writable mesh asset.";

    public Spline Spline => m_Container.Spline;


    [ContextMenu("Rebuild")]
    void Rebuild()
    {
        RebuildAsync().Forget();
    }

    void Reset()
    {
        if (TryGetComponent<MeshFilter>(out var filter))
        {
            filter.sharedMesh = m_Mesh = CreateMeshAsset();
        }

        if (TryGetComponent<MeshRenderer>(out var renderer) && renderer.sharedMaterial == null)
        {
            GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
            Material mat = obj.GetComponent<MeshRenderer>().sharedMaterial;
            DestroyImmediate(obj);
            renderer.sharedMaterial = mat;
        }

        Rebuild();
    }

    public async UniTask RebuildAsync()
    {
        if (IsNullOrEmptyContainer() || IsNullOrEmptyMeshFilter()) return;
        await MySplineMesh.ExtrudeAsync(
            m_Container.Splines, m_Mesh, m_Radius, m_Sides, m_SegmentsPerUnit, m_Capped, m_Range, destroyCancellationToken);


        bool IsNullOrEmptyContainer()
        {
            var isNull = m_Container == null || m_Container.Spline == null || m_Container.Splines.Count == 0;
            if (isNull)
            {
                if (Application.isPlaying)
                    Debug.LogError(k_EmptyContainerError, this);
            }
            return isNull;
            /*int num;
            if (!(m_Container == null) && m_Container.Spline != null)
            {
                num = (m_Container.Splines.Count == 0) ? 1 : 0;
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
            return (byte)num != 0;*/
        }

        bool IsNullOrEmptyMeshFilter()
        {
            if (m_Mesh == null)
            {
                if (TryGetComponent<MeshFilter>(out var filter))
                {
                    m_Mesh = filter.sharedMesh;
                }
                else
                {
                    Debug.LogError(k_EmptyMeshFilterError, this);
                    return true;
                }
            }
            return false;
        }
    }

    internal Mesh CreateMeshAsset()
    {
        Mesh mesh = new Mesh();
        mesh.name = name;
#if UNITY_EDITOR
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
#endif
        return mesh;

    }
}