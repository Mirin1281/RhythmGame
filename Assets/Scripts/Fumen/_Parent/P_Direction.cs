using Cysharp.Threading.Tasks;
using UnityEngine;

namespace NoteGenerating
{
    [AddTypeMenu("角度変更"), System.Serializable]
    public class P_Direction : ParentGeneratorBase
    {
        [SerializeField] float deg;

        protected override async UniTask MoveParentAsync(NoteBase parent)
        {
            parent.SetRotate(ConvertIfInverse(deg));
            await UniTask.CompletedTask;
        }

        public override string CSVContent1
        {
            get => deg.ToString();
            set => deg = float.Parse(value);
        }
    }
}