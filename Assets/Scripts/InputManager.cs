using UnityEngine;
using UnityEngine.InputSystem.EnhancedTouch;
using System;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;
using System.Collections.Generic;

public class InputManager : MonoBehaviour
{
    Camera mainCamera;
    [SerializeField] LayerMask layer;

    public event Action<Vector2> OnInput;
    public event Action<Vector2[]> OnHold;
    public event Action<Vector2> OnFlick;
    
    // フリックの感度
    const float posDiff = 30f;
    class FlickParameter
    {
        public int index;
        public Vector2 startPos;
        public Vector2 currentPos;
        public bool isPrepared;
    }
    readonly List<FlickParameter> flickParameters = new(4);

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
        foreach (Vector2 pos in GetScreenPositions())
        {
            OnInput?.Invoke(pos);
        }

        var flickParam = new FlickParameter()
        {
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
                Ray ray = mainCamera.ScreenPointToRay(p.startPos);
                if (Physics.Raycast(ray, out var hit, 20f, layer))
                {
                    OnFlick?.Invoke(hit.point);
                }
                p.isPrepared = false;
                flickParameters.Remove(p);
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
            flickParameters.RemoveAt(i);
        }
    }

    void Update()
    {
        OnHold?.Invoke(GetScreenPositions());
    }

    public Vector2[] GetScreenPositions()
    {
        var touches = Touch.activeTouches;
        Vector2[] poses = new Vector2[touches.Count];
        
        for(int i = 0; i < touches.Count; i++)
        {
            var sPos = touches[i].screenPosition;
            Ray ray = mainCamera.ScreenPointToRay(sPos);
            if (Physics.Raycast(ray, out var hit, 20f, layer))
            {
                poses[i] = new Vector2(hit.point.x, hit.point.y);
            }
        }
        return poses;
    }
}
