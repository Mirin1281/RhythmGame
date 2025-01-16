using Cysharp.Threading.Tasks;
using UnityEngine;
using System;

namespace NoteCreating
{
    [AddTypeMenu("◆ギザギザアーク", -1), System.Serializable]
    public class F_JaggedArc : Command_General
    {
        [SerializeField] float delayLPB;
        [SerializeField] float jagInterval = 16f;
        [SerializeField] float length = 2;
        [Space(20)]
        [SerializeField] float startWidth;
        [SerializeField] float fromWidth = 6;
        [SerializeField] EaseType easeType = EaseType.Linear;
        [Space(20)]
        [SerializeField] float rotate;

        protected override async UniTask ExecuteAsync()
        {
            int count = Mathf.RoundToInt(jagInterval / length);
            var easing = new Easing(startWidth, fromWidth, count, easeType);
            ArcCreateData[] datas = new ArcCreateData[count];
            await Wait(delayLPB);
            for (int i = 0; i < count; i++)
            {
                int a = i % 2 == 0 ? -1 : 1;
                Vector2 pos = new Vector2(easing.Ease(i) * a, 0);
                datas[i] = new ArcCreateData(pos.x, i == 0 ? 0 : jagInterval, ArcCreateData.VertexType.Linear, false, true, 0, jagInterval);
            }
            Arc(datas);
        }

        protected override Color GetCommandColor()
        {
            return new Color32(160, 190, 240, 255);
        }

        protected override string GetSummary()
        {
            return $"Length: {length}{GetMirrorSummary()}";
        }

        protected override string GetName()
        {
            return "ギザギザアーク";
        }

        /*public override async void Preview()
        {
            var arc = GameObject.FindAnyObjectByType<ArcNote>(FindObjectsInactive.Include);
            if (arc == null)
            {
                Debug.LogWarning("ヒエラルキー上にアークノーツを設置してください");
                return;
            }
            arc.SetActive(true);
            arc.SetRadius(0.4f);
            arc.SetPos(new Vector3(0, 0, 0.5f));
            arc.SetRotate(new Vector3(-90f, 0f, 0f));
            arc.Is2D = true;

            float speed = Speed;

            int count = Mathf.RoundToInt(jagInterval / length);
            var easing = new Easing(startWidth, fromWidth, count, easeType);
            ArcCreateData[] datas = new ArcCreateData[count];
            for (int i = 0; i < count; i++)
            {
                int a = i % 2 == 0 ? -1 : 1;
                Vector2 pos = is2D ?
                    new Vector2(easing.Ease(i) * a, 0) :
                    MyUtility.GetRotatedPos(new Vector2(easing.Ease(i) * a, 0), rotate) + Inv(posOffset);
                datas[i] = new ArcCreateData(new Vector3(Inv(pos.x), pos.y, jagInterval), ArcCreateData.VertexType.Linear, false, true, 0, jagInterval);
            }
            await arc.DebugCreateNewArcAsync(datas, Helper.GetTimeInterval(1) * speed, IsInverse, Helper.DebugSpherePrefab);

            GameObject previewObj = FumenDebugUtility.GetPreviewObject();
            float lineY = 0f;
            for (int i = 0; i < 1000; i++)
            {
                var line = Helper.PoolManager.LinePool.GetLine(is2D ? 0 : 1);
                line.SetPos(is2D ? new Vector3(0, lineY) : new Vector3(0, 0.01f, lineY));
                line.SetAlpha(0.7f);
                line.transform.SetParent(previewObj.transform);
                lineY += Helper.GetTimeInterval(4) * speed;
                if (lineY > arc.LastZ) break;
            }
        }*/
    }
}
