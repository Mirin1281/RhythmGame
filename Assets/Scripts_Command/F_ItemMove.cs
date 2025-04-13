using UnityEngine;
using Cysharp.Threading.Tasks;
using System;
using System.Linq;


#if UNITY_EDITOR
using UnityEditor;
#endif

namespace NoteCreating
{
    [UnityEngine.Scripting.APIUpdating.MovedFrom(false, null, "Assembly-CSharp", null)]
    [AddTypeMenu("◆判定線やノーツを生成して制御", -70), System.Serializable]
    public class F_ItemMove : CommandBase, INotSkipCommand
    {
        [Serializable]
        public struct EaseData<T> where T : struct
        {
            [SerializeField] T from;
            [SerializeField, Min(0)] Lpb easeTime;
            [SerializeField] EaseType easeType;

            public readonly T From => from;
            public readonly Lpb EaseLpb => easeTime;
            public readonly EaseType EaseType => easeType;

            public EaseData(T from = default, Lpb easeTime = default, EaseType easeType = EaseType.Default)
            {
                this.from = from;
                this.easeTime = easeTime;
                this.easeType = easeType;
            }
        }

        [Serializable]
        public class CreateData
        {
            [SerializeField, Min(0), Tooltip("生成を遅らせます")] Lpb delayLPB;
            [SerializeField] bool enabled = true;
            [SerializeField] Vector2 startPos;
            [SerializeField] EaseData<Vector2>[] posEaseDatas;
            [SerializeField] float startRot;
            [SerializeField] EaseData<float>[] rotEaseDatas;
            [SerializeField, Min(0)] float startAlpha = 1f;
            [SerializeField] EaseData<float>[] alphaEaseDatas;

            public Lpb DelayLPB => delayLPB;
            public bool Enabled => enabled;
            public Vector2 StartPos => startPos;
            public EaseData<Vector2>[] PosEaseDatas => posEaseDatas;
            public float StartRot => startRot;
            public EaseData<float>[] RotEaseDatas => rotEaseDatas;
            public float StartAlpha => startAlpha;
            public EaseData<float>[] AlphaEaseDatas => alphaEaseDatas;
        }

        public enum ItemType
        {
            NormalNote,
            SlideNote,
            HoldNote,
            Line,
        }

        [SerializeField] Mirror mirror;
#if UNITY_EDITOR
        [SerializeField] string summary;
#endif

        [SerializeField] ItemType itemType = ItemType.Line;
        [SerializeField] float option = 1; // Hold時に長さを設定する
        [SerializeField] bool setJudge;
        [SerializeField] bool isMultitap;

        [SerializeField, Min(0), Tooltip("判定線の生存時間")]
        Lpb lifeLpb = new Lpb(0.5f);

        [SerializeField, Tooltip("trueにするとdelayがリストの順で加算されます\nfalseだと元の時間からの差で発火します")]
        bool isChainWait = true;

        [SerializeField, Tooltip("基準の座標\n初期ではカメラの高さを設定しています")]
        Vector2 basePos = new Vector2(0, 4);

        [SerializeField, Tooltip("座標を\"特定の座標を軸に\"回転させます")]
        bool isRotateFromPos;
        [SerializeField] float rotateFromPos;
        [SerializeField] Vector2 centerPos;

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
            var item = GetItem(itemType);
            PosEase(item, data.StartPos, data.PosEaseDatas, delta).Forget();
            RotEase(item, data.StartRot, data.RotEaseDatas, delta).Forget();
            AlphaEase(item, data.StartAlpha, data.AlphaEaseDatas, delta).Forget();

            if (itemType == ItemType.HoldNote)
            {
                var hold = item as HoldNote;
                hold.SetLength(new Lpb(option) * Speed);
                hold.SetMaskPos(Vector2.one * 100);
            }
            else if (setJudge && itemType is ItemType.NormalNote or ItemType.SlideNote)
            {
                float lifeTime = lifeLpb.Time - delta;
                if (lifeTime > 0)
                {
                    var pos = data.StartPos;
                    if (data.PosEaseDatas.Length > 0)
                        pos = data.PosEaseDatas.Last().From;
                    pos = MyUtility.GetRotatedPos(pos + basePos, rotateFromPos, centerPos);

                    var judgeStatus = new NoteJudgeStatus(item as RegularNote, pos, lifeTime, expectType: NoteJudgeStatus.ExpectType.Static);
                    Helper.NoteInput.AddExpect(judgeStatus);
                }
            }

            await WaitSeconds(lifeLpb.Time, delta);
            item.SetActive(false);


            async UniTaskVoid PosEase(ItemBase item, Vector2 startPos, EaseData<Vector2>[] datas, float delta)
            {
                if (isRotateFromPos)
                {
                    item.SetPos(mirror.Conv(MyUtility.GetRotatedPos(startPos + basePos, rotateFromPos, centerPos)));
                }
                else
                {
                    item.SetPos(mirror.Conv(startPos + basePos));
                }
                //item.SetPos(mirror.Conv(startPos + basePos));
                for (int i = 0; i < datas.Length; i++)
                {
                    if (datas[i].EaseType == EaseType.None)
                    {
                        delta = await Wait(datas[i].EaseLpb, delta);
                        continue;
                    }
                    Vector2 start = GetBeforePos(i, datas);
                    var d = datas[i];

                    float posTime = d.EaseLpb.Time;
                    var posEasing = new EasingVector2(start, d.From, posTime, d.EaseType);
                    delta = await WhileYieldAsync(posTime, t =>
                    {
                        Vector2 p = default;
                        if (isRotateFromPos)
                        {
                            p = MyUtility.GetRotatedPos(posEasing.Ease(t) + basePos, rotateFromPos, centerPos);
                        }
                        else
                        {
                            p = posEasing.Ease(t) + basePos;
                        }
                        item.SetPos(mirror.Conv(p));
                    }, delta);
                }

                Vector2 GetBeforePos(int index, EaseData<Vector2>[] datas)
                {
                    if (index == 0) return startPos;
                    if (datas[index - 1].EaseType == EaseType.None)
                    {
                        return GetBeforePos(index - 1, datas);
                    }
                    else
                    {
                        return datas[index - 1].From;
                    }
                }
            }

            async UniTaskVoid RotEase(ItemBase item, float startRot, EaseData<float>[] datas, float delta)
            {
                float addRot = 0;
                if (isRotateFromPos)
                {
                    addRot = rotateFromPos;
                }
                item.SetRot(mirror.Conv(startRot) + addRot);
                for (int i = 0; i < datas.Length; i++)
                {
                    if (datas[i].EaseType == EaseType.None)
                    {
                        delta = await Wait(datas[i].EaseLpb, delta);
                        continue;
                    }
                    var d = datas[i];
                    float start = GetBeforeRot(i, datas);
                    float rotTime = d.EaseLpb.Time;
                    var rotEasing = new Easing(start, d.From, rotTime, d.EaseType);
                    delta = await WhileYieldAsync(rotTime, t =>
                    {
                        item.SetRot(mirror.Conv(rotEasing.Ease(t) + addRot));
                    }, delta);
                }

                float GetBeforeRot(int index, EaseData<float>[] datas)
                {
                    if (index == 0) return startRot;
                    if (datas[index - 1].EaseType == EaseType.None)
                    {
                        return GetBeforeRot(index - 1, datas);
                    }
                    else
                    {
                        return GetNormalizedAngle(datas[index - 1].From);
                    }
                }
                static float GetNormalizedAngle(float angle, float min = -180, float max = 180)
                {
                    return Mathf.Repeat(angle - min, max - min) + min;
                }
            }

            async UniTaskVoid AlphaEase(ItemBase item, float startAlpha, EaseData<float>[] datas, float delta)
            {
                item.SetAlpha(startAlpha);
                for (int i = 0; i < datas.Length; i++)
                {
                    if (datas[i].EaseType == EaseType.None)
                    {
                        delta = await Wait(datas[i].EaseLpb, delta);
                        continue;
                    }

                    var d = datas[i];
                    float start = GetBeforeAlpha(i, datas);
                    float fadeTime = d.EaseLpb.Time;
                    var alphaEasing = new Easing(start, d.From, fadeTime, d.EaseType);
                    delta = await WhileYieldAsync(fadeTime, t =>
                    {
                        item.SetAlpha(alphaEasing.Ease(t));
                    }, delta);
                }

                float GetBeforeAlpha(int index, EaseData<float>[] datas)
                {
                    if (index == 0) return startAlpha;
                    if (datas[index - 1].EaseType == EaseType.None)
                    {
                        return GetBeforeAlpha(index - 1, datas);
                    }
                    else
                    {
                        return datas[index - 1].From;
                    }
                }
            }
        }

        ItemBase GetItem(ItemType itemType)
        {
            ItemBase item = itemType switch
            {
                ItemType.NormalNote => Helper.PoolManager.RegularPool.GetNote(RegularNoteType.Normal),
                ItemType.SlideNote => Helper.PoolManager.RegularPool.GetNote(RegularNoteType.Slide),
                ItemType.HoldNote => Helper.PoolManager.HoldPool.GetNote(new Lpb(option) * Speed),
                ItemType.Line => Helper.GetLine(),
                _ => throw new ArgumentException()
            };

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