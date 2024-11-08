
using UnityEngine;

public static class ConstContainer
{
    public static readonly string DATA_PATH = "Assets/Scriptable/GeneratorData";

    public static readonly string InGameSceneName = "InGame";
    public static readonly string TestSceneName = "FumenTest";

    public static readonly string ScoreDataName = "ScoreData";
    public static readonly string GameDataName = "GameData";

    public static readonly string NoteSECategory = "NoteSE";
    public static readonly string SECategory = "SE";

    public static readonly Color UnNoteCommandColor = new Color(0.8f, 0.8f, 0.9f);
    public static readonly Color LineCommandColor = new Color(0.95f, 0.8f, 0.85f);
    public static readonly Color NoteCommandColor = new Color32(255, 226, 200, 255);

    public static readonly Vector2 ScreenSize = new Vector2(1920, 1080);
}
