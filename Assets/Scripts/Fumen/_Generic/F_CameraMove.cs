using Cysharp.Threading.Tasks;
using UnityEngine;

namespace NoteGenerating
{
    [AddTypeMenu("◇カメラ制御"), System.Serializable]
    public class F_CameraMove : Generator_Common
    {
        [SerializeField, Min(0)] int loopCount = 1;
        [SerializeField, Min(0)] float loopWait = 4;
        [SerializeField, Min(0)] float delay = 0f;
        [SerializeField] CameraMoveSetting[] settings = new CameraMoveSetting[1];

        protected override async UniTask GenerateAsync()
        {
            await Helper.WaitSeconds(delay);
            await Wait(4, RhythmGameManager.DefaultWaitOnAction);

            for(int k = 0; k < loopCount; k++)
            {
                Create().Forget();
                await Wait(loopWait);
            }
        }

        async UniTaskVoid Create()
        {
            for(int i = 0; i < settings.Length; i++)
            {
                var s = settings[i];
                await Wait(s.Wait);
                Helper.CameraMover.Move(s, IsInverse);
            }
        }

        protected override Color GetCommandColor()
        {
            return ConstContainer.UnNoteCommandColor;
        }

        public override string CSVContent1
        {
            get => MyUtility.GetContentFrom(loopCount, loopWait, delay);
            set
            {
                var texts = value.Split("|");
                loopCount = int.Parse(texts[0]);
                loopWait = float.Parse(texts[1]);
                delay = float.Parse(texts[2]);
            }
        }
        
        public override string CSVContent2
        {
            get => MyUtility.GetContentFrom(settings);
            set => settings = MyUtility.GetArrayFrom<CameraMoveSetting>(value);
        }
    }
}
