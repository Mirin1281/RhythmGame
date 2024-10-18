using System;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace NoteGenerating
{
    [AddTypeMenu("◆スカイ"), System.Serializable]
    public class F_Sky : Generator_Common
    {
        [Serializable]
        public struct SkyNoteData
        {
            [SerializeField] Vector2 pos;
            [SerializeField, Min(0)] float wait;
            [SerializeField] bool disable;
            
            public readonly Vector2 Pos => pos;
            public readonly float Wait => wait;
            public readonly bool Disable => disable;
        }

        [SerializeField] SkyNoteData[] noteDatas = new SkyNoteData[1];

        protected override async UniTask GenerateAsync()
        {
            foreach(var data in noteDatas)
            {
                if(data.Disable == false)
                {
                    Sky(data.Pos);
                }
                await Wait(data.Wait);
            }
        }

        protected override Color GetCommandColor()
        {
            return ConstContainer.VersatileCommandColor;
        }

        protected override string GetSummary()
        {
            return noteDatas.Length + GetInverseSummary();
        }

        public override void Preview()
        {
            GameObject previewObj = MyUtility.GetPreviewObject();

            float z = 0f;
            for(int i = 0; i < noteDatas.Length; i++)
            {
                var data = noteDatas[i];
                if(data.Disable == false)
                {
                    SkyNote(data.Pos, z);
                }
                z += Helper.GetTimeInterval(data.Wait) * Speed3D;
            }

            float lineZ = 0f;
            for(int i = 0; i < 10000; i++)
            {
                var line = Helper.PoolManager.LinePool.GetLine(1);
                line.transform.localPosition = new Vector3(0, 0, lineZ);
                line.transform.SetParent(previewObj.transform);
                lineZ += Helper.GetTimeInterval(4) * Speed3D;
                if(lineZ > z) break;
            }

            void SkyNote(Vector2 pos, float z)
            {
                SkyNote sky = Helper.GetSky();
                var startPos = new Vector3(Inv(pos.x), pos.y, z);
                sky.SetPos(startPos);
                sky.transform.SetParent(previewObj.transform);
            }
        }

        public override string CSVContent1
        {
            get => MyUtility.GetContentFrom(noteDatas);
            set => noteDatas = MyUtility.GetArrayFrom<SkyNoteData>(value);
        }
    }
}
