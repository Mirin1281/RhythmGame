using Cysharp.Threading.Tasks;
using UnityEditor;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class AspectSetter : MonoBehaviour
{
    void Awake()
    {
        SetAspect();   
    }

#if UNITY_EDITOR
    [ContextMenu("SetAspect")]
    async UniTask SetAspectAsync()
    {
        await UniTask.Delay(1000);
        SetAspect();
        EditorUtility.SetDirty(this);
    }
#endif

    void SetAspect()
    {
        var _camera = GetComponent<Camera>();

        var scrnAspect = Screen.width / (float)Screen.height; // 現在のアスペクト比
        var targAspect = ConstContainer.ScreenSize.x / ConstContainer.ScreenSize.y; // 目標のアスペクト比

        var rate = targAspect / scrnAspect;
        var rect = new Rect(0, 0, 1, 1);

        // 倍率が小さい場合、横をそろえる
        if (rate < 1)
        {
            rect.width = rate;
            rect.x = 0.5f - rect.width * 0.5f;
        }
        else // 縦をそろえる
        {
            rect.height = 1 / rate;
            rect.y = 0.5f - rect.height * 0.5f;
        }

        _camera.rect = rect;
    }
}