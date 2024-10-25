using Cysharp.Threading.Tasks;
using UnityEngine;

namespace NoteGenerating
{
    public abstract class Generator_Common : NoteGeneratorBase
    {
        [SerializeField] bool isInverse;
        protected bool IsInverse => isInverse ^ RhythmGameManager.SettingIsMirror;
        
        /// <summary>
        /// IsInverseがtrueの時、-1倍して返します
        /// </summary>
        protected float Inv(float x) => x * (IsInverse ? -1 : 1);

        protected virtual float Speed => RhythmGameManager.Speed;
        protected virtual float Speed3D => RhythmGameManager.Speed3D;

        /// <summary>
        /// ノーツの初期生成地点
        /// </summary>
        protected float StartBase => (2f * Speed + 0.2f) * 177f / Helper.Metronome.Bpm;
        protected float StartBase3D => (2f * Speed3D + 0.2f) * 177f / Helper.Metronome.Bpm;


        #region Notes

        /// <summary>
        /// Normal, Slide, Flickノーツを生成します
        /// </summary>
        /// <param name="x">着弾地点のX座標</param>
        /// <param name="type">Normal, Slide, Flickのどれかを指定</param>
        /// <param name="delta">生成時間の誤差情報(通常は無視してOK)</param>
        /// <param name="parentTs">高度な移動をする際の親オブジェクト</param>
        protected NoteBase_2D Note2D(float x, NoteType type, float delta = -1, bool isMove = true, Transform parentTs = null)
        {
            if(delta == -1)
            {
                delta = Delta;
            }
            NoteBase_2D note = Helper.GetNote2D(type, parentTs);
            Vector3 startPos = new (Inv(x), StartBase, -0.04f);
            if(isMove)
            {
                DropAsync(note, startPos, delta).Forget();
            }
            else
            {
                note.SetPos(startPos);
            }

            // 現在の時間から何秒後に着弾するか
            float expectTime = startPos.y/Speed - delta;
            if(parentTs == null)
            {
                Helper.NoteInput.AddExpect(note, expectTime);
            }
            else
            {
                float parentDir = parentTs.transform.eulerAngles.z * Mathf.Deg2Rad;
                Vector3 pos = x * new Vector3(Mathf.Cos(parentDir), Mathf.Sin(parentDir));
                Helper.NoteInput.AddExpect(note, new Vector2(default, pos.y), expectTime, mode: NoteExpect.ExpectMode.Y_Static);
            }
            return note;
        }
        protected HoldNote Hold(float x, float length, float delta = -1, bool isMove = true, Transform parentTs = null)
        {
            if(delta == -1)
            {
                delta = Delta;
            }
            float holdTime = Helper.GetTimeInterval(length);
            HoldNote hold = Helper.GetHold(holdTime * Speed, parentTs);
            Vector3 startPos = new (Inv(x), StartBase, -0.04f);
            hold.SetMaskLocalPos(new Vector2(startPos.x, 0));
            if(isMove)
            {
                DropAsync(hold, startPos, delta).Forget();
            }
            else
            {
                hold.SetPos(startPos);
            }

            float expectTime = startPos.y/Speed - delta;
            if(parentTs == null)
            {
                Helper.NoteInput.AddExpect(hold, expectTime, holdTime);
            }
            else
            {
                float parentDir = parentTs.transform.eulerAngles.z * Mathf.Deg2Rad;
                Vector3 pos = x * new Vector3(Mathf.Cos(parentDir), Mathf.Sin(parentDir));
                Helper.NoteInput.AddExpect(hold, new Vector2(default, pos.y), expectTime, holdTime, mode: NoteExpect.ExpectMode.Y_Static);
            }
            return hold;
        }

        protected SkyNote Sky(Vector2 pos, float delta = -1, bool isMove = true)
        {
            if(delta == -1)
            {
                delta = Delta;
            }
            SkyNote sky = Helper.GetSky();
            var startPos = new Vector3(Inv(pos.x), pos.y, StartBase3D);
            if(isMove)
            {
                DropAsync3D(sky, startPos, delta).Forget();
            }
            else
            {
                sky.SetPos(startPos);
            }

            float expectTime = startPos.z/Speed3D - delta;
            Helper.NoteInput.AddExpect(sky, startPos, expectTime);
            return sky;
        }

        protected ArcNote Arc(ArcCreateData[] datas, bool is2D = false, float delta = -1)
        {
            if(delta == -1)
            {
                delta = Delta;
            }
            ArcNote arc = Helper.GetArc();
            if(is2D)
            {
                arc.SetRadius(0.4f);
                arc.Is2D = true;
                arc.transform.localRotation = Quaternion.Euler(-90f, 0f, 0f);
            }
            arc.CreateNewArcAsync(datas, Helper.GetTimeInterval(1) * (is2D ? Speed : Speed3D), IsInverse).Forget();

            Vector3 startPos;
            if(is2D)
            {
                startPos = new Vector3(0, StartBase);
                DropAsync(arc, startPos, delta).Forget();
            }
            else
            {
                startPos = new Vector3(0, 0f, StartBase3D);
                DropAsync3D(arc, startPos, delta).Forget();
            }
            Helper.NoteInput.AddArc(arc);
            return arc;
        }

        protected async UniTask<float> LoopNote(float lpb, NoteType type, params float?[] nullableXs)
        {
            foreach(var nullableX in nullableXs)
            {
                if(nullableX is float x)
                {
                    Note2D(x, type, Delta);
                }
                await Wait(lpb);
            }
            return Delta;
        }

        #endregion

        protected async UniTask DropAsync(NoteBase note, Vector3 startPos, float delta = -1)
        {
            if(delta == -1)
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
        protected async UniTask DropAsync3D(NoteBase note, Vector3 startPos, float delta = -1)
        {
            if(delta == -1)
            {
                delta = Delta;
            }
            float baseTime = CurrentTime - delta;
            float time = 0f;
            var vec = Speed3D * Vector3.back;
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
        protected Transform CreateParent(IParentGeneratable parentGeneratable)
        {
            return parentGeneratable?.GenerateParent(Delta, Helper, IsInverse);
        }

        /// <summary>
        /// 反転がtrueの際にテキストを追加します
        /// </summary>
        /// <returns></returns>
        protected string GetInverseSummary()
        {
            if(isInverse)
            {
                return "  <color=#0000ff><b>(inv)</b></color>";
            }
            else
            {
                return string.Empty;
            }
        }

        public override string CSVContent4
        {
            get => IsInverse.ToString();
            set => isInverse = bool.Parse(value);
        }
    }
}