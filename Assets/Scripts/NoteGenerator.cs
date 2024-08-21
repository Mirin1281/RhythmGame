using UnityEngine;
using Cysharp.Threading.Tasks;

namespace NoteGenerating
{
    public class NoteGenerator : MonoBehaviour
    {
        [SerializeField] FumenData fumenData;
        [SerializeField] Metronome metronome;
        [SerializeField] NoteGenerateHelper noteGenerateHelper;

        void Start()
        {
            metronome.OnBeat += GenerateProcessAsync;
        }
        void OnDestroy()
        {
            metronome.OnBeat -= GenerateProcessAsync;
        }

        void GenerateProcessAsync(int beatCount, float delta)
        {
            foreach(var generateData in fumenData.Fumen.GetReadOnlyGenerateDataList())
            {
                if(beatCount == generateData.BeatTiming)
                {
                    generateData.Generate(noteGenerateHelper, delta);
                }
            }
        }
    }
}
