using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine;

namespace Novel
{
    public abstract class FadableMonoBehaviour : MonoBehaviour
    {
        protected abstract void SetAlpha(float a);
        protected abstract float GetAlpha();

        /// <summary>
        /// �A�N�e�B�u�ɂ��Ă���t�F�[�h�C�����܂�
        /// </summary>
        public async UniTask ShowFadeAsync(float time = ConstContainer.DefaultFadeTime, CancellationToken token = default)
        {
            gameObject.SetActive(true);
            await FadeAlphaAsync(0f, 1f, time, token);
        }

        /// <summary>
        /// �t�F�[�h�A�E�g���Ă����A�N�e�B�u�ɂ��܂�
        /// </summary>
        public async UniTask ClearFadeAsync(float time = ConstContainer.DefaultFadeTime, CancellationToken token = default)
        {
            await FadeAlphaAsync(0f, time, token);
            gameObject.SetActive(false);
        }

        /// <summary>
        /// �w�肵�������x�܂ŘA���I�ɕω������܂�
        /// </summary>
        async UniTask FadeAlphaAsync(float startAlpha, float toAlpha, float time, CancellationToken token)
        {
            var t = 0f;
            while (t < time)
            {
                SetAlpha(t.Ease(startAlpha, toAlpha, time, EaseType.OutQuad));
                t += Time.deltaTime;
                await UniTask.Yield(token == default ? this.GetCancellationTokenOnDestroy() : token);
            }
            SetAlpha(toAlpha);
        }
        /// <summary>
        /// �w�肵�������x�܂ŘA���I�ɕω������܂�
        /// </summary>
        UniTask FadeAlphaAsync(float toAlpha, float time, CancellationToken token)
        {
            return FadeAlphaAsync(GetAlpha(), toAlpha, time, token);
        }
    }
}