using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using CriWare;
using Cysharp.Threading.Tasks;
using NoteGenerating;
using UnityEngine;
using UnityEngine.AddressableAssets;

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

    public static string GetAddress(AssetReference assetReference)
    {
        var handle = Addressables.LoadResourceLocationsAsync(assetReference);
        var result = handle.WaitForCompletion();
        var location = result.FirstOrDefault();
        var address = location?.PrimaryKey ?? string.Empty;
        return address;
    }


    /// <summary>
    /// (デバッグ用)ノーツのプレビューに使用するオブジェクトの用意をします
    /// </summary>
    public static GameObject GetPreviewObject(bool isClear = true)
    {
        GameObject previewObj = GameObject.FindObjectsByType<GameObject>(FindObjectsInactive.Include, FindObjectsSortMode.None)
            .Where(obj => obj.name == "Preview2D")
            .FirstOrDefault();
        if(previewObj == null) return null;
        if(isClear)
        {
            previewObj.SetActive(true);
            foreach(var child in previewObj.transform.OfType<Transform>().ToArray())
            {
                GameObject.DestroyImmediate(child.gameObject);
            }
        }
        return previewObj;
    }

    /// <summary>
    /// 入力された配列内の値をフォーマットに従って文字列に変換します
    /// </summary>
    public static string GetContentFrom<T>(T[] array, string separator = "|")
    {
        StringBuilder sb = new ();
        for(int i = 0; i < array.Length; i++)
        {
            sb.Append(GetFieldContent(array[i]));
            if(i == array.Length - 1) break;
            sb.Append("\n");
        }
        return sb.ToString();


        StringBuilder GetFieldContent(T t)
        {
            StringBuilder sb = new ();
            Type type = typeof(T);
            FieldInfo[] fields = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            for(int i = 0; i < fields.Length; i++)
            {
                object v = fields[i].GetValue(t);
                sb.Append(v);
                if(i == fields.Length - 1) break;
                sb.Append(separator);
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
    public static T[] GetArrayFrom<T>(string content, string separator = "|")
    {
        if(string.IsNullOrWhiteSpace(content)) return new T[0];
        var txts = content.Split("\n");
        var array = new T[txts.Length];
        for(int i = 0; i < array.Length; i++)
        {
            array[i] = GetInstanceFromContent(txts[i]);
        }
        return array;


        T GetInstanceFromContent(string content)
        {
            var fTxts = content.Split(separator);
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

    /// <summary>
    /// クラスを型名から生成します
    /// </summary>
    public static T CreateInstance<T>(string className, string namespaceName = nameof(NoteGenerating)) where T : class
    {
        Type t = GetTypeByClassName(className, namespaceName);
        if (t == null)
        {
            return null;
        }
        return Activator.CreateInstance(t) as T;


        static Type GetTypeByClassName(string className, string namespaceName)
        {
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach (Type type in assembly.GetTypes())
                {
                    if (type.Name == className && 
                        type.Namespace == namespaceName)
                    {
                        return type;
                    }
                }
            }
            Debug.LogWarning($"{className}クラスが見つかりませんでした！\n" +
                $"タイポもしくは{className}クラスが名前空間{namespaceName}内に存在しない可能性があります");
            return null;
        }
    }

    public static void DebugPreview2DNotes(IEnumerable<INoteData> noteDatas, NoteGenerateHelper Helper, bool isInverse, bool isClearPreview)
    {
        float Inv(float x) => x * (isInverse ? -1 : 1);


        GameObject previewObj = GetPreviewObject(isClearPreview);
        int simultaneousCount = 0;
        float beforeY = -1;
        NoteBase_2D beforeNote = null;

        float y = 0f;
        foreach(var data in noteDatas)
        {
            var type = data.Type;
            if(type == CreateNoteType.Normal)
            {
                DebugNote(data.X, y, NoteType.Normal, data.Width);
            }
            else if(type == CreateNoteType.Slide)
            {
                DebugNote(data.X, y, NoteType.Slide, data.Width);
            }
            else if(type == CreateNoteType.Flick)
            {
                DebugNote(data.X, y, NoteType.Flick, data.Width);
            }
            else if(type == CreateNoteType.Hold)
            {
                if(data.Length == 0)
                {
                    Debug.LogWarning("ホールドの長さが0です");
                    continue;
                }
                DebugHold(data.X, y, data.Length, data.Width);
            }
            y += Helper.GetTimeInterval(data.Wait) * RhythmGameManager.Speed;
        }

        if(isClearPreview == false) return;
        float lineY = 0f;
        for(int i = 0; i < 10000; i++)
        {
            var line = Helper.PoolManager.LinePool.GetLine();
            line.SetPos(new Vector3(0, lineY));
            line.transform.SetParent(previewObj.transform);
            lineY += Helper.GetTimeInterval(4) * RhythmGameManager.Speed;
            if(lineY > y) break;
        }


        void DebugNote(float x, float y, NoteType type, float width)
        {
            NoteBase_2D note = Helper.GetNote2D(type);
            if((width is 0 or 1) == false)
            {
                note.SetWidth(width);
            }
            var startPos = new Vector3(Inv(x), y);
            note.SetPos(startPos);
            note.transform.SetParent(previewObj.transform);

            SetSimultaneous(note, y);
        }

        void DebugHold(float x, float y, float length, float width)
        {
            var holdTime = Helper.GetTimeInterval(length);
            var hold = Helper.GetHold(holdTime * RhythmGameManager.Speed);
            if((width is 0 or 1) == false)
            {
                hold.SetWidth(width);
            }
            hold.SetMaskLocalPos(new Vector2(Inv(x), 0));
            var startPos = new Vector3(Inv(x), y);
            hold.SetPos(startPos);
            hold.transform.SetParent(previewObj.transform);

            SetSimultaneous(hold, y);
        }

        void SetSimultaneous(NoteBase_2D note, float y)
        {
            if(beforeY == y)
            {
                if(simultaneousCount == 1)
                {
                    Helper.PoolManager.SetSimultaneousSprite(beforeNote);
                }
                Helper.PoolManager.SetSimultaneousSprite(note);
                simultaneousCount++;
            }
            else
            {
                simultaneousCount = 1;
            }
            beforeY = y;
            beforeNote = note;
        }
    }

    public static string GetContent<T>(T t)
    {
        int separateLevel = 1;
        StringBuilder sb = new ();
        Type type = typeof(T);
        FieldInfo[] fields = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        for(int i = 0; i < fields.Length; i++)
        {
            var f = fields[i];
            object v = f.GetValue(t);
            if(f.FieldType.IsArray)
            {
                Array array = v as Array;
                sb.Append(GetContentFromArray(array, separateLevel));
            }
            else
            {
                sb.Append(v);
            }
            
            if(i == fields.Length - 1) break;
            sb.Append(GetSeparator(separateLevel));
        }
        return sb.ToString();


        static StringBuilder GetContentFromArray(Array array, int separateLevel)
        {
            StringBuilder sb = new ();
            separateLevel++;
            for(int i = 0; i < array.Length; i++)
            {
                var element = array.GetValue(i);
                sb.Append(GetFieldContent(element, element.GetType(), separateLevel));
                if(i == array.Length - 1) break;
                sb.Append(GetSeparator(separateLevel));
            }
            return sb;


            static StringBuilder GetFieldContent(object t, Type type, int separateLevel)
            {
                StringBuilder sb = new ();
                separateLevel++;
                FieldInfo[] fields = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                for(int i = 0; i < fields.Length; i++)
                {
                    var f = fields[i];
                    object v = f.GetValue(t);
                    if(f.FieldType.IsArray)
                    {
                        Array array = v as Array;
                        sb.Append(GetContentFromArray(array, separateLevel));
                    }
                    else
                    {
                        sb.Append(v);
                    }

                    if(i == fields.Length - 1) break;
                    sb.Append(GetSeparator(separateLevel));
                }
                return sb;
            }
        }

        static char GetSeparator(int level)
        {
            return level switch
            {
                1 => '|',
                2 => '#',
                3 => '%',
                4 => '&',
                _ => throw new ArgumentOutOfRangeException()
            };
        }
    }

    /*public static void GetNoteGenerator<T>(T generator, string content) where T : NoteGeneratorBase
    {
        if(string.IsNullOrWhiteSpace(content)) return;

        var type = typeof(T);
        var fieldStrings = content.Split("|");
        FieldInfo[] fields = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        for(int i = 0; i < fields.Length; i++)
        {
            FieldInfo f = fields[i];
            if(f.FieldType.IsArray)
            {
                var array = f.GetValue(generator) as Array;
                var arrayType = f.FieldType.GetElementType();
                SetToArray(array, arrayType, fieldStrings[i]);
            }
            else
            {
                f.SetValue(generator, fieldStrings[i]);
            }
        }


        static void SetToArray(Array array, Type type, string value)
        {
            var fieldStrings = value.Split("#");
            //FieldInfo[] fields = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            for(int k = 0; k < array.Length; k++)
            {
                var element = array.GetValue(k);
                SetToElement(element, element.GetType(), fieldStrings[k]);
            }
            


            static void SetToElement(object element, Type elementType, string value)
            {
                var fieldStrings = value.Split("%");
                FieldInfo[] fields = elementType.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                for(int i = 0; i < fields.Length; i++)
                {
                    FieldInfo f = fields[i];
                    if(f.FieldType.IsArray)
                    {
                        var array = f.GetValue(element) as Array;
                        var arrayType = f.FieldType.GetElementType();
                        SetToArray(array, arrayType, fieldStrings[i]);
                    }
                    else
                    {
                        f.SetValue(element, Convert(f.FieldType, fieldStrings[i]));
                    }
                    
                }
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
        }*/

    public static void GetNoteGenerator<T>(T generator, string content) where T : NoteGeneratorBase
    {
        if(string.IsNullOrWhiteSpace(content)) return;

        var type = typeof(T);
        var fieldStrings = content.Split("|");
        FieldInfo[] fields = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        for(int i = 0; i < fields.Length; i++)
        {
            FieldInfo f = fields[i];
            if(f.FieldType.IsArray)
            {
                var arrayType = f.FieldType.GetElementType();
                var createdArray = CreateArray(arrayType, fieldStrings[i]);
                f.SetValue(generator, createdArray);
            }
            else
            {
                f.SetValue(generator, fieldStrings[i]);
            }
        }


        static Array CreateArray(Type type, string value)
        {
            var elementStrings = value.Split("#");
            var array = Array.CreateInstance(type, elementStrings.Length);
            for(int i = 0; i < array.Length; i++)
            {
                array.SetValue(GetElement(type, elementStrings[i]), i);
            }
            return array;


            static object GetElement(Type type, string value)
            {
                var elementStrings = value.Split("%");
                object instance = Activator.CreateInstance(type);
                FieldInfo[] fields = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                for(int i = 0; i < fields.Length; i++)
                {
                    FieldInfo f = fields[i];
                    if(f.FieldType.IsArray)
                    {
                        var arrayType = f.FieldType.GetElementType();
                        var createdArray = CreateArray(arrayType, elementStrings[i]);
                        f.SetValue(instance, createdArray);
                    }
                    else
                    {
                        f.SetValue(instance, Convert(f.FieldType, elementStrings[i]));
                    }
                }
                return instance;
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
}
