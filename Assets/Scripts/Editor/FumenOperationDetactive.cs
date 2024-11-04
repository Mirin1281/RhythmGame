using UnityEngine;
using UnityEditor;
using NoteGenerating;
using System.Reflection;

[InitializeOnLoad]
public class FumenOperationDetactive
{
    enum OperationType
    {
        Copy,
        Duplicate,
    }

    static FumenOperationDetactive()
    {
        bool keyDown = false;
        void function()
        {
            if (!keyDown && Event.current.type == EventType.KeyDown)
            {
                keyDown = true;

                if(Event.current.keyCode == KeyCode.C)
                {
                    var fumenData = Selection.GetFiltered<FumenData>(SelectionMode.Assets);
                    if (fumenData != null)
                    {
                        Log(OperationType.Copy);
                    }
                }

                if(Event.current.keyCode == KeyCode.D)
                {
                    var fumenData = Selection.GetFiltered<FumenData>(SelectionMode.Assets);
                    if (fumenData != null)
                    {
                        Log(OperationType.Duplicate);
                    }
                }
            }

            if (keyDown && Event.current.type == EventType.KeyUp)
            {
                keyDown = false;
            }
        }

        FieldInfo info = typeof(EditorApplication).GetField("globalEventHandler", BindingFlags.Static | BindingFlags.Instance | BindingFlags.NonPublic);
        EditorApplication.CallbackFunction functions = (EditorApplication.CallbackFunction)info.GetValue(null);
        functions += function;
        info.SetValue(null, functions);
    }

    static void Log(OperationType operationType)
    {
        if(operationType == OperationType.Copy)
        {
            Debug.LogError("譜面データのコピーはバグに繋がるため、インスペクターの複製ボタンからコピーしてください");
        }
        else if(operationType == OperationType.Duplicate)
        {
            Debug.LogError("譜面データの複製はバグに繋がるため、インスペクターの複製ボタンから行ってください");
        }
    }
}
