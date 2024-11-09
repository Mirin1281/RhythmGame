using System;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace NoteGenerating
{
    [AddTypeMenu("◆スカイ", -1), System.Serializable]
    public class F_Sky : Generator_Common
    {
        [Serializable]
        public struct SkyNoteData
        {
            [SerializeField] bool disable;
            [SerializeField, Min(0)] float wait;
            [SerializeField] Vector2 pos;
            
            public readonly bool Disable => disable;
            public readonly float Wait => wait;
            public readonly Vector2 Pos => pos;
        }

        [SerializeField] SkyNoteData[] noteDatas = new SkyNoteData[1];

        protected override async UniTask GenerateAsync()
        {
            foreach(var data in noteDatas)
            {
                await Wait(data.Wait);
                if(data.Disable) continue;
                Sky(data.Pos);
            }
        }

        protected override Color GetCommandColor()
        {
            return ConstContainer.NoteCommandColor;
        }

        protected override string GetSummary()
        {
            return noteDatas.Length + GetInverseSummary();
        }

        public override void Preview()
        {
            GameObject previewObj = FumenDebugUtility.GetPreviewObject();

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
    }
}
