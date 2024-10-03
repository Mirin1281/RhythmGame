using System;
using Cysharp.Threading.Tasks;
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
    [SerializeField] int wait;
    [SerializeField] bool isPosMove = false;
    [SerializeField] Vector3 pos;
    [SerializeField] bool isRotateMove = true;
    [SerializeField] Vector3 rotate;
    [SerializeField, Tooltip("現在の回転から目標の回転までの差が\nより近いような回り方を選んで回転します")]
    bool isRotateClamp = true;
    [SerializeField] float time = 1f;
    [SerializeField] EaseType easeType = EaseType.OutQuad;
    [SerializeField] CameraMoveType moveType = CameraMoveType.Absolute;

    public int Wait => wait;
    public bool IsPosMove => isPosMove;
    public Vector3 Pos => pos;
    public bool IsRotateMove => isRotateMove;
    public Vector3 Rotate => rotate;
    public bool IsRotateClamp => isRotateClamp;
    public float Time => time;
    public EaseType EaseType => easeType;
    public CameraMoveType MoveType => moveType;

    public CameraMoveSetting(bool isPosMove, Vector3 pos, bool isRotateMove, Vector3 rotate,
        bool isRotateClamp, float time, EaseType easeType, CameraMoveType moveType, int wait = 0)
    {
        this.wait = wait;
        this.isPosMove = isPosMove;
        this.pos = pos;
        this.isRotateMove = isRotateMove;
        this.rotate = rotate;
        this.isRotateClamp = isRotateClamp;
        this.time = time;
        this.easeType = easeType;
        this.moveType = moveType;
    }

    public CameraMoveSetting() {} // デフォルトコンストラクタ
}

public class CameraMover : MonoBehaviour
{
    [SerializeField] Metronome metronome;
    Vector3 deltaPos;
    Quaternion deltaRot;
    Vector3 basePos;
    Quaternion baseRot;

    [ContextMenu("Switch")]
    void SwitchDimension()
    {
        if(transform.localPosition.y == 4f)
        {
            transform.localPosition = new Vector3(0f, 7f, -6.5f);
            transform.localRotation = Quaternion.Euler(25f, 0f, 0f);
        }
        else if(transform.localPosition.y == 7f)
        {
            transform.localPosition = new Vector3(0f, 4f, -10f);
            transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
        }
    }

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

    public void SetPos(Vector3 pos)
    {
        basePos = pos;
    }

    public void SetRot(Quaternion rot)
    {
        baseRot = rot;
    }

    public void Move(Vector3? nullablePos, Vector3? nullableRot, CameraMoveType moveType,
        float time, EaseType easeType, bool isRotateClamp = true, bool isInverse = false)
    {
        CameraMoveSetting m = new(
            nullablePos.HasValue, nullablePos.GetValueOrDefault(),
            nullableRot.HasValue, nullableRot.GetValueOrDefault(),
            isRotateClamp,
            time,
            easeType,
            moveType);
        Move(m, isInverse);
    }
    public void Move(CameraMoveSetting m, bool isInverse = false)
    {
        if(m.MoveType == CameraMoveType.Absolute)
        {
            MoveTo(m, isInverse);
        }
        else if (m.MoveType == CameraMoveType.Relative)
        {
            MoveRelative(m, isInverse).Forget();
        }
    }

    /// <summary>
    /// 相対移動させます。複数登録すると動きが加算されます
    /// </summary>
    async UniTask MoveRelative(CameraMoveSetting m, bool isInverse = false)
    {
        int a = isInverse ? -1 : 1;
        await WhileYieldAsync(m.Time, t => 
        {
            if(m.IsPosMove)
            {
                deltaPos += new Vector3(
                    a * t.Ease(0, m.Pos.x, m.Time, m.EaseType),
                    t.Ease(0, m.Pos.y, m.Time, m.EaseType),
                    t.Ease(0, m.Pos.z, m.Time, m.EaseType)
                );
            }
            if(m.IsRotateMove)
            {
                deltaRot = Quaternion.Euler(
                    t.Ease(0, m.Rotate.x, m.Time, m.EaseType),
                    a * t.Ease(0, m.Rotate.y, m.Time, m.EaseType),
                    a * t.Ease(0, m.Rotate.z, m.Time, m.EaseType));
            }
        });
        await UniTask.Yield(PlayerLoopTiming.LastEarlyUpdate, destroyCancellationToken);
        if(m.IsPosMove)
            basePos += new Vector3(a * m.Pos.x, m.Pos.y, m.Pos.z);
        if(m.IsRotateMove)
            baseRot *= Quaternion.Euler(new Vector3(m.Rotate.x, a * m.Rotate.y, a * m.Rotate.z));
    }

    static float GetNormalizedAngle(float angle, float min = -180, float max = 180)
    {
        return Mathf.Repeat(angle - min, max - min) + min;
    }

    void MoveTo(CameraMoveSetting m, bool isInverse = false)
    {
        if(m.IsPosMove == false && m.IsRotateMove == false) return;
        int a = isInverse ? -1 : 1;
        var startPos = transform.position;
        var startRot = transform.eulerAngles;

        if(m.IsPosMove)
        {
            WhileYield(m.Time, t => 
            {
                basePos = new Vector3(
                    a * t.Ease(startPos.x, m.Pos.x, m.Time, m.EaseType),
                    t.Ease(startPos.y, m.Pos.y, m.Time, m.EaseType),
                    t.Ease(startPos.z, m.Pos.z, m.Time, m.EaseType)
                );
            });
        }
        if(m.IsRotateMove)
        {
            if(m.IsRotateClamp)
            {
                WhileYield(m.Time, t => 
                {
                    baseRot = Quaternion.Euler(
                        t.Ease(startRot.x, GetNormalizedAngle(m.Rotate.x, startRot.x - 180, startRot.x + 180), m.Time, m.EaseType),
                        a * t.Ease(startRot.y, GetNormalizedAngle(m.Rotate.y, startRot.y - 180, startRot.y + 180), m.Time, m.EaseType),
                        a * t.Ease(startRot.z, GetNormalizedAngle(m.Rotate.z, startRot.z - 180, startRot.z + 180), m.Time, m.EaseType));
                });
            }
            else
            {
                WhileYield(m.Time, t => 
                {
                    baseRot = Quaternion.Euler(
                        t.Ease(startRot.x, m.Rotate.x, m.Time, m.EaseType),
                        a * t.Ease(startRot.y, m.Rotate.y, m.Time, m.EaseType),
                        a * t.Ease(startRot.z, m.Rotate.z, m.Time, m.EaseType));
                });
            }
        }
    }

    public void Shake(float strength, float time, EaseType easeType = EaseType.OutBack, bool isInverse = false)
    {
        int a = isInverse ? -1 : 1;
        baseRot = Quaternion.Euler(0f, 0f, a * strength);
        WhileYield(time, t => 
        {
            baseRot = Quaternion.Euler(0f, 0f, a * t.Ease(strength, 0, time, easeType));
        });
    }

    void WhileYield(float time, Action<float> action) => WhileYieldAsync(time, action).Forget();
    async UniTask WhileYieldAsync(float time, Action<float> action)
    {
        float baseTime = metronome.CurrentTime;
        float t = 0f;
        while(t < time)
        {
            t = metronome.CurrentTime - baseTime;
            action.Invoke(t);
            await UniTask.Yield(PlayerLoopTiming.PreLateUpdate, destroyCancellationToken);
        }
        action.Invoke(time);
    }
}
