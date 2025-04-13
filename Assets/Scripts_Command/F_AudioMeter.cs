using Cysharp.Threading.Tasks;
using UnityEngine;

namespace NoteCreating
{
    [AddTypeMenu("◇オーディオ波形"), System.Serializable]
    public class F_AudioMeter : CommandBase
    {
        [Header(nameof(FumenData) + "からPcmAnalyzerSamplesを予め設定してください")]
        [SerializeField] Mirror mirror;
        [SerializeField] int zip = 4;
        [Space(20)]
        [SerializeField] Lpb lifeLpb = new Lpb(4);
        [SerializeField] int lifeCount = 32;
        [Space(20)]
        [SerializeField] float alpha = 0.2f;
        [SerializeField] float fadeTime = 0.3f;
        [SerializeField] float amp = 3f;
        [SerializeField] float height = 4f;

        protected override async UniTaskVoid ExecuteAsync()
        {
            await Wait(MoveLpb);
            int itemCount = Helper.WaveMeter.PcmSamples / zip;
            ItemBase[] items = new ItemBase[itemCount];
            for (int i = 0; i < items.Length; i++)
            {
                var item = Helper.GetRegularNote(RegularNoteType.Slide);
                items[i] = item;
                float x = (i - ((itemCount - 1) / 2f)) / (itemCount / 32f * 1.5f);
                item.SetPos(new Vector3(x, 0));
                item.SetRot(90);
                item.SetAlpha(0f);
                item.FadeAlphaAsync(alpha, fadeTime).Forget();
            }

            float[] pcmData = new float[Helper.WaveMeter.PcmSamples];
            await WhileYieldAsync(lifeLpb.Time * lifeCount, _ =>
            {
                Helper.WaveMeter.GetPcmData(ref pcmData);
                for (int i = 0; i < itemCount; i++)
                {
                    var item = items[i];
                    float y = pcmData[i * zip] * amp + height;
                    item.SetPos(new Vector3(item.GetPos().x, y));
                }
            });

            for (int i = 0; i < items.Length; i++)
            {
                var item = items[i];
                item.SetActive(false);
            }
        }

#if UNITY_EDITOR

        protected override Color GetCommandColor()
        {
            return CommandEditorUtility.CommandColor_Other;
        }

        protected override string GetSummary()
        {
            return $"Length: {lifeLpb / new Lpb(4) * lifeCount}";
        }
#endif
    }
}
