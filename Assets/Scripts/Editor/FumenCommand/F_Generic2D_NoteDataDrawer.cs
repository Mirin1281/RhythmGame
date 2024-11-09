using UnityEngine;
using UnityEditor;

namespace NoteGenerating.Editor
{
    [CustomPropertyDrawer(typeof(NoteData))]
    public class F_Generic2D_NoteDataDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var h = new PropertyDrawerHelper(position, property);
            var width = h.GetWidth();
            
            h.SetWidth(width * 0.12f);
            h.LabelField("待:");
            h.SetX(h.GetX() - 30f);
            var waitProp = h.PropertyField("wait", false);

            h.SetX(width / 5f * 1f);
            var typeProp = h.PropertyField(width * 0.16f, "type", false);
            var type = (CreateNoteType)typeProp.enumValueIndex;

            if(type == CreateNoteType._None)
            {
                h.DrawBox(new Rect(19, position.y - 2, width + 40, 22), Color.cyan);
                return;
            }

            h.SetX(width / 5f * 2f);
            h.LabelField("X:");
            h.SetX(h.GetX() - 30f);
            h.PropertyField("x", false);

            h.SetX(width / 5f * 3f);
            h.LabelField("幅:");
            h.SetX(h.GetX() - 30f);
            h.PropertyField(width * 0.12f - 10, "width", false);

            if(type == CreateNoteType.Hold)
            {
                h.SetX(width / 5f * 4f);
                h.LabelField("長:");
                h.SetX(h.GetX() - 30f);
                h.PropertyField(width * 0.12f - 10, "length", false);
            }

            if(waitProp.floatValue == 0f && GetElementIndex(property) != 0)
            {
                h.DrawBox(new Rect(19, position.y - 22, width + 40, 2f * 20), Color.yellow);
            }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
            => 18;

        // https://www.urablog.xyz/entry/2017/02/12/165706
        int GetElementIndex( SerializedProperty property )
        {
            string propertyPath = property.propertyPath;
            var propertys = propertyPath.Split('.');

            // このデータが配列内の要素ならば、「aaaa.Array.data[...]」の形になるはずだ！
            if(propertys.Length < 3) return -1;

            // クラスを経由して、パスが長くなった場合でも、このデータが配列内の要素ならば、その後ろから二番目は「Array」になるはずだ！
            string arrayProperty = propertys[propertys.Length - 2];
            if(arrayProperty != "Array") return -1;

            // このデータが配列内の要素ならば、data[...]の形になっているはずだ！
            var paths = propertyPath.Split('.');
            var lastPath = paths[propertys.Length - 1];
            if(lastPath.StartsWith("data[") == false) return -1;

            // 数字の要素だけ抜き出すんだ！
            var regex = new System.Text.RegularExpressions.Regex(@"[^0-9]");
            var countText = regex.Replace(lastPath, "");
            if (int.TryParse(countText, out int index) == false) return -1;

            return index;
        }
    }

    /*[CustomPropertyDrawer(typeof(F_Generic2D))]
    public class F_Generic2DDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (GUILayout.Button("waitをずらす"))
            {
                var p = property.FindPropertyRelative("noteDatas");
                int k = p.arraySize;
                while (k > -10 || k < 50)
                {
                    k--;
                    
                    var d = p.GetArrayElementAtIndex(k);
                    var waitProp = d.FindPropertyRelative("wait");
                    if(k == 0)
                    {
                        waitProp.floatValue = 0;
                        return;
                    }

                    var nextD = p.GetArrayElementAtIndex(k - 1);
                    var nextWaitProp = nextD.FindPropertyRelative("wait");

                    waitProp.floatValue = nextWaitProp.floatValue;
                }
            }

            EditorGUI.PropertyField(position, property.FindPropertyRelative("isInverse"));

            position.y += EditorGUIUtility.singleLineHeight;
            EditorGUI.PropertyField(position, property.FindPropertyRelative("speedRate"));

            position.y += EditorGUIUtility.singleLineHeight;
            EditorGUI.PropertyField(position, property.FindPropertyRelative("isSpeedChangable"));

            position.y += EditorGUIUtility.singleLineHeight;
            var parentProp = property.FindPropertyRelative("parentGeneratable");
            EditorGUI.PropertyField(new Rect(position.x - 15, position.y, position.width + 15, position.height), parentProp);


            position.y += EditorGUI.GetPropertyHeight(parentProp);
            EditorGUI.PropertyField(position, property.FindPropertyRelative("isCheckSimultaneous"));

            position.y += EditorGUIUtility.singleLineHeight;
            var noteDatasProp = property.FindPropertyRelative("noteDatas");
            EditorGUI.PropertyField(position, noteDatasProp);

            if(noteDatasProp.isExpanded == false) return;

            GUILayoutUtility.GetRect(0, 140 + noteDatasProp.arraySize * (18 + 2));

            var endProperty = noteDatasProp.GetEndProperty();
            noteDatasProp.NextVisible(true);
            position.y += 6;
            int i = 0;
            while (noteDatasProp.NextVisible(false))
            {
                position.y += 20;
                i++;
                if (SerializedProperty.EqualContents(noteDatasProp, endProperty)) break;
                if (noteDatasProp.propertyType == SerializedPropertyType.ArraySize
                 || i % 2 == 0) continue;
            }
        }
    }*/
}