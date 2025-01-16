using Cysharp.Threading.Tasks;
using UnityEngine;

namespace NoteCreating
{
    public class Line : ItemBase
    {
        [SerializeField] SpriteRenderer spriteRenderer;
        public void SetWidth(float width)
        {
            transform.localScale = new Vector3(width, transform.localScale.y);
        }
        public void SetHeight(float height)
        {
            transform.localScale = new Vector3(transform.localScale.x, height);
        }

        public override void SetRendererEnabled(bool enabled)
        {
            spriteRenderer.enabled = enabled;
        }

        public override float GetAlpha()
        {
            return spriteRenderer.color.a;
        }
        public override void SetAlpha(float alpha)
        {
            var c = spriteRenderer.color;
            spriteRenderer.color = new Color(c.r, c.g, c.b, alpha);
        }

        /*public async UniTask MoveAsync(Vector3 toPos, float time, float delta = 0)
        {
            var xEasing = new Easing(GetPos().x, toPos.x, time, EaseType.OutQuad);
            var yEasing = new Easing(GetPos().y, toPos.y, time, EaseType.OutQuad);
            var zEasing = new Easing(GetPos().z, toPos.z, time, EaseType.OutQuad);

            float t = delta;
            while(t < time)
            {
                t += Time.deltaTime;
                SetPos(new Vector3(xEasing.Ease(t), yEasing.Ease(t), zEasing.Ease(t)));
                await UniTask.Yield(destroyCancellationToken);
            }
        }
        public async UniTask RotateAsync(float toRot, float time, float delta = 0)
        {
            var easing = new Easing(transform.eulerAngles.z, toRot, time, EaseType.OutQuad);

            float t = delta;
            while(t < time)
            {
                t += Time.deltaTime;
                SetRotate(easing.Ease(t));
                await UniTask.Yield(destroyCancellationToken);
            }
        }*/
    }
}