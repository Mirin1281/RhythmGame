using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace NoteCreating
{
    public static class FumenDebugUtility
    {
        /// <summary>
        /// ノーツのプレビューに使用するオブジェクトの用意をします
        /// </summary>
        public static GameObject GetPreviewObject(bool isClear = true)
        {
            GameObject previewObj = GameObject.FindObjectsByType<GameObject>(FindObjectsInactive.Include, FindObjectsSortMode.None)
                .Where(obj => obj.name == "Preview2D")
                .FirstOrDefault();
            if (previewObj == null) return null;
            if (isClear)
            {
                previewObj.SetActive(true);
                foreach (var child in previewObj.transform.OfType<Transform>().ToArray())
                {
                    GameObject.DestroyImmediate(child.gameObject);
                }
            }
            return previewObj;
        }

        /// <summary>
        /// クラスを型名から生成します
        /// </summary>
        public static T CreateInstance<T>(string className, string namespaceName = nameof(NoteCreating)) where T : class
        {
            Type t = GetTypeByClassName(className, namespaceName);
            if (t == null) return null;
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

        public static string GetContent<T>(T cmd) where T : CommandBase
        {
            int separateLevel = 1;
            StringBuilder sb = new();
            Type type = cmd.GetType();
            FieldInfo[] fields = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            for (int i = 0; i < fields.Length; i++)
            {
                var f = fields[i];
                object v = f.GetValue(cmd);
                if (f.FieldType.IsArray)
                {
                    Array array = v as Array;
                    sb.Append(GetContentFromArray(array, separateLevel));
                }
                else
                {
                    sb.Append(v);
                }

                sb.Append(GetSeparator(separateLevel));
            }

            if (cmd is IMirrorable inversable)
            {
                sb.Append(inversable.IsMirror);
            }
            return sb.ToString();


            static StringBuilder GetContentFromArray(Array array, int separateLevel)
            {
                if (array == null) return null;
                StringBuilder sb = new();
                separateLevel++;
                for (int i = 0; i < array.Length; i++)
                {
                    var element = array.GetValue(i);
                    sb.Append(GetFieldContent(element, element.GetType(), separateLevel));
                    if (i == array.Length - 1) break;
                    sb.Append(GetSeparator(separateLevel));
                }
                return sb;


                static StringBuilder GetFieldContent(object t, Type type, int separateLevel)
                {
                    StringBuilder sb = new();
                    separateLevel++;
                    FieldInfo[] fields = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                    for (int i = 0; i < fields.Length; i++)
                    {
                        var f = fields[i];
                        object v = f.GetValue(t);
                        if (f.FieldType.IsArray)
                        {
                            Array array = v as Array;
                            sb.Append(GetContentFromArray(array, separateLevel));
                        }
                        else
                        {
                            sb.Append(v);
                        }

                        if (i == fields.Length - 1) break;
                        sb.Append(GetSeparator(separateLevel));
                    }
                    return sb;
                }
            }
        }

        public static void SetMember<T>(T command, string content) where T : CommandBase
        {
            if (string.IsNullOrWhiteSpace(content)) return;

            int separateLevel = 1;

            var type = command.GetType();
            var fieldStrings = content.Split(GetSeparator(separateLevel));
            FieldInfo[] fields = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            for (int i = 0; i < fields.Length; i++)
            {
                FieldInfo f = fields[i];
                Type fType = f.FieldType;
                if (fType.IsArray)
                {
                    var arrayType = fType.GetElementType();
                    var createdArray = CreateArray(arrayType, fieldStrings[i], separateLevel);
                    f.SetValue(command, createdArray);
                }
                else
                {
                    var v = Convert(fType, fieldStrings[i]);
                    if (v != null)
                    {
                        f.SetValue(command, v);
                    }
                }
            }
            if (command is IMirrorable mirrorable)
            {
                Debug.Log(fieldStrings[fields.Length]);
                mirrorable.SetIsMirror(bool.Parse(fieldStrings[fields.Length]));
            }


            static Array CreateArray(Type type, string value, int separateLevel)
            {
                separateLevel++;
                var elementStrings = value.Split(GetSeparator(separateLevel));
                if (elementStrings.Length == 1 && string.IsNullOrEmpty(elementStrings[0])) return null;
                var array = Array.CreateInstance(type, elementStrings.Length);
                for (int i = 0; i < array.Length; i++)
                {
                    array.SetValue(GetElement(type, elementStrings[i], separateLevel), i);
                }
                return array;


                static object GetElement(Type type, string value, int separateLevel)
                {
                    separateLevel++;
                    var elementStrings = value.Split(GetSeparator(separateLevel));
                    if (elementStrings.Length == 1 && string.IsNullOrEmpty(elementStrings[0])) return null;
                    object instance = Activator.CreateInstance(type);
                    FieldInfo[] fields = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                    for (int i = 0; i < fields.Length; i++)
                    {
                        FieldInfo f = fields[i];
                        if (f.FieldType.IsArray)
                        {
                            var arrayType = f.FieldType.GetElementType();
                            var createdArray = CreateArray(arrayType, elementStrings[i], separateLevel);
                            f.SetValue(instance, createdArray);
                        }
                        else
                        {
                            var v = Convert(f.FieldType, elementStrings[i]);
                            if (v != null)
                            {
                                f.SetValue(instance, v);
                            }
                        }
                    }
                    return instance;
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
                object obj;
                try
                {
                    obj = converter.ConvertFrom(stringValue);
                }
                catch
                {
                    return null;
                }
                return obj;
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
                5 => '~',
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        public static void DebugPreview2DNotes(IEnumerable<INoteData> noteDatas, NoteCreateHelper Helper, bool isInverse, bool isClearPreview)
        {
            float Inv(float x) => x * (isInverse ? -1 : 1);


            GameObject previewObj = GetPreviewObject(isClearPreview);
            int simultaneousCount = 0;
            float beforeY = -1;
            RegularNote beforeNote = null;

            float y = 0f;
            foreach (var data in noteDatas)
            {
                y += Helper.GetTimeInterval(data.Wait) * RhythmGameManager.Speed;

                var type = data.Type;
                if (type == CreateNoteType.Normal)
                {
                    DebugNote(data.X, y, RegularNoteType.Normal, data.Width);
                }
                else if (type == CreateNoteType.Slide)
                {
                    DebugNote(data.X, y, RegularNoteType.Slide, data.Width);
                }
                else if (type == CreateNoteType.Flick)
                {
                    DebugNote(data.X, y, RegularNoteType.Flick, data.Width);
                }
                else if (type == CreateNoteType.Hold)
                {
                    if (data.Length == 0)
                    {
                        Debug.LogWarning("ホールドの長さが0です");
                        continue;
                    }
                    DebugHold(data.X, y, data.Length, data.Width);
                }
            }

            if (isClearPreview == false) return;
            float lineY = 0f;
            for (int i = 0; i < 10000; i++)
            {
                var line = Helper.PoolManager.LinePool.GetLine();
                line.SetPos(new Vector3(0, lineY));
                line.transform.SetParent(previewObj.transform);
                lineY += Helper.GetTimeInterval(4) * RhythmGameManager.Speed;
                if (lineY > y) break;
            }


            void DebugNote(float x, float y, RegularNoteType type, float width)
            {
                RegularNote note = Helper.GetNote(type);
                if ((width is 0 or 1) == false)
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
                if ((width is 0 or 1) == false)
                {
                    hold.SetWidth(width);
                }
                hold.SetMaskLocalPos(new Vector2(Inv(x), 0));
                var startPos = new Vector3(Inv(x), y);
                hold.SetPos(startPos);
                hold.transform.SetParent(previewObj.transform);

                SetSimultaneous(hold, y);
            }

            void SetSimultaneous(RegularNote note, float y)
            {
                if (beforeY == y)
                {
                    if (simultaneousCount == 1)
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
    }
}