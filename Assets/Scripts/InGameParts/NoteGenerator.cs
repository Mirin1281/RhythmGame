using UnityEngine;
using Cysharp.Threading.Tasks;

namespace NoteGenerating
{
    public class NoteGenerator : MonoBehaviour
    {
        [SerializeField] InGameManager inGameManager;
        [SerializeField] Metronome metronome;
        [SerializeField] NoteGenerateHelper noteGenerateHelper;

        void Awake()
        {
            metronome.OnBeat += GenerateProcessAsync;
        }

        void GenerateProcessAsync(int beatCount, float delta)
        {
            foreach(var generateData in inGameManager.FumenData.Fumen.GetReadOnlyGenerateDataList())
            {
                if(beatCount == generateData.BeatTiming)
                {
                    generateData.Generate(noteGenerateHelper, delta);
                }
            }
        }
    }
}
