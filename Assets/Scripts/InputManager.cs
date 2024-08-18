using UnityEngine;
using UnityEngine.InputSystem.EnhancedTouch;
using System;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;
using System.Collections.Generic;

public class InputManager : MonoBehaviour
{
    Camera mainCamera;
    
    class FlickParameter
    {
        public int index;
        public Vector2 startPos;
        public Vector2 currentPos;
        public bool isPrepared;
    }
    readonly List<FlickParameter> flickParameters = new(4);
    
    const float posDiff = 30f; // フリックの感度

    public event Action<Vector2> OnInput;
    public event Action<Vector2[]> OnHold;
    public event Action<Vector2> OnFlick;
    public event Action<int> OnUp;

    void Start()
    {
        mainCamera = Camera.main;
        EnhancedTouchSupport.Enable();
        Touch.onFingerDown += OnFingerDown;
        Touch.onFingerMove += OnFingerMove;
        Touch.onFingerUp += OnFingerUp;
    }
    void OnDestroy()
    {
        Touch.onFingerDown -= OnFingerDown;
        Touch.onFingerMove -= OnFingerMove;
        Touch.onFingerUp -= OnFingerUp;
        EnhancedTouchSupport.Disable();
    }

    void OnFingerDown(Finger finger)
    {
        foreach (Vector2 pos in GetWorldPositions())
        {
            OnInput?.Invoke(pos);
        }

        var flickParam = new FlickParameter()
        {
            index = finger.index,
            startPos = finger.screenPosition,
            isPrepared = true,
        };
        flickParameters.Add(flickParam);
    }
    void OnFingerMove(Finger finger)
    {
        for(int i = 0; i < flickParameters.Count; i++)
        {
            var p = flickParameters[i];
            if(p.index != finger.index) continue;
            p.currentPos = finger.screenPosition;
            if (p.isPrepared && 
               (Mathf.Abs(p.currentPos.y - p.startPos.y) >= posDiff || Mathf.Abs(p.currentPos.x - p.startPos.x) >= posDiff))
            {
                OnFlick?.Invoke(GetWorldPosition(p.startPos));
                p.isPrepared = false;
                p.startPos = p.currentPos;
                //flickParameters.Remove(p);
            }
        }


        /*async UniTask PulseFlash()
        {
            mainCamera.backgroundColor = new Color(0.5f, 0.5f, 0.5f);
            await MyStatic.WaitSeconds(0.1f);
            mainCamera.backgroundColor = Color.black;
        }*/
    }
    void OnFingerUp(Finger finger)
    {
        for(int i = 0; i < flickParameters.Count; i++)
        {
            if(flickParameters[i].index != finger.index) continue;
            OnUp?.Invoke(flickParameters[i].index);
            flickParameters.RemoveAt(i);
        }
    }

    void Update()
    {
        OnHold?.Invoke(GetWorldPositions());
    }

    public Vector2[] GetWorldPositions()
    {
        var touches = Touch.activeTouches;
        Vector2[] poses = new Vector2[touches.Count];
        for(int i = 0; i < touches.Count; i++)
        {
            poses[i] = GetWorldPosition(touches[i].screenPosition);
        }
        return poses;
    }

    public (Vector2 pos, int fingerIndex)[] GetWorldPositionAndIndices()
    {
        var touches = Touch.activeTouches;
        (Vector2, int)[] poses = new (Vector2, int)[touches.Count];
        for(int i = 0; i < touches.Count; i++)
        {
            poses[i] = (GetWorldPosition(touches[i].screenPosition), touches[i].finger.index);
        }
        return poses;
    }

    Vector2 GetWorldPosition(Vector2 screenPos)
    {
        Ray ray = mainCamera.ScreenPointToRay(screenPos);
        if (ray.direction.z != 0)
        {
            float t = -ray.origin.z / ray.direction.z; // z=0の平面との交点を求める
            return ray.origin + ray.direction * t;
        }
        return Vector2.zero;
    }
}
