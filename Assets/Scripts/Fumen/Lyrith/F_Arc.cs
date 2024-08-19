using Cysharp.Threading.Tasks;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace NoteGenerating
{
    [AddTypeMenu("Lyrith/◆アーク"), System.Serializable]
    public class F_Arc : Generator_Type1
    {
        [SerializeField] DebugSphere prefab;
        [SerializeField] ArcColorType defaultColor = ArcColorType.Red;
        [SerializeField] ArcCreateData[] datas;

        protected override float Speed => RhythmGameManager.Speed3D;

        protected override async UniTask GenerateAsync()
        {
            await UniTask.CompletedTask;
            var arc = Helper.ArcNotePool.GetNote();
            arc.CreateNewArcAsync(datas, Helper.Metronome.Bpm, Speed, IsInverse).Forget();
            arc.SetColor(defaultColor, IsInverse);

            var startPos = new Vector3(0, 0f, StartBase);
            DropAsync(arc, startPos).Forget();
            Helper.NoteInput.AddArc(arc);
        }

        async UniTask DropAsync(ArcNote arc, Vector3 startPos)
        {
            float baseTime = CurrentTime - Delta;
            var vec = Speed * Vector3.back;
            while (arc.IsActive)
            {
                float time = CurrentTime - baseTime;
                arc.SetPos(startPos + time * vec);
                await UniTask.Yield(Helper.Token);
            }
        }

        protected override Color GetCommandColor()
        {
            ArcColorType type = ArcColorType.None;
            if(IsInverse)
            {
                if(defaultColor == ArcColorType.Red)
                {
                    type = ArcColorType.Blue;
                }
                else if(defaultColor == ArcColorType.Blue)
                {
                    type = ArcColorType.Red;
                }
            }
            else
            {
                type = defaultColor;
            }
            return type switch
            {
                ArcColorType.Red => new Color32(240, 180, 200, 255),
                ArcColorType.Blue => new Color32(160, 200, 255, 255),
                _ => base.GetCommandColor()
            };
        }

        protected override string GetSummary()
        {
            int judgeCount = 0;
            for(int i = 0; i < datas.Length; i++)
            {
                if(i == datas.Length - 1) break;
                if(datas[i].IsJudgeDisable == false)
                {
                    judgeCount++;
                }
            }
            return $"判定数: {judgeCount}";
        }

        protected override void Preview()
        {
#if UNITY_EDITOR
            var arc = GameObject.FindAnyObjectByType<ArcNote>(FindObjectsInactive.Include);
            Selection.activeGameObject = arc.gameObject;
            arc.SetActive(true);
            arc.DebugCreateNewArcAsync(datas, 177f, Speed, IsInverse, prefab).Forget();
            arc.SetColor(defaultColor, IsInverse);
            //SceneView.RepaintAll();
#endif
        }
    }
}
