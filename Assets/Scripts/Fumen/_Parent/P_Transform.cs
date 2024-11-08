using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

namespace NoteGenerating
{
    [MovedFrom(false, null, null, "P_Pos")]
    [AddTypeMenu("座標と角度変更"), System.Serializable]
    public class P_Transform : ParentGeneratorBase
    {
        [SerializeField] Vector2 pos;
        [SerializeField] float deg;

        protected override async UniTask MoveParentAsync(NoteBase parent)
        {
            parent.SetPos(pos);
            parent.SetRotate(Inv(deg));
            await UniTask.CompletedTask;
        }

        public override string CSVContent1
        {
            get => pos + "=" + deg;
            set
            {
                var texts = value.Split("=");
                pos = texts[0].ToVector3();
                deg = float.Parse(texts[1]);
            }
        }
    }
}