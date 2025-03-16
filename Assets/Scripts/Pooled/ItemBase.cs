using Cysharp.Threading.Tasks;
using UnityEngine;

namespace NoteCreating
{
    /// <summary>
    /// インゲームで扱うプール可能なオブジェクトの基底クラス
    /// </summary>
    public abstract class ItemBase : MonoBehaviour, IPoolable
    {

        #region IPoolable

        bool _isActiveForPool;
        public bool IsActiveForPool => _isActiveForPool;
        public bool IsActive => gameObject.activeInHierarchy;

        void OnEnable()
        {
            _isActiveForPool = true;
        }

        // プール時に2フレーム開けるとバグらない
        async UniTaskVoid OnDisable()
        {
            await UniTask.DelayFrame(2, cancellationToken: destroyCancellationToken);
            _isActiveForPool = false;
        }

        public void SetActive(bool enabled)
        {
            gameObject.SetActive(enabled);
        }

        #endregion


        public virtual Vector3 GetPos(bool isWorld = false)
        {
            if (isWorld)
            {
                return transform.position;
            }
            else
            {
                return transform.localPosition;
            }
        }
        public virtual void SetPos(Vector3 pos, bool isWorld = false)
        {
            if (isWorld)
            {
                transform.position = pos;
            }
            else
            {
                transform.localPosition = pos;
            }
        }

        public virtual float GetRot(bool isWorld = false)
        {
            if (isWorld)
            {
                return transform.eulerAngles.z;
            }
            else
            {
                return transform.localEulerAngles.z;
            }
        }

        public virtual void SetRot(float deg, bool isWorld = false)
        {
            if (isWorld)
            {
                Vector3 angles = transform.eulerAngles;
                transform.eulerAngles = new Vector3(angles.x, angles.y, deg);
            }
            else
            {
                Vector3 angles = transform.localEulerAngles;
                transform.localEulerAngles = new Vector3(angles.x, angles.y, deg);
            }
        }

        public abstract void SetRendererEnabled(bool enabled);

        public abstract float GetAlpha();
        public abstract void SetAlpha(float alpha);

        public async UniTask FadeAlphaAsync(float endAlpha, float time, EaseType easeType = EaseType.OutQuad, float delta = 0)
        {
            var easing = new Easing(GetAlpha(), endAlpha, time, easeType);
            float baseTime = Metronome.Instance.CurrentTime - delta;
            while (true)
            {
                float t = Metronome.Instance.CurrentTime - baseTime;
                SetAlpha(easing.Ease(t));
                if (t >= time) break;
                await UniTask.Yield(destroyCancellationToken);
            }
            SetAlpha(endAlpha);
        }

        public void FadeIn(float time, float endAlpha, EaseType easeType = EaseType.OutQuad)
        {
            FadeInAsync(time, endAlpha, easeType);
        }
        public UniTask FadeInAsync(float time, float endAlpha, EaseType easeType = EaseType.OutQuad)
        {
            SetAlpha(0);
            return FadeAlphaAsync(endAlpha, time, easeType);
        }

        public void FadeOut(float time, EaseType easeType = EaseType.OutQuad)
        {
            FadeOutAsync(time, easeType).Forget();
        }
        public async UniTask FadeOutAsync(float time, EaseType easeType = EaseType.OutQuad)
        {
            await FadeAlphaAsync(0, time, easeType);
            SetActive(false);
        }
    }
}