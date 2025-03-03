using UnityEngine;
using CriWare;

namespace NoteCreating
{
    public class AudioWaveMeter : MonoBehaviour
    {
        [SerializeField] int monitoredChannelId = 0;
        CriAtomExOutputAnalyzer analyzer;
        public int PcmSamples { get; private set; }

        public void Init(CriAtomSource source, int samples)
        {
            CriAtomExOutputAnalyzer.Config config = new()
            {
                enablePcmCapture = true,
                numCapturedPcmSamples = samples
            };
            PcmSamples = samples;
            analyzer = new CriAtomExOutputAnalyzer(config);
            analyzer.AttachExPlayer(source.player);
        }

        void OnDestroy()
        {
            if (analyzer != null)
            {
                analyzer.DetachExPlayer();
                analyzer.Dispose();
            }
        }

        public void GetPcmData(ref float[] pcmData)
        {
            if (analyzer != null)
            {
                analyzer.GetPcmData(ref pcmData, monitoredChannelId);
            }
            else
            {
                Debug.LogWarning("Must Init Analyzer!");
            }
        }
    }
}