using UnityEngine;
using UnityEngine.InputSystem.EnhancedTouch;
using System;
using System.Linq;
using System.Collections.Generic;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;
using Cysharp.Threading.Tasks;

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

    // 構造体にしたすぎる
    public class InputStatus
    {
        public readonly int FingerIndex;
        public Vector2 Position { get; private set; }
        public ArcColorType ArcColorType { get; private set; }

        public InputStatus(int index, Vector2 pos)
        {
            FingerIndex = index;
            Position = pos;
            ArcColorType = ArcColorType.None;
        }

        public void SetPosition(Vector2 position) => Position = position;
        public void SetArcColorType(ArcColorType type) => ArcColorType = type;
    }
    List<InputStatus> inputStatuses = new(8);
    public List<InputStatus> InputStatuses => inputStatuses;
    
    const float posDiff = 30f; // フリックの感度

    public event Action<Vector2> OnInput;
    public event Action<List<InputStatus>> OnHold;
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
        OnInput = null;
        OnHold = null;
        OnFlick = null;
        OnUp = null;
        Touch.onFingerDown -= OnFingerDown;
        Touch.onFingerMove -= OnFingerMove;
        Touch.onFingerUp -= OnFingerUp;
        EnhancedTouchSupport.Disable();
    }

    void OnFingerDown(Finger finger)
    {
        inputStatuses = FetchInputStatuses();
        inputStatuses.ForEach(status => OnInput?.Invoke(status.Position));

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
            }
        }


        /*async UniTask PulseFlash()
        {
            mainCamera.backgroundColor = new Color(0.5f, 0.5f, 0.5f);
            await UniTask.Delay(100);
            mainCamera.backgroundColor = Color.black;
        }*/
    }
    void OnFingerUp(Finger finger)
    {
        OnUp?.Invoke(finger.index);
        for(int i = 0; i < flickParameters.Count; i++)
        {
            if(finger.index == flickParameters[i].index)
            {
                flickParameters.RemoveAt(i);
            }
        }
        for(int i = 0; i < inputStatuses.Count; i++)
        {
            if(finger.index == inputStatuses[i].FingerIndex)
            {
                inputStatuses.RemoveAt(i);
            }
        }
    }

    readonly List<Input> inputs = new(10);
    public IReadOnlyList<Input> Inputs => inputs;
    public readonly struct Input
    {
        public readonly int index;
        public readonly Vector2 pos;
        public Input(int index, Vector2 pos)
        {
            this.index = index;
            this.pos = pos;
        }
    }

    void Update()
    {
        {
            var touches = Touch.activeTouches;
            for(int i = 0; i < inputStatuses.Count; i++)
            {
                for(int k = 0; k < touches.Count; k++)
                {
                    if(touches[k].finger.index == inputStatuses[i].FingerIndex)
                    {
                        inputStatuses[i].SetPosition(GetWorldPosition(touches[k].screenPosition));
                        break;
                    }
                }
            }
            /*foreach(var touch in touches)
            {
                Debug.Log(touch.finger.index);
            }*/
            //if(inputStatuses.Count != 0)
            //{
                OnHold?.Invoke(inputStatuses);
            //}
        }

        {
            
            inputs.Clear();
            var touches = Touch.activeTouches;
            foreach(var touch in touches)
            {
                var input = new Input(touch.finger.index, GetWorldPosition(touch.screenPosition));
                inputs.Add(input);
            }
        }
    }

    List<InputStatus> FetchInputStatuses()
        => Touch.activeTouches
        .Select(touch => new InputStatus(touch.finger.index, GetWorldPosition(touch.screenPosition)))
        .ToList();

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
