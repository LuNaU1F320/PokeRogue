using System.IO;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public static class SaveSystem
{
    private static string savePath = Path.Combine(Application.persistentDataPath, "save.json");

    public static void SaveGame(List<Pokemon> party, int stageIndex)
    {
        var saveData = new SaveData
        {
            party = party.Select(p => p.GetSaveData()).ToList(),
            stageIndex = stageIndex
        };

        string json = JsonUtility.ToJson(saveData, true);
        File.WriteAllText(savePath, json);
        Debug.Log($"저장됨: {savePath}");
    }

    public static SaveData LoadGame()
    {
        if (!File.Exists(savePath))
        {
            Debug.LogWarning("저장 파일 없음");
            return null;
        }

        string json = File.ReadAllText(savePath);
        SaveData saveData = JsonUtility.FromJson<SaveData>(json);
        return saveData;
    }
}
