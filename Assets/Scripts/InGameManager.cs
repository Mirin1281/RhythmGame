using Cysharp.Threading.Tasks;
using NoteGenerating;
using UnityEngine;

public class InGameManager : MonoBehaviour
{
    [SerializeField] MusicMasterData masterData;
    [SerializeField] Metronome metronome;
    public MusicMasterData MasterData => masterData;
    public FumenData FumenData => masterData.GetFumenData();
    public MusicData MusicData => masterData.MusicData;

    async UniTask Awake()
    {
        if(RhythmGameManager.Instance != null && RhythmGameManager.Instance.MusicMasterData != null)
        {
            masterData = RhythmGameManager.Instance.MusicMasterData;
        }
        // ここにアークのテスト生成などを書く
        await MyUtility.WaitSeconds(0.1f, destroyCancellationToken);
        metronome.Play();
    }
}
