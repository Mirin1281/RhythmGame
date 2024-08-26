using UnityEngine;
using UnityEngine.InputSystem.EnhancedTouch;
using System;
using System.Collections.Generic;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;

public class InputManager : MonoBehaviour
{
    Camera mainCamera;
    
    public readonly struct Input : IEquatable<Input>
    {
        public readonly int index;
        public readonly Vector2 pos;
        public Input(int index, Vector2 pos)
        {
            this.index = index;
            this.pos = pos;
        }

        public bool Equals(Input input) => (index, pos) == (input.index, input.pos);
        public override int GetHashCode() => (index, pos).GetHashCode();
    }
    readonly List<Input> inputs = new(10);

    // 座標はスクリーン座標であることに注意
    readonly struct FlickInput : IEquatable<FlickInput>
    {
        public readonly int index;
        public readonly Vector2 startPos;
        public FlickInput(Finger finger)
        {
            index = finger.index;
            startPos = finger.screenPosition;
        }

        public bool Equals(FlickInput f) => (index, startPos) == (f.index, f.startPos);
        public override int GetHashCode() => (index, startPos).GetHashCode();
    }
    readonly List<FlickInput> flickInputs = new(4);

    const float posDiff = 30f; // フリックの感度

    public event Action<Vector2> OnInput;
    public event Action<List<Input>> OnHold;
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
        var pos = GetWorldPosition(finger.screenPosition);
        OnInput?.Invoke(pos);
        var flickInput = new FlickInput(finger);
        flickInputs.Add(flickInput);
    }
    void OnFingerMove(Finger finger)
    {
        /*var f = flickInputs.Where(f => f.index == finger.index).FirstOrDefault();
        if(f.Equals(default)) return;
        if (Mathf.Abs(finger.screenPosition.y - f.startPos.y) >= posDiff
         || Mathf.Abs(finger.screenPosition.x - f.startPos.x) >= posDiff)
        {
            OnFlick?.Invoke(GetWorldPosition(f.startPos));
            flickInputs.Remove(f);
        }*/
        for(int i = 0; i < flickInputs.Count; i++)
        {
            var input = flickInputs[i];
            if(input.index != finger.index) continue;
            if (Mathf.Abs(finger.screenPosition.y - input.startPos.y) >= posDiff
             || Mathf.Abs(finger.screenPosition.x - input.startPos.x) >= posDiff)
            {
                OnFlick?.Invoke(GetWorldPosition(input.startPos));
                flickInputs.RemoveAt(i);
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

        for(int i = 0; i < flickInputs.Count; i++)
        {
            if(finger.index == flickInputs[i].index)
            {
                flickInputs.RemoveAt(i);
            }
        }
    }

    void Update()
    {
        inputs.Clear();
        foreach(var touch in Touch.activeTouches)
        {
            var input = new Input(touch.finger.index, GetWorldPosition(touch.screenPosition));
            inputs.Add(input);
        }
        OnHold?.Invoke(inputs);
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
