using System;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using CriWare;
using Cysharp.Threading.Tasks;
using UnityEngine;

public static class MyUtility
{
    public static UniTask WaitSeconds(float time, CancellationToken token)
    {
        if(time <= 0) return UniTask.CompletedTask;
        return UniTask.Delay(TimeSpan.FromSeconds(time), cancellationToken: token);
    }

    /// <summary>
    /// 入力された座標が任意の四角形の中に入っているかを判定します
    /// </summary>
    /// <param name="rect">四角形</param>
    /// <param name="inputPos">入力座標</param>
    /// <param name="rotate">回転(ラジアン)</param>
    /// <returns></returns>
    public static bool IsPointInsideRectangle(Rect rect, Vector2 inputPos, float rotate = 0)
    {
        if(rotate == 0)
        {
            float dx = inputPos.x - rect.x;
            float dy = inputPos.y - rect.y;
            return Mathf.Abs(dx) <= rect.width / 2 && Mathf.Abs(dy) <= rect.height / 2;
        }
        else
        {
            // 点を四角形の中心に移動
            float dx = inputPos.x - rect.x;
            float dy = inputPos.y - rect.y;

            // 逆回転させる（四角形がrだけ回転しているので、点を-rだけ回転させる）
            float cosR = Mathf.Cos(-rotate);
            float sinR = Mathf.Sin(-rotate);

            // 回転行列を適用
            float rotatedX = dx * cosR - dy * sinR;
            float rotatedY = dx * sinR + dy * cosR;

            // 四角形の範囲内かどうかを確認（長辺はw、短辺はhの中心対称）
            return Mathf.Abs(rotatedX) <= rect.width / 2 && Mathf.Abs(rotatedY) <= rect.height / 2;
        }
    }

    public static Vector2 GetRotatedPos(Vector2 pos, float deg, Vector2 centerPos = default)
    {
        var cos = Mathf.Cos(deg * Mathf.Deg2Rad);
        var sin = Mathf.Sin(deg * Mathf.Deg2Rad);
        return new Vector2(
            centerPos.x + (pos.x - centerPos.x) * cos - (pos.y - centerPos.y) * sin,
            centerPos.y + (pos.x - centerPos.x) * sin + (pos.y - centerPos.y) * cos);
    }

    public static UniTask LoadCueSheetAsync(string cueSheetName, string acbName = null)
    {
        CriAtom.AddCueSheetAsync(cueSheetName, (acbName ?? cueSheetName) + ".acb", "");
        return UniTask.WaitWhile(() => CriAtom.CueSheetsAreLoading);
    }

    /// <summary>
    /// (デバッグ用)ノーツのプレビューに使用するオブジェクトの用意をします
    /// </summary>
    public static GameObject GetPreviewObject()
    {
        GameObject previewObj = GameObject.FindObjectsByType<GameObject>(FindObjectsInactive.Include, FindObjectsSortMode.None)
            .Where(obj => obj.name == "Preview2D")
            .FirstOrDefault();
        previewObj.SetActive(true);
        foreach(var child in previewObj.transform.OfType<Transform>().ToArray())
        {
            GameObject.DestroyImmediate(child.gameObject);
        }
        return previewObj;
    }

    /// <summary>
    /// 入力された配列内の値をフォーマットに従って文字列に変換します
    /// </summary>
    public static string GetContentFrom<T>(T[] array)
    {
        StringBuilder sb = new ();
        for(int i = 0; i < array.Length; i++)
        {
            sb.Append(GetFieldContent(array[i]));
            if(i == array.Length - 1) break;
            sb.Append("\n");
        }
        return sb.ToString();


        static StringBuilder GetFieldContent(T t)
        {
            StringBuilder sb = new ();
            Type type = typeof(T);
            FieldInfo[] fields = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            for(int i = 0; i < fields.Length; i++)
            {
                object v = fields[i].GetValue(t);
                sb.Append(v);
                if(i == fields.Length - 1) break;
                sb.Append("|");
            }
            return sb;
        }
    }

    /// <summary>
    /// 入力された値をフォーマットに従って文字列に変換します
    /// </summary>
    public static string GetContentFrom(params object[] objects)
    {
        string text = null;
        for(int i = 0; i < objects.Length; i++)
        {
            text += objects[i];
            if(i == objects.Length - 1) break;
            text += "|";
        }
        return text;
    }

    /// <summary>
    /// 配列を入力されたフォーマットに従って生成します
    /// </summary>
    public static T[] GetArrayFrom<T>(string content)
    {
        if(string.IsNullOrWhiteSpace(content)) return null;
        var txts = content.Split("\n");
        var array = new T[txts.Length];
        for(int i = 0; i < array.Length; i++)
        {
            array[i] = GetInstanceFromContent(txts[i]);
        }
        return array;


        static T GetInstanceFromContent(string content)
        {
            var fTxts = content.Split('|');
            Type type = typeof(T);
            object instance = Activator.CreateInstance(type);
            FieldInfo[] fields = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            for(int i = 0; i < fields.Length; i++)
            {
                FieldInfo f = fields[i];
                Type fieldType = f.FieldType;
                f.SetValue(instance, Convert(fieldType, fTxts[i]));
            }
            return (T)instance;
        }

        static object Convert(Type type, string stringValue)
        {
            if (type == typeof(Vector2))
            {
                return stringValue.ToVector2();
            }
            else if (type == typeof(Vector3))
            {
                return stringValue.ToVector3();
            }
            var converter = System.ComponentModel.TypeDescriptor.GetConverter(type);
            return converter.ConvertFrom(stringValue);
        }
    }
}
