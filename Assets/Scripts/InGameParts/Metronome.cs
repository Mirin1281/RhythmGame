using UnityEngine;
using Cysharp.Threading.Tasks;
using System;
using CriWare;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class Metronome : MonoBehaviour, IVolumeChanable
{
    [SerializeField] InGameManager inGameManager;
    [SerializeField] CriAtomSource atomSource;
    [Space(10)]
    [SerializeField] bool skipOnStart;
    [SerializeField, Range(0, 1)] float timeRate;
    readonly int tolerance = 10;

    float bpm;
    bool isLooping;
    double currentTime;
    int beatCount;
    
    CriAtomExPlayback playback;
    int bpmChangeCount;
    double BeatInterval => 60d / bpm;
    MusicData MusicData => inGameManager.MusicData;

    /// <summary>
    /// (ビートの回数, 誤差)
    /// </summary>
    public event Action<int, float> OnBeat;
    public float CurrentTime => (float)currentTime + MusicData.Offset + RhythmGameManager.Offset;
    public float Bpm => bpm;

    public void Play()
    {
#if UNITY_EDITOR
        EditorApplication.pauseStateChanged += SwitchMusic;
#else
        skipOnStart = false;
#endif

        bpm = MusicData.Bpm;
        atomSource.cueSheet = MusicData.SheetName;
        atomSource.cueName = MusicData.CueName;
        
        if(skipOnStart)
        {
            float skipTime = atomSource.GetLength() * timeRate;
            atomSource.startTime = Mathf.RoundToInt(skipTime * 1000f);
            currentTime = skipTime;
            int b = Mathf.RoundToInt(skipTime / (float)BeatInterval);
            beatCount = Mathf.Clamp(b - tolerance, 0, int.MaxValue);
            
            foreach(var generateData in inGameManager.FumenData.Fumen.GetReadOnlyGenerateDataList())
            {
                if(b >= generateData.BeatTiming + tolerance && generateData.GetNoteGeneratorBase() is IZoneCommand zone)
                {
                    zone.CallZone(skipTime - (generateData.BeatTiming + tolerance) * (float)BeatInterval);
                }
            }
        }

        playback = atomSource.Play();
        UpdateTimerAsync(beatCount).Forget();
    }

    void OnDestroy()
    {
        Stop();
        OnBeat = null;
#if UNITY_EDITOR
        EditorApplication.pauseStateChanged -= SwitchMusic;
#endif
    }

    async UniTask UpdateTimerAsync(int startBeatCount = 0)
    {
        beatCount = startBeatCount;
        isLooping = true;
        double baseTime = Time.timeAsDouble - currentTime;
        double nextBeat = BeatInterval * (beatCount + 1);
        while(isLooping)
        {
            currentTime = Time.timeAsDouble - baseTime;
            if(CurrentTime > nextBeat)
            {
                if(MusicData.TryGetBPMChangeBeatCount(bpmChangeCount, out int changeBeatCount) && beatCount == changeBeatCount)
                {
                    bpm = MusicData.GetChangeBPM(bpmChangeCount);
                    bpmChangeCount++;
                }
                OnBeat?.Invoke(beatCount - MusicData.StartBeatOffset, (float)(CurrentTime - nextBeat));
                
                beatCount++;
                nextBeat += BeatInterval;
            }
            await UniTask.Yield(PlayerLoopTiming.EarlyUpdate, destroyCancellationToken);
        }
    }

    void Stop()
    {
        isLooping = false;
        playback.Stop();
    }
    public void Pause()
    {
        isLooping = false;
        playback.Pause();
    }
    public void Resume()
    {
        playback.Resume(CriAtomEx.ResumeMode.PausedPlayback);
        currentTime += 0.01f;
        UpdateTimerAsync(beatCount).Forget();
    }

    void IVolumeChanable.ChangeVolume(float value)
    {
        atomSource.volume = value;
    }
    
#if UNITY_EDITOR
    void SwitchMusic(PauseState state)
    {
        if(state == PauseState.Paused)
        {
            playback.Pause();
        }
        else
        {
            playback.Resume(CriAtomEx.ResumeMode.PausedPlayback);
        }
    }
#endif
}