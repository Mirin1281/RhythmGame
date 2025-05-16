using TMPro;
using UnityEngine;
using Cysharp.Threading.Tasks;

namespace NoteCreating
{
    public enum NoteGrade
    {
        None,
        Perfect,
        FastGreat,
        LateGreat,
        FastFar,
        LateFar,
        Miss,
    }

    public class Judgement : MonoBehaviour
    {
        [SerializeField] TMP_Text comboText;
        [SerializeField] TMP_Text deltaText;
        [SerializeField] TMP_Text judgeText;
        [SerializeField] TMP_Text scoreText;
        [SerializeField] TMP_Text fullComboStatusText;

        [SerializeField] ParticlePool particlePool;
#if UNITY_EDITOR
        bool showDebugRange;
        [SerializeField] GameObject debugNoteRangePrefab;
#else
        bool showDebugRange = false;
#endif
        public bool ShowDebugRange { set => showDebugRange = value; }

        Result result;
        float showScore;
        bool isFullCombo = true;
        bool isAllPerfect = true;
        int judgeRequestId = 0;

        public Result Result => result;
        const float Range = 5.4f;
        const float ArcRange = 4f;

        public void Init(FumenData fumenData)
        {
            result = new Result(fumenData);
        }
        void Awake()
        {
            comboText.SetText("0");
            judgeText.SetText("");
        }
        void OnDestroy()
        {
            result = null;
        }


        public bool IsNearPosition(NoteJudgeStatus judgeStatus, Vector2 inputPos)
        {
            float width = judgeStatus.Note == null ? 1 : judgeStatus.Note.Width;
            float height = judgeStatus.IsVerticalRange ? 10f : 1.5f;
            return MyUtility.IsPointInsideRectangle(
                new Rect(judgeStatus.Pos, Range * new Vector2(width, height)),
                inputPos,
                judgeStatus.Rot);
        }
        public bool IsNearPositionHold(HoldNote hold, Vector2 inputPos)
        {
            return MyUtility.IsPointInsideRectangle(
                new Rect(hold.GetLandingPos(), Range * new Vector2(hold.Width, hold.IsVerticalRange ? 10f : 1.5f)),
                inputPos,
                hold.GetRot());
        }
        public bool IsNearPositionArc(Vector2 pos1, Vector2 pos2, float range = ArcRange)
        {
            var distance = Vector2.Distance(pos1, pos2);
            return distance < range / 2f;
        }

        public void PlayParticle(NoteGrade grade, Vector2 pos)
        {
            particlePool.PlayParticle(pos, grade);
        }

        public void SetCombo(NoteGrade grade)
        {
            /*if (grade == NoteGrade.Miss)
            {
                Debug.Log(0);
            }*/
            int beforeScore = result.Score;
            result.SetComboAndScore(grade);
            comboText.SetText("{0}", result.Combo);
            SetScoreTextAsync(beforeScore, result.Score).Forget();

            if (isAllPerfect && grade != NoteGrade.Perfect)
            {
                isAllPerfect = false;
                fullComboStatusText.SetText("[F]");
            }
            if (isFullCombo && grade == NoteGrade.Miss)
            {
                isFullCombo = false;
                fullComboStatusText.gameObject.SetActive(false);
            }


            async UniTaskVoid SetScoreTextAsync(float beforeScore, float toScore)
            {
                float baseTime = Metronome.Instance.CurrentTime;
                float t = 0;
                var easing = new Easing(beforeScore, toScore, 0.3f, EaseType.OutQuad);
                while (t < 0.3f)
                {
                    t = Metronome.Instance.CurrentTime - baseTime;
                    var s = easing.Ease(t);
                    if (showScore < s)
                    {
                        showScore = s;
                        scoreText.SetText("{0:00000000}", s);
                    }

                    await UniTask.Yield(destroyCancellationToken);
                }
            }
        }

        public NoteGrade GetGradeAndSetText(float delta)
        {
            var grade = GetGrade(delta);
            if (grade != NoteGrade.Perfect)
                SetJudgeText(grade).Forget();
            else if (grade != NoteGrade.Miss)
                result.AddTotalDelta(delta);

            int miliDelta = Mathf.RoundToInt(delta * 1000f);
            if (miliDelta > 0)
            {
                deltaText.SetText("+{0}", miliDelta);
            }
            else
            {
                deltaText.SetText("{0}", miliDelta);
            }
            return grade;


            /*async UniTask SetJudgeText(NoteGrade grade)
            {
                cts.Cancel();
                cts = new();
                CancellationToken token = cts.Token;

                judgeText.SetText(grade.ToString());
                await MyUtility.WaitSeconds(1f, token);
                judgeText.SetText(string.Empty);
            }*/

            async UniTask SetJudgeText(NoteGrade grade)
            {
                judgeRequestId++;
                int currentId = judgeRequestId;

                string judgeStr = grade switch
                {
                    NoteGrade.FastGreat => "<voffset=-0.15em><size=140%>-</size></voffset> Great",
                    NoteGrade.LateGreat => "<voffset=-0.15em><size=140%>+</size></voffset> Great",
                    NoteGrade.FastFar => "<voffset=-0.15em><size=140%>-</size></voffset> Far",
                    NoteGrade.LateFar => "<voffset=-0.15em><size=140%>+</size></voffset> Far",
                    _ => string.Empty
                };

                judgeText.SetText(judgeStr);
                await MyUtility.WaitSeconds(1f, destroyCancellationToken);
                if (currentId != judgeRequestId) return;
                judgeText.SetText(string.Empty);
            }
        }

        public static NoteGrade GetGrade(float delta)
        {
            if (Mathf.Abs(delta) < 0.06f)
            {
                return NoteGrade.Perfect;
            }
            else if (Mathf.Abs(delta) < 0.10f)
            {
                if (delta > 0)
                {
                    return NoteGrade.LateGreat;
                }
                else
                {
                    return NoteGrade.FastGreat;
                }
            }
            else if (Mathf.Abs(delta) < 0.15f)
            {
                if (delta > 0)
                {
                    return NoteGrade.LateFar;
                }
                else
                {
                    return NoteGrade.FastFar;
                }
            }
            else
            {
                return NoteGrade.Miss;
            }
        }

#if UNITY_EDITOR
        public void DebugShowRange(NoteJudgeStatus judgeStatus)
        {
            if (showDebugRange == false) return;
            var obj = Instantiate(debugNoteRangePrefab, transform);
            obj.transform.localPosition = judgeStatus.Pos;
            obj.transform.localRotation = Quaternion.AngleAxis(judgeStatus.Rot, Vector3.forward);
            obj.transform.localScale = Range * new Vector3(
                judgeStatus.Note == null ? 1 : judgeStatus.Note.Width, judgeStatus.IsVerticalRange ? 10f : 1.5f);
            UniTask.Void(async () =>
            {
                await MyUtility.WaitSeconds(0.15f, destroyCancellationToken);
                Destroy(obj);
            });
        }
#endif
    }
}