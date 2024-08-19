using UnityEngine;
using Cysharp.Threading.Tasks;
using System;
using CriWare;

public class Metronome : MonoBehaviour
{
    [SerializeField] CriAtomSource criAtomSource;
    [SerializeField] MusicData musicData;
    [SerializeField] bool autoStart;
    [Space(10)]
    [SerializeField] bool skipOnStart;
    [SerializeField, Range(0, 1)] float timeRate;

    bool addTime;
    float bpm;
    double currentTime;
    double interval;
    int bpmChangeCount;
    CriAtomExPlayback playback;

    /// <summary>
    /// (ビートの回数, 誤差)
    /// </summary>
    public event Action<int, float> OnBeat;
    public int StartBeatOffset => musicData.StartBeatOffset;
    public float CurrentTime => (float)currentTime + musicData.Offset + RhythmGameManager.Offset;
    public float Bpm
    {
        get => bpm;
        private set
        {
            bpm = value;
            interval = 60d / bpm;
        }
    }

    void Start()
    {
        Bpm = musicData.Bpm;

        if(string.IsNullOrEmpty(musicData.SheetName))
        {
            criAtomSource.cueSheet = musicData.SheetName;
        }
        if(string.IsNullOrEmpty(musicData.CueName))
        {
            criAtomSource.cueSheet = musicData.CueName;
        }

        if(skipOnStart)
        {
            float skipTime = criAtomSource.GetLength() * timeRate;
            criAtomSource.startTime = Mathf.RoundToInt(skipTime * 1000f);
            currentTime = skipTime;
        }
        playback = criAtomSource.Play();
        addTime = autoStart;
        UpdateTimerAsync().Forget();
    }

    public void Stop()
    {
        addTime = false;
        playback.Stop();
    }

    async UniTask UpdateTimerAsync()
    {
        double baseTime = Time.timeAsDouble - currentTime;
        int beatCount = 0;
        double nextBeat = interval;
        while(addTime)
        {
            currentTime = Time.timeAsDouble - baseTime;
            if(CurrentTime > nextBeat)
            {
                if(musicData.TryGetBPMChangeBeatCount(bpmChangeCount, out int changeBeatCount) && beatCount == changeBeatCount)
                {
                    Bpm = musicData.GetChangeBPM(bpmChangeCount);
                    bpmChangeCount++;
                }
                OnBeat?.Invoke(beatCount, (float)(CurrentTime - nextBeat));
                
                beatCount++;
                nextBeat += interval;
            }
            await UniTask.Yield(PlayerLoopTiming.EarlyUpdate, destroyCancellationToken);
        }
    }
}