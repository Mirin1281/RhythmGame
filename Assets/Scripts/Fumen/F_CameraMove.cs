using Cysharp.Threading.Tasks;
using UnityEngine;

namespace NoteGenerating
{
    [AddTypeMenu("◆カメラ制御"), System.Serializable]
    public class F_CameraMove : Generator_2D
    {
        [SerializeField, Min(0)] float delay = 0f;
        [SerializeField] CameraMoveSetting[] settings = new CameraMoveSetting[1];

        protected override async UniTask GenerateAsync()
        {
            await Helper.WaitSeconds(delay);
            await Wait(4, RhythmGameManager.DefaultWaitOnAction);

            for(int i = 0; i < settings.Length; i++)
            {
                var s = settings[i];
                Helper.CameraMover.Move(s, IsInverse);
                await Wait(s.Wait);
            }
        }

        protected override Color GetCommandColor()
        {
            return ConstContainer.UnNoteCommandColor;
        }

        public override string CSVContent1
        {
            get => IsInverse + "|" + delay;
            set
            {
                var texts = value.Split("|");
                SetInverse(bool.Parse(texts[0]));
                delay = float.Parse(texts[1]);
            }
        }

        public override string CSVContent2
        {
            get
            {
                string text = string.Empty;
                for(int i = 0; i < settings.Length; i++)
                {
                    var data = settings[i];
                    if(data == null) continue;
                    text += data.Wait + "|";
                    text += data.IsPosMove + "|";
                    text += data.Pos + "|";
                    text += data.IsRotateMove + "|";
                    text += data.Rotate + "|";
                    text += data.IsRotateClamp + "|";
                    text += data.Time + "|";
                    text += data.EaseType + "|";
                    text += data.MoveType;
                    if(i == settings.Length - 1) break;
                    text += "\n";
                }
                return text;
            }
            set
            {
                var texts = value.Split("\n");
                if(texts.Length == 1 && string.IsNullOrEmpty(texts[0])) return;
                var settings = new CameraMoveSetting[texts.Length];
                for(int i = 0; i < texts.Length; i++)
                {
                    var contents = texts[i].Split('|');
                    settings[i] = new CameraMoveSetting(
                        bool.Parse(contents[1]),
                        contents[2].ToVector3(),
                        bool.Parse(contents[3]),
                        contents[4].ToVector3(),
                        bool.Parse(contents[5]),
                        float.Parse(contents[6]),
                        EaseType.Parse<EaseType>(contents[7]),
                        CameraMoveType.Parse<CameraMoveType>(contents[8]),
                        int.Parse(contents[0])
                    );
                }
                this.settings = settings;
            }
        }
    }
}
