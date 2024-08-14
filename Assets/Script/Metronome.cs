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
    
    double currentTime;
    public float CurrentTime => (float)currentTime + musicData.Offset + RhythmGameManager.Offset;

    bool addTime;
    public bool AddTime
    {
        set
        {
            addTime = value;
            if (value == false) return;
            UpdateTimerAsync().Forget();
        }
    }

    void Start()
    {
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

    /// <summary>
    /// (ビートの回数, 誤差)
    /// </summary>
    public event Action<int, float> OnBeat;
    public float Bpm => musicData.Bpm;
    double interval => 60d / Bpm;

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
                OnBeat?.Invoke(beatCount, (float)(CurrentTime - nextBeat));
                beatCount++;
                nextBeat += interval;
            }
            await UniTask.Yield(PlayerLoopTiming.EarlyUpdate, destroyCancellationToken);
        }
    }
}