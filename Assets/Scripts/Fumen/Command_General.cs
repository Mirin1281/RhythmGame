using Cysharp.Threading.Tasks;
using UnityEngine;

namespace NoteCreating
{
    public abstract class Command_General : CommandBase, IMirrorable
    {
        [SerializeField] bool isMirror;
        public bool IsMirror => isMirror ^ RhythmGameManager.SettingIsMirror;

        void IMirrorable.SetIsMirror(bool i)
        {
            isMirror = i;
        }

        /// <summary>
        /// IsInverseがtrueの時、-1倍して返します
        /// </summary>s
        protected float Inv(float x) => x * (IsMirror ? -1 : 1);
        protected int Inv(int x) => x * (IsMirror ? -1 : 1);
        protected Vector3 Inv(Vector3 pos) => new Vector3(Inv(pos.x), pos.y);
        protected Vector2 Inv(Vector2 pos) => new Vector2(Inv(pos.x), pos.y);

        protected virtual float Speed => RhythmGameManager.Speed;

        /// <summary>
        /// ノーツの初期生成地点
        /// </summary>
        protected float StartBase => 359.3f * Speed / Helper.Metronome.Bpm;
        //protected float StartBase => (2f * Speed + 0.2f) * 177f / Helper.Metronome.Bpm;


        /// <summary>
        /// Normal, Slide, Flickノーツを生成します
        /// </summary>
        /// <param name="x">着弾地点のX座標</param>
        /// <param name="type">Normal, Slide, Flickのどれかを指定</param>
        /// <param name="delta">生成時間の誤差情報(通常は無視してOK)</param>
        /// <param name="parentTs">高度な移動をする際の親オブジェクト</param>
        protected RegularNote Note(float x, RegularNoteType type, float delta = -1, bool isMove = true, Transform parentTs = null)
        {
            if (delta == -1)
            {
                delta = Delta;
            }
            RegularNote note = Helper.GetNote(type, parentTs);
            Vector3 startPos = new(Inv(x), StartBase, 0);
            if (isMove)
            {
                DropAsync(note, startPos, delta).Forget();
            }
            else
            {
                note.SetPos(startPos);
            }

            // 現在の時間から何秒後に着弾するか
            float expectTime = startPos.y / Speed - delta;
            if (parentTs == null)
            {
                Helper.NoteInput.AddExpect(note, expectTime);
            }
            else
            {
                float parentDir = parentTs.transform.eulerAngles.z * Mathf.Deg2Rad;
                Vector3 pos = x * new Vector3(Mathf.Cos(parentDir), Mathf.Sin(parentDir));
                Helper.NoteInput.AddExpect(note, new Vector2(default, pos.y), expectTime, expectType: NoteJudgeStatus.ExpectType.Y_Static);
            }
            return note;
        }

        protected HoldNote Hold(float x, float length, float delta = -1, bool isMove = true, Transform parentTs = null)
        {
            if (delta == -1)
            {
                delta = Delta;
            }
            float holdTime = Helper.GetTimeInterval(length);
            HoldNote hold = Helper.GetHold(holdTime * Speed, parentTs);
            Vector3 startPos = new(Inv(x), StartBase, -0.04f);
            hold.SetMaskLocalPos(new Vector2(startPos.x, 0));
            if (isMove)
            {
                DropAsync(hold, startPos, delta).Forget();
            }
            else
            {
                hold.SetPos(startPos);
            }

            float expectTime = startPos.y / Speed - delta;
            if (parentTs == null)
            {
                Helper.NoteInput.AddExpect(hold, expectTime, holdTime);
            }
            else
            {
                float parentDir = parentTs.transform.eulerAngles.z * Mathf.Deg2Rad;
                Vector3 pos = x * new Vector3(Mathf.Cos(parentDir), Mathf.Sin(parentDir));
                Helper.NoteInput.AddExpect(hold, new Vector2(default, pos.y), expectTime, holdTime, expectType: NoteJudgeStatus.ExpectType.Y_Static);
            }
            return hold;
        }

        protected ArcNote Arc(ArcCreateData[] datas, float delta = -1)
        {
            if (delta == -1)
            {
                delta = Delta;
            }
            ArcNote arc = Helper.GetArc();
            arc.CreateNewArcAsync(datas, Helper.GetTimeInterval(1) * Speed, IsMirror).Forget();

            Vector3 startPos = new Vector3(0, StartBase);
            DropAsync(arc, startPos, delta).Forget();
            Helper.NoteInput.AddArc(arc);
            return arc;
        }

        protected async UniTask<float> LoopNote(float lpb, RegularNoteType type, params float?[] nullableXs)
        {
            foreach (var nullableX in nullableXs)
            {
                if (nullableX is float x)
                {
                    Note(x, type, Delta);
                }
                await Wait(lpb);
            }
            return Delta;
        }


        protected async UniTask DropAsync(ItemBase note, Vector3 startPos, float delta = -1)
        {
            if (delta == -1)
            {
                delta = Delta;
            }
            float baseTime = CurrentTime - delta;
            float time = 0f;
            var vec = Speed * Vector3.down;
            while (note.IsActive && time < 8f)
            {
                time = CurrentTime - baseTime;
                note.SetPos(startPos + time * vec);
                await Helper.Yield();
            }
        }

        /// <summary>
        /// Parentを生成します
        /// </summary>
        protected Transform CreateParent(IParentCreatable parentCreatable)
        {
            return parentCreatable?.CreateParent(Delta, Helper, IsMirror);
        }

        /// <summary>
        /// 反転がtrueの際にテキストを追加します
        /// </summary>
        /// <returns></returns>
        protected string GetMirrorSummary()
        {
            if (isMirror)
            {
                return "  <color=#0000ff><b>(mir)</b></color>";
            }
            else
            {
                return string.Empty;
            }
        }
    }
}