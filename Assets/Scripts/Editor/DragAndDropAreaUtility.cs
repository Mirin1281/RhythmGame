//  DragAndDropAreaUtility.cs
//  http://kan-kikuchi.hatenablog.com/entry/DragAndDropAreaUtility
//
//  Created by kan.kikuchi on 2021.01.17.

using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

/// <summary>
/// ドラック&ドロップでオブジェクトを取得するGUIを表示するクラス
/// </summary>
public static class DragAndDropAreaUtility{

    /// <summary>
    /// ドラック&ドロップでオブジェクトを取得するGUI表示(取得してない時はnullが返る)
    /// </summary>
    public static T GetObject<T>(string areaTitle = "Drag & Drop", float widthMin = 0, float height = 50) where T : Object {
        //ドラックドロップされたオブジェクト取得
        var objectReferences = GetObjects(areaTitle, widthMin, height);

        //ドロップされたオブジェクトに対象の物があれば返す
        return objectReferences?.FirstOrDefault(o => o is T) as T;
    }

    /// <summary>
    /// ドラック&ドロップで複数のオブジェクトを取得するGUI表示(取得した時だけtrueが返る、取得した物はtargetListにaddされる)
    /// </summary>
    public static bool GetObjects<T>(List<T> targetList, string areaTitle = "Drag & Drop", float widthMin = 0, float height = 50) where T : Object {
        //ドラックドロップされたオブジェクトがなければスルー
        var objectReferences = GetObjects(areaTitle, widthMin, height);
        if (objectReferences == null) {
        return false;
        }
        
        //ドロップされたオブジェクトに対象の物が無ければスルー
        var targetObjectReferences = objectReferences.OfType<T>().ToList();
        if (targetObjectReferences.Count == 0) {
        return false;
        }
        
        //対象を登録
        targetList.AddRange(targetObjectReferences);
        return true;
    }

    //ドラックドロップで複数のオブジェクトを取得するGUI表示(取得してない時はnullが返る)
    private static Object[] GetObjects(string areaTitle = "Drag & Drop", float widthMin = 0, float height = 50){
        //D&D出来る場所を描画
        var dropArea = GUILayoutUtility.GetRect(widthMin, height, GUILayout.ExpandWidth(true));
        DrawBoxLayout(dropArea, areaTitle, Color.white);
        
        //マウスの位置がD&Dの範囲になければスルー
        if (!dropArea.Contains (Event.current.mousePosition)) {
        return null;
        }

        //現在のイベントを取得
        var eventType = Event.current.type;

        //ドラッグ＆ドロップで操作が更新された時でも、実行した時でもなければスルー
        if(eventType != EventType.DragUpdated && eventType != EventType.DragPerform) {
        return null;
        }

        //カーソルに+のアイコンを表示
        DragAndDrop.visualMode = DragAndDropVisualMode.Copy;

        //ドラッグ＆ドロップで無ければスルー
        if(eventType != EventType.DragPerform){
        return null;
        }
        
        //ドラッグを受け付ける(ドラッグしてカーソルにくっ付いてたオブジェクトが戻らなくなるので)
        DragAndDrop.AcceptDrag ();

        //イベントを使用済みにする
        Event.current.Use();
        
        return DragAndDrop.objectReferences;
    }

    static void DrawBoxLayout(Rect position, string areaTitle, Color color)
    {
        GUI.Box(position, areaTitle);
        var originalColor = GUI.color;

        // Alpha値を小さくしないと文字が見えないので下げる
        GUI.color = new Color(color.r, color.g, color.b, 0.2f);
        var style = new GUIStyle
        {
            normal =
            {
                background = Texture2D.whiteTexture
            }
        };
        GUI.Box(position, string.Empty, style);

        GUI.color = originalColor;
    }
}