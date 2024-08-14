using Cysharp.Threading.Tasks;

namespace NoteGenerating
{
    [UnityEngine.Scripting.APIUpdating.MovedFrom(false, null, null, "F_Sample")]
    [AddTypeMenu("Lyrith/1 サンプル"), System.Serializable]
    public class F_Lyrith1 : Generator_Type1
    {
        protected override async UniTask GenerateAsync()
        {
            await Loop(4, NoteType.Normal,
                null,
                1,
                2,
                1,
                0,
                null,
                -2,
                null,

                0,
                1,
                2,
                1,
                0
            );
            await Wait(8);
            await Loop(8, NoteType.Normal,
                3,
                3
            );

            
            //Hold(0, 1);
            /*await CreateSlides();


            async UniTask CreateSlides()
            {
                var currentDelta = Delta;
                int count = 24;
                Easing easing = new Easing(GetInverse(-5), GetInverse(5), count / 2, EaseType.OutQuad);
                for(int i = 0; i < count; i++)
                {
                    var value = easing.Ease(i);
                    Note(value, NoteType.Slide, currentDelta);
                    currentDelta = await WaitPlane(count, currentDelta);
                }
            }*/
        }
    }
}
