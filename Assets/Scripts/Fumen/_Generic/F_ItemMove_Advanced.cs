using UnityEngine;
using Cysharp.Threading.Tasks;
using System;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace NoteCreating
{
    // 回転(revolute)のイージングに対応
    [AddTypeMenu("◆判定線やノーツを生成して制御(拡張)", -70), System.Serializable]
    public class F_ItemMove_Advanced : CommandBase, INotSkipCommand
    {
        [Serializable]
        public class CreateData
        {
            [SerializeField, Tooltip("生成を遅らせます")] Lpb delayLPB;
            [SerializeField] bool enabled = true;
            [SerializeField] Vector2 startPos;
            [SerializeField] F_ItemMove.EaseData<Vector2>[] posEaseDatas;
            [SerializeField] float startRot;
            [SerializeField] F_ItemMove.EaseData<float>[] rotEaseDatas;
            [SerializeField, Min(0)] float startAlpha = 1f;
            [SerializeField] F_ItemMove.EaseData<float>[] alphaEaseDatas;
            [SerializeField] float startRevolute;
            [SerializeField] F_ItemMove.EaseData<float>[] revoluteEaseDatas;

            public Lpb DelayLPB => delayLPB;
            public bool Enabled => enabled;
            public Vector2 StartPos => startPos;
            public F_ItemMove.EaseData<Vector2>[] PosEaseDatas => posEaseDatas;
            public float StartRot => startRot;
            public F_ItemMove.EaseData<float>[] RotEaseDatas => rotEaseDatas;
            public float StartAlpha => startAlpha;
            public F_ItemMove.EaseData<float>[] AlphaEaseDatas => alphaEaseDatas;
            public float StartRevolute => startRevolute;
            public F_ItemMove.EaseData<float>[] RevoluteEaseDatas => revoluteEaseDatas;
        }

        public enum ItemType
        {
            NormalNote,
            SlideNote,
            HoldNote,
            Line,
        }

        [SerializeField] Mirror mirror;
        [SerializeField] string summary;

        [SerializeField] ItemType itemType = ItemType.Line;

        [Space(10)]
        [SerializeField] float option = 1; // Hold時に長さを設定する
        [SerializeField] bool setJudge;
        [SerializeField] bool isMultitap;

        [Space(10)]
        [SerializeField, Tooltip("判定線の生存時間")]
        Lpb lifeLpb = new Lpb(0.5f);

        [SerializeField, Tooltip("trueにするとdelayがリストの順で加算されます\nfalseだと元の時間からの差で発火します")]
        bool isChainWait = false;

        [SerializeField, Tooltip("基準の座標\n初期ではカメラの高さを設定しています")]
        Vector2 basePos = new Vector2(0, 0);

        [SerializeField] Vector2 revoluteCenterPos = new Vector2(0, 4);

        [SerializeField] CreateData[] createDatas = new CreateData[1];

        protected override async UniTaskVoid ExecuteAsync()
        {
            float delta = await Wait(MoveLpb, Delta);
            if (isChainWait)
            {
                LoopCreate(createDatas, delta).Forget();
            }
            else
            {
                for (int k = 0; k < createDatas.Length; k++)
                {
                    Create(createDatas[k], delta, true).Forget();
                }
            }


            async UniTaskVoid LoopCreate(CreateData[] datas, float delta)
            {
                for (int i = 0; i < datas.Length; i++)
                {
                    var d = datas[i];
                    delta = await Wait(d.DelayLPB, delta: delta);
                    Create(d, delta, false).Forget();
                }
            }
        }

        async UniTaskVoid Create(CreateData data, float delta, bool isWait)
        {
            if (data.Enabled == false || delta > lifeLpb.Time) return;
            if (isWait)
            {
                delta = await Wait(data.DelayLPB, delta: delta);
            }

            var item = GetItem(itemType, delta);

            Vector2 startPos = data.StartPos;
            int posIndex = data.PosEaseDatas.Length != 0 ? 0 : int.MaxValue;
            float previousPosTime = 0;
            float posTime = data.PosEaseDatas.Length != 0 ? 0 : float.MaxValue;
            EasingVector2 posEasing = default;

            float startRot = data.StartRot;
            int rotIndex = data.RotEaseDatas.Length != 0 ? 0 : int.MaxValue;
            float previousRotTime = 0;
            float rotTime = data.RotEaseDatas.Length != 0 ? 0 : float.MaxValue;
            Easing rotEasing = default;

            float startAlpha = data.StartAlpha;
            int alphaIndex = data.AlphaEaseDatas.Length != 0 ? 0 : int.MaxValue;
            float previousAlphaTime = 0;
            float alphaTime = data.AlphaEaseDatas.Length != 0 ? 0 : float.MaxValue;
            Easing alphaEasing = default;

            float startRevolute = data.StartRevolute;
            int revoluteIndex = data.RevoluteEaseDatas.Length != 0 ? 0 : int.MaxValue;
            float previousRevoluteTime = 0;
            float revoluteTime = data.RevoluteEaseDatas.Length != 0 ? 0 : float.MaxValue;
            Easing revoluteEasing = default;

            await WhileYieldAsync(lifeLpb.Time, t =>
            {
                Vector2 pos = GetPosition(t, data.PosEaseDatas);
                float rot = GetRotation(t, data.RotEaseDatas);
                float alpha = GetAlpha(t, data.AlphaEaseDatas);
                float revolute = GetRevolute(t, data.RevoluteEaseDatas);

                item.SetPos(mirror.Conv(MyUtility.GetRotatedPos(pos, revolute, revoluteCenterPos) + basePos));
                item.SetRot(mirror.Conv(GetNormalizedAngle(rot) + revolute));
                item.SetAlpha(alpha);
            }, delta);

            item.SetActive(false);


            Vector2 GetPosition(float t, F_ItemMove.EaseData<Vector2>[] posDatas)
            {
                if (t >= posTime)
                {
                    if (posIndex < posDatas.Length)
                    {
                        previousPosTime = posTime;
                        posTime += posDatas[posIndex].EaseLpb.Time;
                        posEasing = GetEasing(posIndex);
                        if (posDatas[posIndex].EaseType != EaseType.None)
                        {
                            startPos = posDatas[posIndex].From;
                        }
                    }
                    posIndex++;
                }

                Vector2 pos;
                if (posIndex >= posDatas.Length + 1)
                {
                    pos = startPos;
                }
                else
                {
                    pos = posEasing.Ease(t - previousPosTime);
                }
                return pos;


                EasingVector2 GetEasing(int index)
                {
                    var d = posDatas[posIndex];
                    if (index == 0)
                    {
                        return new EasingVector2(data.StartPos, d.From, d.EaseLpb.Time, d.EaseType);
                    }
                    else if (posDatas[index - 1].EaseType == EaseType.None)
                    {
                        return GetEasing(index - 1);
                    }
                    else
                    {
                        return new EasingVector2(posDatas[index - 1].From, d.From, d.EaseLpb.Time, d.EaseType);
                    }
                }
            }

            float GetRotation(float t, F_ItemMove.EaseData<float>[] rotDatas)
            {
                if (t >= rotTime)
                {
                    if (rotIndex < rotDatas.Length)
                    {
                        previousRotTime = rotTime;
                        rotTime += rotDatas[rotIndex].EaseLpb.Time;
                        rotEasing = GetEasing(rotIndex);
                        if (rotDatas[rotIndex].EaseType != EaseType.None)
                        {
                            startRot = rotDatas[rotIndex].From;
                        }
                    }
                    rotIndex++;
                }

                float rot;
                if (rotIndex >= rotDatas.Length + 1)
                {
                    rot = startRot;
                }
                else
                {
                    rot = rotEasing.Ease(t - previousRotTime);
                }
                return rot;


                Easing GetEasing(int index)
                {
                    var d = rotDatas[rotIndex];
                    if (index == 0)
                    {
                        return new Easing(data.StartRot, d.From, d.EaseLpb.Time, d.EaseType);
                    }
                    else if (rotDatas[index - 1].EaseType == EaseType.None)
                    {
                        return GetEasing(index - 1);
                    }
                    else
                    {
                        return new Easing(rotDatas[index - 1].From, d.From, d.EaseLpb.Time, d.EaseType);
                    }
                }
            }

            float GetAlpha(float t, F_ItemMove.EaseData<float>[] alphaDatas)
            {
                if (t >= alphaTime)
                {
                    if (alphaIndex < alphaDatas.Length)
                    {
                        previousAlphaTime = alphaTime;
                        alphaTime += alphaDatas[alphaIndex].EaseLpb.Time;
                        alphaEasing = GetEasing(alphaIndex);
                        if (alphaDatas[alphaIndex].EaseType != EaseType.None)
                        {
                            startAlpha = alphaDatas[alphaIndex].From;
                        }
                    }
                    alphaIndex++;
                }

                float alpha;
                if (alphaIndex >= alphaDatas.Length + 1)
                {
                    alpha = startAlpha;
                }
                else
                {
                    alpha = alphaEasing.Ease(t - previousAlphaTime);
                }
                return alpha;


                Easing GetEasing(int index)
                {
                    var d = alphaDatas[alphaIndex];
                    if (index == 0)
                    {
                        return new Easing(data.StartAlpha, d.From, d.EaseLpb.Time, d.EaseType);
                    }
                    else if (alphaDatas[index - 1].EaseType == EaseType.None)
                    {
                        return GetEasing(index - 1);
                    }
                    else
                    {
                        return new Easing(alphaDatas[index - 1].From, d.From, d.EaseLpb.Time, d.EaseType);
                    }
                }
            }

            float GetRevolute(float t, F_ItemMove.EaseData<float>[] revoluteDatas)
            {
                if (t >= revoluteTime)
                {
                    if (revoluteIndex < revoluteDatas.Length)
                    {
                        previousRevoluteTime = revoluteTime;
                        revoluteTime += revoluteDatas[revoluteIndex].EaseLpb.Time;
                        revoluteEasing = GetEasing(revoluteIndex);
                        if (revoluteDatas[revoluteIndex].EaseType != EaseType.None)
                        {
                            startRevolute = revoluteDatas[revoluteIndex].From;
                        }
                    }
                    revoluteIndex++;
                }

                float revolute;
                if (revoluteIndex >= revoluteDatas.Length + 1)
                {
                    revolute = startRevolute;
                }
                else
                {
                    revolute = revoluteEasing.Ease(t - previousRevoluteTime);
                }
                return revolute;


                Easing GetEasing(int index)
                {
                    var d = revoluteDatas[revoluteIndex];
                    if (index == 0)
                    {
                        return new Easing(data.StartRevolute, d.From, d.EaseLpb.Time, d.EaseType);
                    }
                    else if (revoluteDatas[index - 1].EaseType == EaseType.None)
                    {
                        return GetEasing(index - 1);
                    }
                    else
                    {
                        return new Easing(revoluteDatas[index - 1].From, d.From, d.EaseLpb.Time, d.EaseType);
                    }
                }
            }

            static float GetNormalizedAngle(float angle, float min = -180, float max = 180)
            {
                return Mathf.Repeat(angle - min, max - min) + min;
            }
        }

        ItemBase GetItem(ItemType itemType, float delta)
        {
            ItemBase item = itemType switch
            {
                ItemType.NormalNote => Helper.PoolManager.RegularPool.GetNote(RegularNoteType.Normal),
                ItemType.SlideNote => Helper.PoolManager.RegularPool.GetNote(RegularNoteType.Slide),
                ItemType.HoldNote => Helper.PoolManager.HoldPool.GetNote(new Lpb(option) * Speed),
                ItemType.Line => Helper.GetLine(),
                _ => throw new ArgumentException()
            };

            if (itemType == ItemType.HoldNote)
            {
                var hold = item as HoldNote;
                hold.SetMaskPos(Vector2.one * 100);
            }
            else if (setJudge && itemType is ItemType.NormalNote or ItemType.SlideNote)
            {
                float lifeTime = lifeLpb.Time - delta;
                if (lifeTime > 0)
                    Helper.NoteInput.AddExpect(item as RegularNote, lifeTime);
            }

            if (isMultitap && itemType == ItemType.NormalNote)
            {
                Helper.PoolManager.SetMultitapSprite(item as RegularNote);
            }

#if UNITY_EDITOR
            if (EditorApplication.isPlaying == false)
            {
                item.transform.SetParent(previewer.transform);
            }
#endif
            return item;
        }

#if UNITY_EDITOR

        protected override string GetName()
        {
            return "ItemMove-Ad";
        }

        protected override Color GetCommandColor()
        {
            return CommandEditorUtility.CommandColor_Yellow;
        }

        protected override string GetSummary()
        {
            string s = $"Length: {createDatas.Length}{mirror.GetStatusText()}";
            if (string.IsNullOrWhiteSpace(summary))
            {
                return s;
            }
            else
            {
                return summary + " : " + s;
            }
        }

        public override void OnPeriod()
        {
            DebugPreview();
        }

        ItemPreviewer previewer;

        void DebugPreview(bool beforeClear = true, int beatDelta = 1)
        {
            previewer = CommandEditorUtility.GetPreviewer(beforeClear);
            if (isChainWait)
            {
                LoopCreate(createDatas, 0).Forget();
            }
            else
            {
                for (int k = 0; k < createDatas.Length; k++)
                {
                    Create(createDatas[k], 0, true).Forget();
                }
            }


            async UniTaskVoid LoopCreate(CreateData[] datas, float delta)
            {
                for (int i = 0; i < datas.Length; i++)
                {
                    delta = await Wait(datas[i].DelayLPB, delta: delta);
                    Create(datas[i], delta, false).Forget();
                }
            }
        }
#endif
    }
}