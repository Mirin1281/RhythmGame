using UnityEngine;
using CriWare;

namespace NoteCreating
{
    public class AudioWaveMeter : MonoBehaviour
    {
        [SerializeField] int monitoredChannelId = 0;
        CriAtomExOutputAnalyzer analyzer;
        public const int PcmSamples = 16;

        public void Init(CriAtomSource source)
        {
            CriAtomExOutputAnalyzer.Config config = new()
            {
                enablePcmCapture = true,
                numCapturedPcmSamples = PcmSamples
            };
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
        }
    }
}