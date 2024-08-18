using UnityEngine;
using Cysharp.Threading.Tasks;
using System;

public class Metronome : MonoBehaviour
{
    [SerializeField] AudioSource audioSource;
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

    public bool AddTime
    {
        set
        {
            addTime = value;
            if (value == false) return;
            UpdateTimerAsync().Forget();
        }
    }

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
        audioSource.clip = musicData.MusicClip;

        if(skipOnStart)
        {
            float skipTime = audioSource.clip.length * timeRate;
            audioSource.time = skipTime;
            currentTime = skipTime;
        }
        audioSource.Play();
        AddTime = autoStart;
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