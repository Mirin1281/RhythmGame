using UnityEngine;
using UnityEditor;
using ItemType = NoteCreating.F_ItemMove.ItemType;
using System.Reflection;
using System.Text.RegularExpressions;
using System;
using System.Collections;
using System.Collections.Generic;

namespace NoteCreating.Editor
{
    [CustomPropertyDrawer(typeof(F_ItemMove))]
    public class F_ItemMoveDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var h = new DrawerHelper(position, property);

            h.PropertyField("mirror");
            h.SetY();

            h.PropertyField("summary");
            h.SetY();
            h.SetY(10);

            var itemTypeProp = h.PropertyField("itemType");
            h.SetY();

            var itemType = (ItemType)itemTypeProp.enumValueIndex;

            if (itemType == ItemType.HoldNote)
            {
                h.PropertyField("option", overrideName: "Length");
                h.SetY();
            }
            else if (itemType is ItemType.NormalNote or ItemType.SlideNote)
            {
                h.PropertyField("setJudge");
                h.SetY();
                h.PropertyField("isMultitap");
                h.SetY();
            }

            h.PropertyField("lifeLpb");
            h.SetY();

            h.PropertyField("isChainWait");
            h.SetY();

            h.PropertyField("basePos");
            h.SetY();

            var isRotateFromPosProp = h.PropertyField("isRotateFromPos");
            h.SetY();

            if (isRotateFromPosProp.boolValue)
            {
                h.PropertyField("rotateFromPos");
                h.SetY();

                h.PropertyField("centerPos");
                h.SetY();
            }

            h.SetY(10);

            if (GUI.Button(new Rect(h.GetX(), h.GetY(), h.GetWidth(), DrawerHelper.Height), "[0]のAlpha設定を全てに適用"))
            {
                Undo.RecordObject(property.serializedObject.targetObject, "Set Alpha Setting");
                var datasProp = property.FindPropertyRelative("createDatas");

                var commandData = property.serializedObject.targetObject as CommandData;
                var itemMove = commandData.GetCommandBase() as F_ItemMove;
                if (itemMove == null)
                {
                    var loopDelay = commandData.GetCommandBase() as F_LoopDelay;
                    itemMove = loopDelay.GetChildCommand() as F_ItemMove;
                }
                var datasInfo = itemMove.GetType().GetField("createDatas", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                var createDatas = datasInfo.GetValue(itemMove) as F_ItemMove.CreateData[];
                var alphaEaseDatasInfo = typeof(F_ItemMove.CreateData).GetField("alphaEaseDatas", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                var first = (F_ItemMove.EaseData<float>[])alphaEaseDatasInfo.GetValue(createDatas[0]);

                for (int i = 1; i < datasProp.arraySize; i++)
                {
                    alphaEaseDatasInfo.SetValue(createDatas[i], first.Clone());
                }
            }

            h.SetY();
            h.SetY(10);

            h.PropertyField("createDatas");
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return 13.5f * DrawerHelper.Height + EditorGUI.GetPropertyHeight(property.FindPropertyRelative("createDatas"));
        }
    }

    [CustomPropertyDrawer(typeof(F_ItemMove.CreateData))]
    public class F_ItemMove_CreateDataDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var h = new DrawerHelper(position, property);

            float tmpWidth = h.GetWidth() / 13f;
            h.DrawBox(new Rect(position.x - tmpWidth, position.y, position.width + tmpWidth, DrawerHelper.Height), Color.cyan);

            h.LabelField($"Element {h.GetArrayElementIndex()}");
            h.SetY();
            h.SetY(10);

            h.PropertyField("delayLPB");
            h.SetY();
            h.SetY(5);

            var enabledProp = h.PropertyField("enabled");
            h.SetY();
            h.SetY(5);

            using (new EditorGUI.DisabledGroupScope(enabledProp.boolValue == false))
            {
                h.DrawBox(new Rect(h.GetX(), h.GetY(), h.GetWidth(), DrawerHelper.Height), Color.white);
                h.PropertyField("startPos");
                h.SetY();

                var posDataProp = h.PropertyField("posEaseDatas");
                h.SetY(EditorGUI.GetPropertyHeight(posDataProp) + 10);

                h.DrawBox(new Rect(h.GetX(), h.GetY(), h.GetWidth(), DrawerHelper.Height), Color.white);
                h.PropertyField("startRot");
                h.SetY();

                var rotDataProp = h.PropertyField("rotEaseDatas");
                h.SetY(EditorGUI.GetPropertyHeight(rotDataProp) + 10);

                h.DrawBox(new Rect(h.GetX(), h.GetY(), h.GetWidth(), DrawerHelper.Height), Color.white);
                h.PropertyField("startAlpha");
                h.SetY();

                h.PropertyField("alphaEaseDatas");
            }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            float height = 8.5f * DrawerHelper.Height;
            height += EditorGUI.GetPropertyHeight(property.FindPropertyRelative("posEaseDatas"));
            height += EditorGUI.GetPropertyHeight(property.FindPropertyRelative("rotEaseDatas"));
            height += EditorGUI.GetPropertyHeight(property.FindPropertyRelative("alphaEaseDatas"));
            return height;
        }
    }

    [CustomPropertyDrawer(typeof(F_ItemMove.EaseData<>))]
    public class F_ItemMove_EaseData_Drawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var h = new DrawerHelper(position, property);

            h.LabelField($"Element {h.GetArrayElementIndex()}");
            h.SetY();

            h.SetIndentLevel(true);

            var easeTypeProp = property.FindPropertyRelative("easeType");
            using (new EditorGUI.DisabledGroupScope((EaseType)easeTypeProp.enumValueIndex == EaseType.None))
            {
                h.PropertyField("from");
                h.SetY();
            }

            h.LabelField("Ease");
            float labelWidth = EditorGUIUtility.labelWidth;
            float w = h.GetWidth() - labelWidth;
            float margin = 5;
            h.SetWidth(w / 2f - margin);
            h.SetX(labelWidth);
            h.PropertyField(easeTypeProp, false);

            h.SetX(labelWidth + w / 2f + margin);
            h.PropertyField("easeTime", false);
            h.SetY();

            h.SetIndentLevel(false);
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return 3 * DrawerHelper.Height;
        }
    }
}