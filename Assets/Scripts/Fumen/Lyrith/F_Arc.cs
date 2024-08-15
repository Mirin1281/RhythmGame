using Cysharp.Threading.Tasks;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace NoteGenerating
{
    [AddTypeMenu("Lyrith/アークテスト"), System.Serializable]
    public class F_Arc : Generator_Type1
    {
        [SerializeField] DebugSphere prefab;
        [SerializeField] ArcNote.ColorType defaultColor = ArcNote.ColorType.Red;
        [SerializeField] ArcCreateData[] datas;

        protected override float Speed => base.Speed * 5f;
        protected override float From => 0f;

        protected override async UniTask GenerateAsync()
        {
            await UniTask.CompletedTask;
            var arc = Helper.ArcNotePool.GetNote();
            arc.CreateNewArc(datas, Helper.Metronome.Bpm, Speed, IsInverse);
            if(IsInverse)
            {
                if(defaultColor == ArcNote.ColorType.Red)
                {
                    arc.SetColor(ArcNote.ColorType.Blue);
                }
                else if(defaultColor == ArcNote.ColorType.Blue)
                {
                    arc.SetColor(ArcNote.ColorType.Red);
                }
            }
            else
            {
                arc.SetColor(defaultColor);
            }
            var startPos = new Vector3(0, 0f, StartBase);
            LinearMoveAsync(arc, startPos).Forget();
            Helper.NoteInput.AddArc(arc);
        }

        async UniTask LinearMoveAsync(ArcNote arc, Vector3 startPos)
        {
            float baseTime = CurrentTime - Delta;
            var vec = Speed * new Vector3(0, 0, -1);
            while (arc.IsActive)
            {
                float time = CurrentTime - baseTime;
                arc.SetPos(startPos + time * vec);
                await UniTask.Yield(Helper.Token);
            }
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
            if(IsInverse)
            {
                if(defaultColor == ArcNote.ColorType.Red)
                {
                    arc.SetColor(ArcNote.ColorType.Blue);
                }
                else if(defaultColor == ArcNote.ColorType.Blue)
                {
                    arc.SetColor(ArcNote.ColorType.Red);
                }
            }
            else
            {
                arc.SetColor(defaultColor);
            }
            arc.DebugCreateNewArc(datas, 177f, Speed, IsInverse, prefab);
            SceneView.RepaintAll();
#endif
        }
    }
}
