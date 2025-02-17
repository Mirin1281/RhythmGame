using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

// UnityではないC#の場合はMathf.Pow()をMath.Pow()などに置き換えてください
//
// 【利用方法】
// 1. float.Ease(float start, float from, float easeTime, EaseType type) // 拡張メソッド
// 2. var easing = new Easing(float start, float from, float easeTime, EaseType type) // 構造体をつくって
//    easing.Ease(float time) //使う

static class ValueEaseExtension
{
    public static float Ease(this int self, float start, float from, float easeTime, EaseType type)
    {
        return Easing.Ease(start, from, easeTime, type, self);
    }
    public static float Ease(this float self, float start, float from, float easeTime, EaseType type)
    {
        return Easing.Ease(start, from, easeTime, type, self);
    }
}

public readonly struct Easing
{
    readonly float start;
    readonly float delta;
    readonly float inversedEaseTime; // 逆数にすることでインスタンスメソッドを使用した際に除算が乗算に変わってお得
    readonly EaseType type;

    static readonly EaseType DefaultEaseType = EaseType.OutQuad;

    public Easing(float start, float from, float easeTime, EaseType type)
    {
        this.start = start;
        this.delta = from - start;
        this.inversedEaseTime = 1f / easeTime;

        if (easeTime == 0)
        {
            this.type = EaseType.End;
        }
        else if (type == EaseType.Default)
        {
            this.type = DefaultEaseType;
        }
        else
        {
            this.type = type;
        }
    }

    public static float Ease(float start, float from, float easeTime, EaseType type, float time)
    {
        if (easeTime == 0)
        {
            type = EaseType.End;
        }
        else if (type == EaseType.Default)
        {
            type = DefaultEaseType;
        }
        float t = time / easeTime;
        return start + (from - start) * GetPlaneValue(type, t);
    }

    public float Ease(float time)
    {
        float t = time * inversedEaseTime;
        return start + delta * GetPlaneValue(type, t);
    }

    public async UniTask EaseAsync(CancellationToken token, float delta, Action<float> action)
    {
        float baseTime = Metronome.Instance.CurrentTime - delta;
        float time = 0f;
        while (time < 1f / inversedEaseTime)
        {
            time = Metronome.Instance.CurrentTime - baseTime;
            float t = time * inversedEaseTime;
            action.Invoke(start + delta * GetPlaneValue(type, t));
            await UniTask.Yield(token);
        }
        action.Invoke(start + delta * GetPlaneValue(type, 1));
    }

    static float GetPlaneValue(EaseType type, float t)
    {
        return type switch
        {
            EaseType.Linear => t,

            EaseType.InQuad => Pow(t, 2),
            EaseType.InCubic => Pow(t, 3),
            EaseType.InQuart => Pow(t, 4),

            EaseType.OutQuad => 1 - Pow(1 - t, 2),
            EaseType.OutCubic => 1 - Pow(1 - t, 3),
            EaseType.OutQuart => 1 - Pow(1 - t, 4),

            EaseType.InOutQuad =>
                t < 0.5f
                    ? 2 * Pow(t, 2)
                    : 1 - Pow(-2 * t + 2, 2) / 2f,
            EaseType.InOutCubic =>
                t < 0.5f
                    ? 4 * Pow(t, 3)
                    : 1 - Pow(-2 * t + 2, 3) / 2f,
            EaseType.InOutQuart =>
                t < 0.5f
                    ? 8 * Pow(t, 4)
                    : 1 - Pow(-2 * t + 2, 4) / 2f,

            EaseType.InSine => 1 - Mathf.Cos(t * Mathf.PI / 2f),
            EaseType.OutSine => Mathf.Sin(t * Mathf.PI / 2f),
            EaseType.InOutSine => -(Mathf.Cos(t * Mathf.PI) - 1) / 2f,

            EaseType.InCirc => 1 - Mathf.Sqrt(1 - Pow(t, 2)),
            EaseType.OutCirc => Mathf.Sqrt(1 - Pow(t - 1, 2)),
            EaseType.InOutCirc =>
                t < 0.5f
                    ? (1 - Mathf.Sqrt(1 - 4 * t)) / 2f
                    : (Mathf.Sqrt(1 - Pow(-2 * t + 2, 2)) + 1) / 2f,

            EaseType.InBack => (GetOption(type) + 1) * Pow(t, 3) - GetOption(type) * Pow(t, 2),
            EaseType.OutBack => 1 + (GetOption(type) + 1) * Pow(t - 1, 3) + GetOption(type) * Pow(t - 1, 2),
            EaseType.InOutBack =>
                t < 0.5f
                    ? Pow(2 * t, 2) * ((GetOption(type) + 1) * 2 * t - GetOption(type)) / 2f
                    : Pow(2 * t - 2, 2) * ((GetOption(type) + 1) * (t * 2 - 2) + GetOption(type)) + 2 / 2f,

            EaseType.Start => 0f,
            EaseType.End => 1f,

            _ => throw new ArgumentException(),
        };


        static float Pow(float value, int p)
        {
            float result = 1;
            for (int i = 0; i < p; i++)
            {
                result *= value;
            }
            return result;
        }

        static float GetOption(EaseType type) => type switch
        {
            EaseType.InBack or EaseType.OutBack => 1.70158f,
            EaseType.InOutBack => 1.70158f * 1.525f,
            _ => throw new ArgumentOutOfRangeException()
        };
    }
}

public enum EaseType
{
    [InspectorName("- " + nameof(Default))] Default,
    [InspectorName(nameof(Linear))] Linear,

    [InspectorName("In Quad")] InQuad,
    [InspectorName("Out Quad")] OutQuad,
    [InspectorName("InOut Quad")] InOutQuad,
    [InspectorName("In Cubic")] InCubic,
    [InspectorName("Out Cubic")] OutCubic,
    [InspectorName("InOut Cubic")] InOutCubic,
    [InspectorName("In Quart")] InQuart,
    [InspectorName("Out Quart")] OutQuart,
    [InspectorName("InOut Quart")] InOutQuart,

    [InspectorName("In Sine")] InSine,
    [InspectorName("Out Sine")] OutSine,
    [InspectorName("InOut Sine")] InOutSine,
    [InspectorName("In Circ")] InCirc,
    [InspectorName("Out Circ")] OutCirc,
    [InspectorName("InOut Circ")] InOutCirc,

    [InspectorName("In Back")] InBack,
    [InspectorName("Out Back")] OutBack,
    [InspectorName("InOut Back")] InOutBack,

    [InspectorName("- " + nameof(Start))] Start,
    [InspectorName("- " + nameof(End))] End,
}

public readonly struct EasingVector2
{
    readonly Easing easingX;
    readonly Easing easingY;

    public EasingVector2(Vector2 start, Vector2 from, float easeTime, EaseType type)
    {
        easingX = new Easing(start.x, from.x, easeTime, type);
        easingY = new Easing(start.y, from.y, easeTime, type);
    }

    public static Vector2 Ease(Vector2 start, Vector2 from, float easeTime, EaseType type, float time)
    {
        return new Vector2(
            Easing.Ease(start.x, from.x, easeTime, type, time),
            Easing.Ease(start.y, from.y, easeTime, type, time));
    }

    public Vector2 Ease(float time)
    {
        return new Vector2(easingX.Ease(time), easingY.Ease(time));
    }
}