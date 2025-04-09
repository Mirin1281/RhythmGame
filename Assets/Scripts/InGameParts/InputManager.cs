using UnityEngine;
using UnityEngine.InputSystem.EnhancedTouch;
using System;
using System.Collections.Generic;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;
using Cysharp.Threading.Tasks;

public class InputManager : MonoBehaviour
{
    [SerializeField] Camera mainCamera;
    [SerializeField] bool enableInput = true;
    public bool EnableInput
    {
        get => enableInput;
        set => enableInput = value;
    }

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

    public event Action<Input> OnDown;
    public event Action<List<Input>> OnHold;
    public event Func<Input, UniTaskVoid> OnUp;

    void Awake()
    {
        EnhancedTouchSupport.Enable();
        Touch.onFingerDown += OnFingerDown;
        Touch.onFingerUp += OnFingerUp;
    }
    void OnDestroy()
    {
        OnDown = null;
        OnHold = null;
        OnUp = null;
        Touch.onFingerDown -= OnFingerDown;
        Touch.onFingerUp -= OnFingerUp;
        EnhancedTouchSupport.Disable();
    }

    void OnFingerDown(Finger finger)
    {
        //if (finger.screenPosition.x > 10000 || finger.screenPosition.x < -10000) return;
        if (enableInput == false) return;
        OnDown?.Invoke(GetInput(finger));
    }
    void OnFingerUp(Finger finger)
    {
        //if (finger.screenPosition.x > 10000 || finger.screenPosition.x < -10000) return;
        if (enableInput == false) return;
        OnUp?.Invoke(GetInput(finger));
    }

    void Update()
    {
        inputs.Clear();
        if (enableInput == false) return;
        foreach (var touch in Touch.activeTouches)
        {
            //if (touch.screenPosition.x > 10000 || touch.screenPosition.x < -10000) continue;
            inputs.Add(GetInput(touch.finger));
        }
        OnHold?.Invoke(inputs);
    }

    Input GetInput(Finger finger) => new Input(finger.index, GetWorldPosition(finger.screenPosition));

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
