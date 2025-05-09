using UnityEngine;
using Cysharp.Threading.Tasks;
using System;
using TMPro;
using DG.Tweening;

namespace NoteCreating
{
    [UnityEngine.Scripting.APIUpdating.MovedFrom(false, null, "Assembly-CSharp", null)]
    [AddTypeMenu(FumenPathContainer.SpecificRoot + "AprilRabbit/チュートリアルのテキスト"), System.Serializable]
    public class F_AprilRabbit_TutorialText : CommandBase
    {
        [SerializeField] Mirror mirror;
        [SerializeField] RubyTextMeshProUGUI tmproPrefab;
        [SerializeField, TextArea] string text;

        [SerializeField] bool isMove;
        [SerializeField] Vector3 pos;

        [SerializeField] bool isHide;
        [SerializeField] float time = 3;
        const string TmproName = "TutorialTMP";
        const string CanvasName = "Canvas";

        protected override async UniTaskVoid ExecuteAsync()
        {
            await Wait(MoveLpb);

            RubyTextMeshProUGUI tmpro;
            if (tmproPrefab != null)
            {
                tmpro = GameObject.Instantiate(tmproPrefab);
                tmpro.name = TmproName;
                tmpro.transform.SetParent(GameObject.Find(CanvasName).transform);
                tmpro.transform.localPosition = new Vector3(0, 150, 0);
                tmpro.transform.localScale = Vector3.one;
                tmpro.rectTransform.sizeDelta = new Vector2(700, 50);
            }
            else
            {
                var o = GameObject.Find(TmproName);
                if (o == null)
                {
                    Debug.Log("TMPが設定されていません");
                    return;
                }
                tmpro = o.GetComponent<RubyTextMeshProUGUI>();
            }

            Fade(tmpro, 1f, 0f);
            if (isMove)
            {
                tmpro.transform.DOLocalMove(mirror.Conv(pos), 0.3f).SetRelative(true);
            }
            tmpro.uneditedText = text;

            if (isHide)
            {
                await WaitSeconds(time);
                Fade(tmpro, 0);
            }

        }

        void Fade(TMP_Text tmpro, float alpha, float? from = null)
        {
            var easing = new Easing(from ?? tmpro.color.a, alpha, 0.3f, EaseType.OutQuad);
            WhileYield(0.3f, t =>
            {
                tmpro.color = new Color(0, 0, 0, easing.Ease(t));
            });
        }

#if UNITY_EDITOR

        protected override string GetName()
        {
            return "Text";
        }
#endif
    }
}