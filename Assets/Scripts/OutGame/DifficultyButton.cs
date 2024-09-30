using System.Threading;
using Cysharp.Threading.Tasks;
using NoteGenerating;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DifficultyButton : MonoBehaviour//, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField] Image image;
    [SerializeField] TMP_Text difficultyTmpro;
    [SerializeField] TMP_Text levelTmpro;
    //[SerializeField] float holdTime = 0.7f;
    Difficulty difficulty;
    //CancellationTokenSource _cts;

    public void Init(Difficulty difficulty)
    {
        this.difficulty = difficulty;
        
        string diffName = difficulty switch
        {
            Difficulty.Normal => "NR",
            Difficulty.Hard => "HD",
            Difficulty.Extra => "EX",
            _ => throw new System.Exception()
        };
        difficultyTmpro.SetText(diffName);
    }

    public void SetLevel(int level)
    {
        levelTmpro.SetText(level.ToString());
    }

    public void OnSelect()
    {
        var group = transform.parent.GetComponent<DifficultyGroup>();
        group.NotifyByChild(difficulty);
        difficultyTmpro.color = Color.white;
        levelTmpro.color = Color.white;
        image.color = new Color32(0, 0, 0, 230);
    }

    public void Deselect()
    {
        difficultyTmpro.color = Color.black;
        levelTmpro.color = Color.black;
        image.color = Color.clear;
    }

    /*void IPointerDownHandler.OnPointerDown(PointerEventData eventData)
    {
        _cts = new();
        HoldAsync(_cts.Token).Forget();
    }

    void IPointerUpHandler.OnPointerUp(PointerEventData eventData)
    {
        _cts?.Cancel();
    }

    async UniTask HoldAsync(CancellationToken token)
    {
        float downTime = 0f;
        while(!token.IsCancellationRequested)
        {
            if(downTime > holdTime)
            {
                Debug.Log(0);
            }
            downTime += Time.deltaTime;
            await UniTask.Yield(token);
        }
    }*/
}
