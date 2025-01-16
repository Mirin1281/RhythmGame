using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PaintController : MonoBehaviour
{
    [SerializeField] RawImage _image;
    [SerializeField] int _width = 10;
    [SerializeField] int _height = 10;
    [SerializeField] Color _lineColor = Color.black;
    [SerializeField] bool _randomColor = true;
    [SerializeField] Image _newImage;

    Texture2D _texture;
    Vector2 _prePos;
    Vector2 _touchPos;
    float _clickTime, _preClickTime;
    Vector2 _disImagePos;
    bool _isClear = true;
    Color _color;

    float StretchRate => (float)Screen.width / Screen.height * (ConstContainer.ScreenSize.y / ConstContainer.ScreenSize.x);

    void Awake()
    {
        Init();
    }

    public void Init()
    {
        var screenSize = new Vector2(Screen.width, Screen.height);
        var ts = _image.GetComponent<RectTransform>();
        //ts.anchoredPosition = screenSize / 2;
        //ts.sizeDelta = screenSize;
        var rect = ts.rect;
        _texture = new Texture2D((int)rect.width, (int)rect.height, TextureFormat.RGBA32, false);

        var canvasScaler = transform.parent.GetComponent<CanvasScaler>();
        canvasScaler.referenceResolution = screenSize;

        SetTextureColor(_texture, new Color32(255, 255, 255, 255));
        _image.texture = _texture;

        if (StretchRate > 1) // 横幅の方が長い場合
        {
            var snappedWidth = Screen.height * (ConstContainer.ScreenSize.x / ConstContainer.ScreenSize.y);
            _disImagePos = new Vector2((screenSize.x - snappedWidth) / 2f, 0);
        }
        else
        {
            var snappedHeight = Screen.width * (ConstContainer.ScreenSize.y / ConstContainer.ScreenSize.x);
            _disImagePos = new Vector2(0, (screenSize.y - snappedHeight) / 2f);
        }

        _color = _randomColor ? Color.HSVToRGB(Random.Range(0, 1f), 1, 0.85f) : _lineColor;
        _isClear = true;
        _newImage.gameObject.SetActive(false);


        static void SetTextureColor(Texture2D tex, Color32 color)
        {
            int length = tex.width * tex.height;
            Color32[] colors = new Color32[length];
            for (int i = 0; i < length; i++)
            {
                colors[i] = color;
            }
            tex.SetPixels32(0, 0, tex.width, tex.height, colors);
            tex.Apply();
        }
    }

    public void OnDrag(BaseEventData arg) // 線を描画
    {
        PointerEventData eventData = arg as PointerEventData;
        _touchPos = eventData.position - _disImagePos; // 現在のポインタの座標

        if (StretchRate > 1) // 横幅の方が長い場合
        {
            _touchPos.x = _touchPos.x * StretchRate;
        }
        else
        {
            _touchPos.y = _touchPos.y / StretchRate;
        }

        _clickTime = eventData.clickTime; // 最後にクリックイベントが送信された時間を取得
        float disTime = _clickTime - _preClickTime; // 前回のクリックイベントとの時差

        Vector2 dir = _prePos - _touchPos; // 直前のタッチ座標との差
        if (disTime > 0.01f)
            dir = Vector2.zero; // 0.1秒以上間隔があいたらタッチ座標の差を0にする

        int dist = (int)dir.magnitude; // タッチ座標ベクトルの絶対値
        dir = dir.normalized; // 正規化

        // 指定のペンの太さ(ピクセル)で、前回のタッチ座標から今回のタッチ座標まで塗りつぶす
        for (int i = 0; i < dist; i++)
        {
            var pos = _touchPos + dir * i; // paint position
            pos.y -= _height / 2f;
            pos.x -= _width / 2f;
            Draw(pos);
        }

        _texture.Apply();
        _prePos = _touchPos;
        _preClickTime = _clickTime;
    }

    public void OnTap(BaseEventData arg) // 点を描画
    {
        PointerEventData eventData = arg as PointerEventData; // タッチの情報取得
        _touchPos = eventData.position - _disImagePos; // 現在のポインタの座標

        if (StretchRate > 1) // 横幅の方が長い場合
        {
            _touchPos.x = _touchPos.x * StretchRate;
        }
        else
        {
            _touchPos.y = _touchPos.y / StretchRate;
        }

        var pos = _touchPos; // paint position
        pos.y -= _height / 2f;
        pos.x -= _width / 2f;
        Draw(pos);

        _texture.Apply();
        _prePos = _touchPos;
        _preClickTime = _clickTime;
        if (_isClear)
            _newImage.gameObject.SetActive(true);
        _isClear = false;
    }

    void Draw(Vector2 pos)
    {
        for (int i = 0; i < _height; i++)
        {
            int y = (int)(pos.y + i);
            if (y < 0 || y > _texture.height) continue; // タッチ座標がテクスチャの外の場合、描画処理を行わない

            for (int k = 0; k < _width; k++)
            {
                int x = (int)(pos.x + k);
                if (x >= 0 && x <= _texture.width)
                {
                    _texture.SetPixel(x, y, _color); // 線を描画
                }
            }
        }
    }
}
