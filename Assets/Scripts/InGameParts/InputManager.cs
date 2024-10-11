using UnityEngine;
using UnityEngine.InputSystem.EnhancedTouch;
using System;
using System.Collections.Generic;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;

public class InputManager : MonoBehaviour
{
    [SerializeField] Camera mainCamera;
    
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
    class FlickInput
    {
        public readonly int index;
        public Vector2 currentPos;
        public const float PosDiff = 40f; // フリックの感度
        public FlickInput(Finger finger)
        {
            index = finger.index;
            currentPos = finger.screenPosition;
        }
    }
    readonly List<FlickInput> flickInputs = new(4);

    public event Action<Input> OnDown;
    public event Action<List<Input>> OnHold;
    public event Action<Input> OnUp;
    public event Action<Input> OnFlick;

    void Awake()
    {
        EnhancedTouchSupport.Enable();
        Touch.onFingerDown += OnFingerDown;
        Touch.onFingerMove += OnFingerMove;
        Touch.onFingerUp += OnFingerUp;
    }
    void OnDestroy()
    {
        OnDown = null;
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
        if(finger.screenPosition.x > 10000 || finger.screenPosition.x < -10000) return;
        Input input = GetInput(finger);
        OnDown?.Invoke(input);
        flickInputs.Add(new FlickInput(finger));
    }
    void OnFingerMove(Finger finger)
    {
        if(finger.screenPosition.x > 10000 || finger.screenPosition.x < -10000) return;
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
            var flickInput = flickInputs[i];
            if(flickInput.index != finger.index) continue;
            if (Vector2.Distance(finger.screenPosition, flickInput.currentPos) > FlickInput.PosDiff)
            {
                //PulseFlash().Forget();
                OnFlick?.Invoke(GetInput(flickInput));
            }
            flickInput.currentPos = finger.screenPosition;
        }


        /*async UniTask PulseFlash()
        {
            mainCamera.backgroundColor = new Color(0.5f, 0.5f, 0.5f);
            await MyUtility.WaitSeconds(0.1f, destroyCancellationToken);
            mainCamera.backgroundColor = Color.white;
        }*/
    }
    void OnFingerUp(Finger finger)
    {
        if(finger.screenPosition.x > 10000 || finger.screenPosition.x < -10000) return;
        OnUp?.Invoke(GetInput(finger));

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
            if(touch.screenPosition.x > 10000 || touch.screenPosition.x < -10000) continue;
            var input = GetInput(touch.finger);
            inputs.Add(input);
        }
        OnHold?.Invoke(inputs);
    }

    Input GetInput(Finger finger) => new Input(finger.index, GetWorldPosition(finger.screenPosition));
    Input GetInput(FlickInput flickInput) => new Input(flickInput.index, GetWorldPosition(flickInput.currentPos));

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
