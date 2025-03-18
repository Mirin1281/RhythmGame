using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;

#if UNITY_EDITOR
namespace NoteCreating
{
    // CSVエクスポート後、変数を追加・削除した場合は該当クラスor構造体にこのインターフェースを
    // アタッチしてインポートすることで、エラーが出ないように適切に処理を行うことができます

    // ゲッターには追加した変数名を入れてください(nameof推奨)
    public interface IFieldAddHandler
    {
        string[] AddedFieldNames { get; }
    }

    // ゲッターには削除した変数のインデックスを入れてください
    // 例えば一番最初の変数を削除した場合は0となります
    public interface IFieldDeleteHandler
    {
        int[] DeletedFieldIndices { get; }
    }

    public static class CommandCSVParser
    {
        /// <summary>
        /// 入出力時にデバッグ用のログを出す
        /// </summary>
        static readonly bool ShowDebugLog = false;

        /// <summary>
        /// Unity標準のコンポーネントは変数の数が大きすぎるため、この値を超えたら入出力を無視します
        /// </summary>
        const int LimitClassFieldCount = 50;

        const char InterfaceDelimiter = '(';


        /// <summary>
        /// リフレクションでインスタンス内の変数を全て書き出します
        /// </summary>
        public static string GetFieldContent<T>(T cmd) where T : CommandBase
        {
            return GetFieldContentGeneric(cmd);
        }
        static string GetFieldContentGeneric(object obj, int delimiterLevel = -1)
        {
            if (obj == null) return null;
            delimiterLevel++;
            StringBuilder sb = new();
            FieldInfo[] fieldInfos = obj.GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            for (int i = 0; i < fieldInfos.Length; i++)
            {
                var field = fieldInfos[i];
                object v = field.GetValue(obj);

                // プリミティブ || string || enumなら追加
                if (field.FieldType.IsPrimitive || field.FieldType == typeof(string) || field.FieldType.IsEnum)
                {
                    if (ShowDebugLog)
                    {
                        Debug.Log($"{delimiterLevel}  {field.FieldType}  {field.Name}  {v}");
                    }
                    sb.Append(v);
                }
                else if (field.FieldType.IsArray) // 配列なら中身を追加
                {
                    var arrayContent = GetContentFromArray(v as Array, delimiterLevel);
                    sb.Append(arrayContent);
                }
                else if (field.FieldType.IsInterface)
                {
                    // インターフェースの特別な処理(読み込み時に困るので、クラス名を付加する)
                    if (v == null)
                    {
                        sb.Append("Null");
                        if (i == fieldInfos.Length - 1) break;
                    }
                    else
                    {
                        // インターフェースがアタッチされているクラス名を取得
                        int index = v.ToString().LastIndexOf('.');
                        string className = v.ToString().Substring(index + 1, v.ToString().Length - index - 1);
                        sb.Append(className);
                    }
                    sb.Append(InterfaceDelimiter);
                    var content = GetFieldContentGeneric(v, delimiterLevel);
                    sb.Append(content);
                }
                else if (field.FieldType.IsClass) // それ以外のクラスは変数の個数が大きかったらスルー
                {
                    int fieldCount = field.FieldType.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).Length;
                    if (fieldCount < LimitClassFieldCount)
                    {
                        var content = GetFieldContentGeneric(v, delimiterLevel);
                        sb.Append(content);
                    }
                    else
                    {
                        Debug.LogWarning($"{field.FieldType} はサイズが大きすぎたため書き出しをスキップされました");
                    }
                }
                else // それ以外のVector2やMirrorなどは再帰的にに中身を取得して追加
                {
                    var content = GetFieldContentGeneric(v, delimiterLevel);
                    sb.Append(content);
                }

                if (i == fieldInfos.Length - 1) break;
                sb.Append(GetDelimiter(delimiterLevel));
            }

            return sb.ToString();


            // 配列の中身を取得
            static StringBuilder GetContentFromArray(Array array, int delimiterLevel)
            {
                if (array == null) return null;
                delimiterLevel++;
                StringBuilder sb = new();

                for (int i = 0; i < array.Length; i++)
                {
                    var v = array.GetValue(i);
                    if (array.GetType().GetElementType().IsInterface)
                    {
                        if (v == null)
                        {
                            sb.Append("Null");
                        }
                        else
                        {
                            // インターフェースがアタッチされているクラス名を取得
                            int index = v.ToString().LastIndexOf('.');
                            string className = v.ToString().Substring(index + 1, v.ToString().Length - index - 1);
                            sb.Append(className);
                        }
                        sb.Append(InterfaceDelimiter);
                        //var content = GetFieldContentGeneric(v, delimiterLevel);
                        //sb.Append(content);
                    }

                    var elementContent = GetFieldContentGeneric(v, delimiterLevel);
                    sb.Append(elementContent);
                    if (i == array.Length - 1) break;
                    sb.Append(GetDelimiter(delimiterLevel));
                }
                return sb;
            }
        }

        /// <summary>
        /// リフレクションでインスタンス内の変数を設定します
        /// </summary>
        public static void SetField<T>(T command, string content) where T : CommandBase
        {
            SetFieldGeneric(command, content);
            if (ShowDebugLog) Debug.Log($"<color=red>End: {(command as ICommand).GetName(true)}</color>");
        }

        static object SetFieldGeneric(object obj, string content, List<int> fieldIndexList = null)
        {
            if (string.IsNullOrWhiteSpace(content) || obj == null) return null;

            fieldIndexList ??= new();
            fieldIndexList.Add(0);
            List<FieldInfo> fieldInfos = obj.GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).ToList();
            List<string> fieldValues = content.Split(GetDelimiter(fieldIndexList.Count - 1)).ToList();
            if (ShowDebugLog)
            {
                //fieldInfos.ForEach(v => Debug.Log(v.FieldType));
                //fieldValues.ForEach(v => Debug.LogWarning(v));
                Debug.Log($"{content}  Delimiter: {GetDelimiter(fieldIndexList.Count - 1)}");
            }

            AdjustFieldsAndValuesOrder(obj, fieldInfos, fieldValues);

            if (fieldInfos.Count != fieldValues.Count)
            {
                Debug.LogError($"Mismatch: Fields - {fieldInfos.Count}  Contents - {fieldValues.Count}\nContent - {content}");
            }

            for (int i = 0; i < fieldInfos.Count; i++)
            {
                var field = fieldInfos[i];
                var fieldValue = fieldValues[i];
                if (!TrySetPrimitiveOrEnum(field, obj, fieldValue, fieldIndexList))
                {
                    if (field.FieldType.IsArray) HandleArrayField(field, obj, fieldValue, fieldIndexList);
                    else if (field.FieldType.IsInterface) HandleInterfaceField(field, obj, fieldValue, fieldIndexList);
                    else if (field.FieldType.IsClass) HandleClassField(field, obj, fieldValue, fieldIndexList);
                    else HandleOtherField(field, obj, fieldValue, fieldIndexList);
                }
                fieldIndexList[fieldIndexList.Count - 1]++;
            }

            fieldIndexList.RemoveAt(fieldIndexList.Count - 1);
            return obj;



            static void AdjustFieldsAndValuesOrder(object obj, List<FieldInfo> fieldInfos, List<string> fieldValues)
            {
                if (obj is IFieldAddHandler addHandler)
                {
                    var addIndices = addHandler.AddedFieldNames
                        .Select(name => fieldInfos.FindIndex(f => f.Name == name))
                        .Where(i => i >= 0).ToList();
                    for (int i = addIndices.Count - 1; i >= 0; i--)
                    {
                        fieldInfos.RemoveAt(addIndices[i]);
                    }
                }
                if (obj is IFieldDeleteHandler deleteHandler)
                {
                    var deleteIndices = deleteHandler.DeletedFieldIndices;
                    for (int i = deleteIndices.Length - 1; i >= 0; i--)
                    {
                        fieldValues.RemoveAt(deleteIndices[i]);
                    }
                }
            }

            static bool TrySetPrimitiveOrEnum(FieldInfo field, object obj, string value, List<int> fieldIndexList)
            {
                if ((field.FieldType.IsPrimitive || field.FieldType == typeof(string) || field.FieldType.IsEnum) == false) return false;
                if (ShowDebugLog)
                {
                    string s = string.Empty;
                    fieldIndexList.ForEach(i => s += i + "-");
                    Debug.Log($"{s[..^1]}  {field.Name}  {field.FieldType}  {value}");
                }

                try
                {
                    var converter = System.ComponentModel.TypeDescriptor.GetConverter(field.FieldType);
                    var instance = converter.ConvertFrom(value);
                    if (instance != null) field.SetValue(obj, instance);
                }
                catch
                {
                    Debug.LogWarning($"{value} could not be converted to {field.FieldType}");
                }
                return true;
            }

            static void HandleArrayField(FieldInfo arrayField, object obj, string value, List<int> fieldIndexList)
            {
                fieldIndexList.Add(0);
                var elementType = arrayField.FieldType.GetElementType();
                var elementValues = value.Split(GetDelimiter(fieldIndexList.Count - 1));

                if (elementValues.Length == 0 || (elementValues.Length == 1 && string.IsNullOrEmpty(elementValues[0])))
                {
                    fieldIndexList.RemoveAt(fieldIndexList.Count - 1);
                    return;
                }

                var array = Array.CreateInstance(elementType, elementValues.Length);
                for (int i = 0; i < elementValues.Length; i++)
                {
                    if (elementType.IsInterface)
                    {
                        var val = elementValues[i];
                        int endIndex = val.IndexOf(InterfaceDelimiter);
                        if (endIndex < 0) return;

                        string className = val[..endIndex];
                        string content = val[(endIndex + 1)..];
                        var instance = CreateInstanceByClassName<object>(className);

                        if (instance != null)
                        {
                            instance = SetFieldGeneric(instance, content, fieldIndexList);
                            if (instance != null)
                            {
                                array.SetValue(instance, i);
                            }
                        }
                    }
                    else
                    {
                        var element = CreateInstance(elementType);
                        array.SetValue(SetFieldGeneric(element, elementValues[i], fieldIndexList), i);
                    }
                    fieldIndexList[fieldIndexList.Count - 1]++;
                }
                arrayField.SetValue(obj, array);
                fieldIndexList.RemoveAt(fieldIndexList.Count - 1);
            }

            static void HandleInterfaceField(FieldInfo field, object obj, string value, List<int> fieldIndexList)
            {
                int endIndex = value.IndexOf(InterfaceDelimiter);
                if (endIndex < 0) return;

                string className = value[..endIndex];
                string content = value[(endIndex + 1)..];
                var instance = CreateInstanceByClassName<object>(className);

                if (instance != null)
                {
                    instance = SetFieldGeneric(instance, content, fieldIndexList);
                    if (instance != null)
                    {
                        field.SetValue(obj, instance);
                    }
                }
            }

            static void HandleClassField(FieldInfo field, object obj, string value, List<int> fieldIndexList)
            {
                int fieldCount = field.FieldType.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).Length;
                if (fieldCount < LimitClassFieldCount)
                {
                    HandleOtherField(field, obj, value, fieldIndexList);
                }
                else
                {
                    Debug.LogWarning($"{field.FieldType} はサイズが大きすぎたため読み込みをスキップされました");
                }
            }

            static void HandleOtherField(FieldInfo field, object obj, string value, List<int> fieldIndexList)
            {
                try
                {
                    var instance = Activator.CreateInstance(field.FieldType);
                    instance = SetFieldGeneric(instance, value, fieldIndexList);
                    if (instance != null)
                    {
                        field.SetValue(obj, instance);
                    }
                }
                catch
                {
                    Debug.LogWarning($"Failed to convert {field.FieldType}");
                }
            }

            static T CreateInstanceByClassName<T>(string className, string namespaceName = nameof(NoteCreating)) where T : class
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

            static object CreateInstance(Type type)
            {
                object obj;
                try
                {
                    obj = Activator.CreateInstance(type);
                    // 引数無しコンストラクタが無い場合は引数情報を引っ張ってきてConstructorInfoから生成する
                }
                catch (MissingMethodException)
                {
                    var constructorInfos = type.GetConstructors();
                    var constructor = type.GetConstructors()[0];
                    var parameters = constructor.GetParameters().Select(parInfo => CreateInstance(parInfo.ParameterType)).ToArray();
                    obj = constructor.Invoke(parameters);
                }
                return obj;
            }
        }

        static char GetDelimiter(int depth) => depth switch
        {
            0 => '|',
            1 => '#',
            2 => '%',
            3 => '&',
            4 => '~',
            5 => '"',
            6 => '>',
            7 => '<',
            8 => '=',
            9 => '?',
            10 => '!',
            11 => '{',
            12 => '}',
            13 => ':',
            _ => throw new ArgumentOutOfRangeException(depth.ToString())
        };
    }
}
#endif