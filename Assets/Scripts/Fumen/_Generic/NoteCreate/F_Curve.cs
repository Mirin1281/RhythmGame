using System;
using UnityEngine;

namespace NoteCreating
{
    [AddTypeMenu(FumenPathContainer.NoteCreate + "カーブ", -60), System.Serializable]
    public class F_Curve : NoteCreateBase<NoteData>
    {
        [SerializeField] TransformConverter transformConverter;
        [SerializeField] float defaultCurve = 30;
        [SerializeField] bool isGroup;
        [SerializeField] float dirSpeed = 40f;

        [Header("オプション1 : カーブの半径 値が小さいほど効果が大きくなります")]
        [SerializeField] NoteData[] noteDatas = new NoteData[] { new(length: new Lpb(4), option1: 30) };
        protected override NoteData[] NoteDatas => noteDatas;

        protected override void Move(RegularNote note, NoteData data, float lifeTime)
        {
            float curve = data.Option1 is -1 or 0 ? defaultCurve : data.Option1;
            if (isGroup) // 実装が甘い
            {
                Vector3 centerPos = new Vector2(-curve, 0);
                var dirEasing = new Easing(MoveTime * dirSpeed, 0, MoveTime, EaseType.OutQuad);
                float time = Time;

                MoveNote(note, data, transformConverter, t =>
                {
                    Vector3 pos = default;
                    float rot = default;
                    /*if (t + time < MoveTime)
                    {
                        rot = dirEasing.Ease(t + time);
                        pos = MyUtility.GetRotatedPos(
                            new Vector3(data.X, (MoveTime - t) * Speed), rot, centerPos);
                    }
                    else // ロングの場合、始点を取った後は真っ直ぐ落とす
                    {
                        pos = new Vector3(data.X, (MoveTime - t) * Speed);
                        rot = 0;
                    }*/
                    if (t + time < 2 * MoveTime)
                    {
                        float dir = (MoveTime - t) * Speed / curve;
                        pos = new Vector3(data.X - curve, 0) + curve * new Vector3(Mathf.Cos(dir), Mathf.Sin(dir));
                        rot = t.Ease(MoveTime * Speed / curve, 0, MoveTime, EaseType.OutQuad) * Mathf.Rad2Deg;
                    }
                    else
                    {
                        Debug.Log(t + time);
                        pos = new Vector3(data.X, (MoveTime - t) * Speed);
                        rot = 0;
                    }
                    return (pos, rot);
                });
            }
            else
            {
                MoveNote(note, data, transformConverter, t =>
                {
                    Vector3 pos = default;
                    float rot = default;
                    if (t < MoveTime)
                    {
                        float dir = (MoveTime - t) * Speed / curve;
                        pos = new Vector3(data.X - curve, 0) + curve * new Vector3(Mathf.Cos(dir), Mathf.Sin(dir));
                        rot = t.Ease(MoveTime * Speed / curve, 0, MoveTime, EaseType.OutQuad) * Mathf.Rad2Deg;
                    }
                    else
                    {
                        pos = new Vector3(data.X, (MoveTime - t) * Speed);
                        rot = 0;
                    }
                    return (pos, rot);
                });
            }
        }

        protected override void AddExpect(RegularNote note, Vector2 pos = default, Lpb length = default, NoteJudgeStatus.ExpectType expectType = NoteJudgeStatus.ExpectType.Y_Static)
        {
            return;
        }

        protected override string GetSummary()
        {
            return NoteDatas?.Length + "    " + transformConverter.GetStatus() + mirror.GetStatusText();
        }
    }
}