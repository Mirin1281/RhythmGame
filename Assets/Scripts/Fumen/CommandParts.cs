using System;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace NoteCreating
{
    public interface ICommand
    {
        void Execute(NoteCreateHelper helper, float delta);
#if UNITY_EDITOR
        string GetName(bool rawName = false);
        string GetSummary();
        Color GetColor();
#endif
    }

    /// <summary>
    /// このインターフェースを持たせると、途中再生時でもスキップされずにコマンドが呼び出されます
    /// </summary>
    public interface INotSkipCommand { }

    // コマンドにフィールドとして持たせて使います
    [Serializable]
    public struct Mirror : IEquatable<Mirror>
    {
        [SerializeField] bool enabled;

        /// <summary>
        /// enabledがtrueの時、-1倍して返します
        /// </summary>s
        public readonly float Conv(float x) => x * (enabled ? -1 : 1);
        public readonly int Conv(int x) => x * (enabled ? -1 : 1);
        public readonly Vector3 Conv(Vector3 pos) => new Vector3(Conv(pos.x), pos.y, pos.z);
        public readonly Vector2 Conv(Vector2 pos) => new Vector2(Conv(pos.x), pos.y);

        /// <summary>
        /// enabledがtrueの際にテキストを追加します
        /// </summary>
        public readonly string GetStatusText()
        {
            if (enabled)
            {
                return "  <color=#0000ff><b>(mir)</b></color>";
            }
            else
            {
                return string.Empty;
            }
        }

        public readonly bool Equals(Mirror other)
        {
            return enabled == other.enabled;
        }

        public override readonly bool Equals(object obj)
        {
            if (obj is Mirror mirror)
            {
                return Equals(mirror);
            }
            else
            {
                return false;
            }
        }

        public override readonly int GetHashCode()
        {
            return enabled.GetHashCode();
        }
    }


    /// <summary>
    /// 音楽を基準にした時間を制御するための構造体 (シングルトンのMetronomeを使用)
    /// </summary>
    [Serializable]
    public struct Lpb : IEquatable<Lpb>, IComparable<Lpb>
    {
        [SerializeField] float _value;

        public Lpb(float lpb, int num = 1)
        {
            if (num == 0)
            {
                _value = 0;
            }
            else
            {
                _value = lpb / num;
            }
        }

        public readonly float Value => _value;

        public readonly float Time
        {
            get
            {
                if (_value == 0) return 0;
#if UNITY_EDITOR
                if (EditorApplication.isPlaying == false)
                {
                    return 240f / CommandEditorUtility.DebugBPM / _value;
                }
#endif
                return 240f / Metronome.Instance.Bpm / _value;
            }
        }

        public static Lpb GetFrom(float time, int num = 1)
        {
            if (time == 0)
            {
                return new Lpb(0);
            }
#if UNITY_EDITOR
            if (EditorApplication.isPlaying == false)
            {
                return new Lpb(240f / CommandEditorUtility.DebugBPM / time * num);
            }
#endif
            return new Lpb(240f / Metronome.Instance.Bpm / time * num);
        }

        #region Overrides & Interfaces

        public readonly bool Equals(Lpb other)
        {
            return _value == other._value;
        }

        public override readonly bool Equals(object obj)
        {
            if (obj is Lpb lpb)
            {
                return Equals(lpb);
            }
            else
            {
                return false;
            }
        }

        public readonly int CompareTo(Lpb other)
        {
            return _value.CompareTo(other._value);
        }

        public override readonly string ToString()
        {
            return $"LPB: {_value},  Time: {Time}";
        }

        public override readonly int GetHashCode()
        {
            return _value.GetHashCode();
        }

        #endregion

        #region Operators

        public static Lpb operator +(Lpb left, Lpb right) => new Lpb(AddLpb(left._value, right._value));
        public static Lpb operator -(Lpb left, Lpb right) => new Lpb(SubtractLpb(left._value, right._value));
        public static Lpb operator *(Lpb left, float right)
        {
            if (right == 0)
            {
                return new Lpb(0);
            }
            return new Lpb(left._value / right);
        }
        public static Lpb operator *(float left, Lpb right)
        {
            return right * left;
        }
        public static Lpb operator /(Lpb left, float right)
        {
            if (right == 0)
            {
                Debug.LogWarning($"{nameof(Lpb)}が0除算されました");
                return new Lpb(0);
            }
            return new Lpb(left._value * right);
        }

        public static float operator /(Lpb left, Lpb right)
        {
            if (left._value == 0) return 0;
            if (right._value == 0)
            {
                Debug.LogWarning($"{nameof(Lpb)}が0除算されました");
                return 0;
            }
            return right._value / left._value;
        }

        public static bool operator ==(Lpb left, Lpb right) => left._value == right._value;
        public static bool operator !=(Lpb left, Lpb right) => left._value != right._value;
        public static bool operator >(Lpb left, Lpb right) => left._value < right._value;
        public static bool operator <(Lpb left, Lpb right) => left._value > right._value;
        public static bool operator >=(Lpb left, Lpb right) => left._value <= right._value;
        public static bool operator <=(Lpb left, Lpb right) => left._value >= right._value;

        static float AddLpb(float a, float b)
        {
            if (a == 0)
            {
                return b;
            }
            else if (b == 0)
            {
                return a;
            }

            return a * b / (a + b);
        }

        static float SubtractLpb(float a, float b)
        {
            if (a == 0)
            {
                return -b;
            }
            else if (b == 0)
            {
                return a;
            }

            if (a == b) return 0;
            return a * b / (b - a);
        }

        #endregion
    }
}