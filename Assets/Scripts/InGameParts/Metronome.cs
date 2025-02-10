using UnityEngine;
using Cysharp.Threading.Tasks;
using System;
using CriWare;
using NoteCreating;

#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// オーディオと全体のタイミングを管理するシングルトン
/// </summary>
public class Metronome : SingletonMonoBehaviour<Metronome>, IVolumeChangable
{
    [SerializeField] CriAtomSource atomSource;
    [Space(20)]
    [SerializeField] bool skipOnStart;
    [SerializeField, Range(0, 1)] float timeRate;

#if UNITY_EDITOR
    static readonly int skipExecuteTolerance = 10; // 途中再生時、値x4分音符前のコマンドを発火します
#endif

    FumenData fumenData;
    float bpm;
    bool isLooping;
    double currentTime;
    int beatCount;
    CriAtomExPlayback playback;
    int bpmChangeCount;
    double BeatInterval => 60d / bpm;
    MusicSelectData SelectData => fumenData.MusicSelectData;

    /// <summary>
    /// 引数は(ビートの回数, 誤差)
    /// </summary>
    public event Action<int, float> OnBeat;
    public float CurrentTime
    {
        get
        {
            if (fumenData == null) return (float)currentTime + RhythmGameManager.Offset;
            return (float)currentTime + SelectData.Offset + RhythmGameManager.Offset;
        }
    }
    public float Bpm => bpm;

    public void Play(FumenData fumenData)
    {
        this.fumenData = fumenData;
        bpm = SelectData.Bpm;
        atomSource.cueSheet = atomSource.cueName = SelectData.SheetName;

#if UNITY_EDITOR
        EditorApplication.pauseStateChanged += SwitchMusic;
        if (skipOnStart)
        {
            float skipTime = atomSource.GetLength() * timeRate;
            currentTime = skipTime;
            atomSource.startTime = Mathf.RoundToInt(skipTime * 1000f);

            int culcedBeatCount = Mathf.RoundToInt(skipTime / (float)BeatInterval);
            beatCount = Mathf.Clamp(culcedBeatCount - skipExecuteTolerance, 0, int.MaxValue);

            foreach (var commandData in fumenData.Fumen.GetReadOnlyCommandDataList())
            {
                if (culcedBeatCount >= commandData.BeatTiming + skipExecuteTolerance && commandData.GetCommandBase() is INotSkipCommand)
                {
                    float delta = skipTime - (commandData.BeatTiming + skipExecuteTolerance + SelectData.StartBeatOffset - 8) * (float)BeatInterval;
                    commandData.Execute(GameObject.FindAnyObjectByType<NoteCreateHelper>(), delta);
                }
            }
        }
#endif

        playback = atomSource.Play();
        UpdateTimerAsync(beatCount).Forget();
    }

    protected override void OnDestroy()
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
        while (isLooping)
        {
            currentTime = Time.timeAsDouble - baseTime;
            if (CurrentTime > nextBeat)
            {
                int offsetedBeatCount = beatCount - SelectData.StartBeatOffset;

                // BPMの変化がある場合 //
                if (SelectData.TryGetBPMChangeBeatCount(bpmChangeCount, out int changeBeatCount))
                {
                    if (offsetedBeatCount == changeBeatCount)
                    {
                        bpm = SelectData.GetChangeBPM(bpmChangeCount);
                        bpmChangeCount++;
                    }
                }

                OnBeat?.Invoke(offsetedBeatCount, (float)(CurrentTime - nextBeat));
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

    void IVolumeChangable.ChangeVolume(float value)
    {
        atomSource.volume = value;
    }

#if UNITY_EDITOR
    void SwitchMusic(PauseState state)
    {
        if (state == PauseState.Paused)
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