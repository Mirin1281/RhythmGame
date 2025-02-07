using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraScaler : MonoBehaviour
{
    public Vector3 ScreenScale = Vector3.one;
    Camera _camera;

    void Start()
    {
        _camera = GetComponent<Camera>();
    }

    void OnPreCull()
    {
        _camera.ResetWorldToCameraMatrix();
        _camera.ResetProjectionMatrix();
        _camera.projectionMatrix *= Matrix4x4.Scale(ScreenScale);
    }

    void OnPreRender()
    {
        if (ScreenScale.x * ScreenScale.y * ScreenScale.z < 0)
        {
            GL.invertCulling = true;
        }
    }

    void OnPostRender()
    {
        GL.invertCulling = false;
    }
}