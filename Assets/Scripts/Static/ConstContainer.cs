using UnityEngine;

public static class ConstContainer
{
    public static readonly string InGameSceneName = "InGame";
    public static readonly string TestSceneName = "FumenTest";

    public static readonly string ScoreDataName = "ScoreData";
    public static readonly string GameDataName = "GameData";

    public static readonly string NoteSECategory = "NoteSE";
    public static readonly string SECategory = "SE";

    public static readonly string InvertColorMaterialPath = "InvertColor_Camera";
    public static readonly string NegativeMaterialPath = "Negative";

    public static readonly Vector2 ScreenSize = new Vector2(1920, 1080);
}

public static class FumenPathContainer
{
    public const string NoteCreate = "Genericノーツ/";
    public const string SpecificRoot = "--特殊/";
}