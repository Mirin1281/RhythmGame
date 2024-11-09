using UnityEngine;
using UnityEditor;
using UnityEditor.AddressableAssets;
using System.Collections.Generic;
using NoteGenerating;

[CustomEditor(typeof(MusicSelectData))]
public class MusicSelectDataEditor : Editor
{
    readonly List<FumenData> _datas = new();

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        GUILayout.Space(20);

        if(DragAndDropAreaUtility.GetObjects(_datas, "譜面データをドラッグ&ドロップ", height: 50))
        {
            var settings = AddressableAssetSettingsDefaultObject.Settings;
            var group = settings.DefaultGroup;
            foreach (var d in _datas)
            {
                if (AssetDatabase.TryGetGUIDAndLocalFileIdentifier(d, out var guid, out long _) == false) continue;
                
                if(d.name.StartsWith("F_", System.StringComparison.Ordinal) == false)
                {
                    Debug.LogWarning("譜面データの名前は原則として\"F_\"で始めてください");
                }

                Difficulty difficulty = d.name[2] switch
                {
                    'N' => Difficulty.Normal,
                    'H' => Difficulty.Hard,
                    'E' => Difficulty.Extra,
                    _ => Difficulty.None
                };
                if(difficulty == Difficulty.None)
                {
                    Debug.LogWarning($"名前がフォーマットに合わなかったためキャンセルされました\n{d.name}");
                    continue;
                }

                var self = target as MusicSelectData;
                var serializedObject = new SerializedObject(self);
                serializedObject.Update();
                var levelProp = difficulty switch
                {
                    Difficulty.Normal => serializedObject.FindProperty("level_normal"),
                    Difficulty.Hard => serializedObject.FindProperty("level_hard"),
                    Difficulty.Extra => serializedObject.FindProperty("level_extra"),
                    _ => throw new System.Exception()
                };
                if(d.Level <= 0)
                {
                    levelProp.intValue = d.Level;
                    serializedObject.ApplyModifiedProperties();
                }
                
                if(group.GetAssetEntry(guid) == null)
                {
                    Debug.LogError("アドレスを取得できませんでした");
                }
                else
                {
                    string address = group.GetAssetEntry(guid).address;
                    self.SetFumenAddress(address, difficulty);
                    EditorUtility.SetDirty(target);
                    AssetDatabase.SaveAssetIfDirty(target);
                }
            }
        }        
    }
}
