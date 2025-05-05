using System;
using Cysharp.Threading.Tasks;
using NoteCreating;
using UnityEngine;

public enum CameraMoveType
{
    /// <summary>
    /// 目的の位置まで移動します
    /// </summary>
    Absolute,

    /// <summary>
    /// 現在の位置から加算します
    /// </summary>
    Relative
}

[Serializable]
public class CameraMoveSetting
{
    [SerializeField] Lpb wait;
    [SerializeField] bool isPosMove = true;
    [SerializeField] Vector3 pos;
    [SerializeField] bool isRotateMove = false;
    [SerializeField] Vector3 rotate;
    [SerializeField, Tooltip("現在の回転から目標の回転までの差が\nより近いような回り方を選んで回転します")]
    bool isRotateClamp = true;
    [SerializeField] EaseType easeType = EaseType.OutQuad;
    [SerializeField] Lpb lpb = new Lpb(4f);
    [SerializeField] CameraMoveType moveType = CameraMoveType.Absolute;

    public Lpb Wait => wait;
    public bool IsPosMove => isPosMove;
    public Vector3 Pos => pos;
    public bool IsRotateMove => isRotateMove;
    public Vector3 Rotate => rotate;
    public bool IsRotateClamp => isRotateClamp;
    public EaseType EaseType => easeType;
    public Lpb Lpb => lpb;
    public CameraMoveType MoveType => moveType;

    public CameraMoveSetting(bool isPosMove, Vector3 pos, bool isRotateMove, Vector3 rotate,
        bool isRotateClamp, Lpb lpb, EaseType easeType, CameraMoveType moveType, Lpb wait = default)
    {
        this.wait = wait;
        this.isPosMove = isPosMove;
        this.pos = pos;
        this.isRotateMove = isRotateMove;
        this.rotate = rotate;
        this.isRotateClamp = isRotateClamp;
        this.lpb = lpb;
        this.easeType = easeType;
        this.moveType = moveType;
    }

    public CameraMoveSetting(bool _)
    {
        this.wait = default;
        this.isPosMove = false;
        this.pos = default;
        this.isRotateMove = false;
        this.rotate = default;
        this.isRotateClamp = true;
        this.lpb = new Lpb(1);
        this.easeType = EaseType.Default;
        this.moveType = CameraMoveType.Relative;
    }
}

public class CameraMover : MonoBehaviour
{
    Vector3 deltaPos;
    Quaternion deltaRot;
    Vector3 basePos;
    Quaternion baseRot;

    void Awake()
    {
        basePos = transform.localPosition;
        baseRot = transform.localRotation;
    }

    void Update()
    {
        deltaPos = Vector3.zero;
        deltaRot = Quaternion.identity;
    }
    void LateUpdate()
    {
        var toPos = basePos + deltaPos;
        var toRot = baseRot * deltaRot;
        transform.SetLocalPositionAndRotation(toPos, toRot);
    }

    /*public void SetPos(Vector3 pos)
    {
        basePos = pos;
    }

    public void SetRot(Quaternion rot)
    {
        baseRot *= rot;
    }*/

    public void Move(Vector3? nullablePos, Vector3? nullableRot, CameraMoveType moveType,
        Lpb lpb, EaseType easeType, bool isRotateClamp = true, float delta = 0, Mirror mir = default)
    {
        CameraMoveSetting m = new(
            nullablePos.HasValue, nullablePos.GetValueOrDefault(),
            nullableRot.HasValue, nullableRot.GetValueOrDefault(),
            isRotateClamp,
            lpb,
            easeType,
            moveType);
        Move(m, delta, mir);
    }
    public void Move(CameraMoveSetting m, float delta, Mirror mir = default)
    {
        if (m.MoveType == CameraMoveType.Absolute)
        {
            MoveTo(m, delta, mir);
        }
        else if (m.MoveType == CameraMoveType.Relative)
        {
            MoveRelative(m, delta, mir).Forget();
        }
    }

    /// <summary>
    /// 相対移動させます。複数登録すると動きが加算されます
    /// </summary>
    async UniTask MoveRelative(CameraMoveSetting m, float delta, Mirror mir = default)
    {
        if (m.IsPosMove == false && m.IsRotateMove == false) return;
        if (m.Lpb == default)
        {
            if (m.IsPosMove)
                basePos += new Vector3(mir.Conv(m.Pos.x), m.Pos.y, m.Pos.z);
            if (m.IsRotateMove)
                baseRot *= Quaternion.Euler(new Vector3(m.Rotate.x, mir.Conv(m.Rotate.y), mir.Conv(m.Rotate.z)));
            return;
        }
        float time = m.Lpb.Time;
        await WhileYieldAsyncRelative(time, delta, t =>
        {
            if (m.IsPosMove)
            {
                deltaPos += new Vector3(
                    mir.Conv(t.Ease(0, m.Pos.x, time, m.EaseType)),
                    t.Ease(0, m.Pos.y, time, m.EaseType),
                    t.Ease(0, m.Pos.z, time, m.EaseType)
                );
            }
            if (m.IsRotateMove)
            {
                deltaRot *= Quaternion.Euler(
                    t.Ease(0, m.Rotate.x, time, m.EaseType),
                    mir.Conv(t.Ease(0, m.Rotate.y, time, m.EaseType)),
                    mir.Conv(t.Ease(0, m.Rotate.z, time, m.EaseType)));
            }
        });
        if (m.IsPosMove)
            basePos += new Vector3(mir.Conv(m.Pos.x), m.Pos.y, m.Pos.z);
        if (m.IsRotateMove)
            baseRot *= Quaternion.Euler(new Vector3(m.Rotate.x, mir.Conv(m.Rotate.y), mir.Conv(m.Rotate.z)));
    }

    /// <summary>
    /// 絶対移動させます
    /// </summary>
    void MoveTo(CameraMoveSetting m, float delta, Mirror mir = default)
    {
        if (m.IsPosMove == false && m.IsRotateMove == false) return;
        var startPos = transform.position;
        var startRot = mir.Conv(transform.eulerAngles);
        if (m.Lpb == default)
        {
            if (m.IsPosMove)
                basePos = new Vector3(mir.Conv(m.Pos.x), m.Pos.y, m.Pos.z);
            if (m.IsRotateMove)
            {
                if (m.IsRotateClamp)
                {
                    baseRot = Quaternion.Euler(new Vector3(
                        GetNormalizedAngle(m.Rotate.x, startRot.x - 180, startRot.x + 180),
                        mir.Conv(GetNormalizedAngle(m.Rotate.y, startRot.y - 180, startRot.y + 180)),
                        mir.Conv(GetNormalizedAngle(m.Rotate.z, startRot.z - 180, startRot.z + 180))));
                }
                else
                {
                    baseRot = Quaternion.Euler(new Vector3(m.Rotate.x, mir.Conv(m.Rotate.y), mir.Conv(m.Rotate.z)));
                }
            }
            return;
        }
        float time = m.Lpb.Time;

        if (m.IsPosMove)
        {
            WhileYieldAsyncAbsolute(time, delta, t =>
            {
                basePos = new Vector3(
                    mir.Conv(t.Ease(startPos.x, m.Pos.x, time, m.EaseType)),
                    t.Ease(startPos.y, m.Pos.y, time, m.EaseType),
                    t.Ease(startPos.z, m.Pos.z, time, m.EaseType)
                );
            }).Forget();
        }
        if (m.IsRotateMove)
        {
            Func<float, Quaternion> rot;
            if (m.IsRotateClamp)
            {
                rot = t => Quaternion.Euler(
                    t.Ease(startRot.x, GetNormalizedAngle(m.Rotate.x, startRot.x - 180, startRot.x + 180), time, m.EaseType),
                    mir.Conv(t.Ease(startRot.y, GetNormalizedAngle(m.Rotate.y, startRot.y - 180, startRot.y + 180), time, m.EaseType)),
                    mir.Conv(t.Ease(startRot.z, GetNormalizedAngle(m.Rotate.z, startRot.z - 180, startRot.z + 180), time, m.EaseType)));
            }
            else
            {
                rot = t => Quaternion.Euler(
                    t.Ease(startRot.x, m.Rotate.x, time, m.EaseType),
                    mir.Conv(t.Ease(startRot.y, m.Rotate.y, time, m.EaseType)),
                    mir.Conv(t.Ease(startRot.z, m.Rotate.z, time, m.EaseType)));
            }
            WhileYieldAsyncAbsolute(time, delta, t => baseRot = rot(t)).Forget();
        }


        static float GetNormalizedAngle(float angle, float min = -180, float max = 180)
        {
            return Mathf.Repeat(angle - min, max - min) + min;
        }
    }

    async UniTask WhileYieldAsyncRelative(float time, float delta, Action<float> action)
    {
        if (time == 0)
        {
            action.Invoke(time);
            return;
        }
        float baseTime = Metronome.Instance.CurrentTime - delta;
        float t = 0f;
        while (t < time)
        {
            t = Metronome.Instance.CurrentTime - baseTime;
            if (t >= time)
            {
                //action.Invoke(time);
                break;
            }
            action.Invoke(t);
            await UniTask.Yield(PlayerLoopTiming.PreLateUpdate, destroyCancellationToken);
        }
    }

    async UniTask WhileYieldAsyncAbsolute(float time, float delta, Action<float> action)
    {
        if (time == 0)
        {
            action.Invoke(time);
            return;
        }
        float baseTime = Metronome.Instance.CurrentTime - delta;
        float t = 0f;
        while (t < time)
        {
            t = Metronome.Instance.CurrentTime - baseTime;
            if (t >= time)
            {
                action.Invoke(time);
                break;
            }
            action.Invoke(t);
            await UniTask.Yield(PlayerLoopTiming.PreLateUpdate, destroyCancellationToken);
        }
    }

    public void MoveRelative(Vector3 pos, Vector3 rot)
    {
        deltaPos += pos;
        deltaRot *= Quaternion.Euler(rot);
    }
}
