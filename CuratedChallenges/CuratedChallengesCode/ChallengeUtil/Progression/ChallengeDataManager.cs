using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using CuratedChallenges.ChallengeUtil.Progression;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Saves;

namespace CuratedChallenges.ChallengeUtil;

public static class ChallengeDataManager
{
    private static ChallengeRunData? _cachedData;
    private const string FILENAME = "challenge_data.json";
    
    private static readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
    {
        WriteIndented = true,
        TypeInfoResolver = new DefaultJsonTypeInfoResolver()
    };

    public static void ClearCache()
    {
        _cachedData = null;
    }

    public static ChallengeRunData GetData()
    {
        if (_cachedData == null)
            _cachedData = LoadData();
        return _cachedData;
    }

    private static ChallengeRunData LoadData()
    {
        string path = SaveManager.Instance.GetProfileScopedPath(FILENAME);
        
        if (!Godot.FileAccess.FileExists(path))
        {
            return new ChallengeRunData();
        }
            
        using var file = Godot.FileAccess.Open(path, Godot.FileAccess.ModeFlags.Read);
        string json = file.GetAsText();
        
        return JsonSerializer.Deserialize<ChallengeRunData>(json, _jsonOptions) ?? new ChallengeRunData();
    }

    public static void SaveData()
    {
        if (_cachedData == null) return;
        
        string path = SaveManager.Instance.GetProfileScopedPath(FILENAME);
        string json = JsonSerializer.Serialize(_cachedData, _jsonOptions);
        
        using var file = Godot.FileAccess.Open(path, Godot.FileAccess.ModeFlags.Write);
        file.StoreString(json);
    }
}