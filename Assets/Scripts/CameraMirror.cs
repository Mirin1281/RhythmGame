using UnityEngine;
using UnityEngine.Rendering;

[RequireComponent(typeof(Camera))]
public class CameraMirror : MonoBehaviour
{
    public bool IsInvert = true;
    Camera _camera;

    void Awake()
    {
        _camera = GetComponent<Camera>();
    }

    void OnEnable()
    {
        RenderPipelineManager.beginCameraRendering += OnBeginCameraRendering;
        RenderPipelineManager.endCameraRendering += OnEndCameraRendering;
    }

    void OnDisable()
    {
        RenderPipelineManager.beginCameraRendering -= OnBeginCameraRendering;
        RenderPipelineManager.endCameraRendering -= OnEndCameraRendering;
    }

    void OnBeginCameraRendering(ScriptableRenderContext context, Camera camera)
    {
        if (IsInvert)
        {
            GL.invertCulling = true;
        }

        _camera.ResetWorldToCameraMatrix();
        _camera.ResetProjectionMatrix();

        var vec = IsInvert ? new Vector3(-1f, 1f, 1f) : Vector3.one;
        _camera.projectionMatrix *= Matrix4x4.Scale(vec);
    }

    void OnEndCameraRendering(ScriptableRenderContext context, Camera camera)
    {
        GL.invertCulling = false;
    }
}