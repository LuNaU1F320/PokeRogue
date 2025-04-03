using System.IO;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SavingSystem : MonoBehaviour
{
    public static SavingSystem Instance { get; private set; }

    const string SaveFileName = "save.json";
    string configPath => Path.Combine(Application.persistentDataPath, "config.json");

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);  // 중복 제거
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }


    public void SaveGame()
    {
        var player = FindObjectOfType<PlayerCtrl>();
        if (player == null)
        {
            Debug.LogWarning("[SavingSystem] PlayerCtrl을 찾을 수 없어요…");
            return;
        }

        var data = player.CaptureState();
        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(GetSavePath(), json);

        Debug.Log($"[SavingSystem] 저장 완료: {GetSavePath()}");
    }

    public void LoadGame()
    {
        string path = GetSavePath();
        if (!File.Exists(path))
        {
            Debug.LogWarning("[SavingSystem] 저장 파일이 없어요…");
            return;
        }

        string json = File.ReadAllText(path);
        var data = JsonUtility.FromJson<PlayerSaveData>(json);

        var player = FindObjectOfType<PlayerCtrl>();
        if (player != null)
        {
            player.RestoreState(data);
            Debug.Log("[SavingSystem] 불러오기 완료!");
        }
        else
        {
            Debug.LogWarning("[SavingSystem] PlayerCtrl을 찾지 못했어요…!");
        }
    }

    string GetSavePath()
    {
        return Path.Combine(Application.persistentDataPath, SaveFileName);
    }

    #region Config
    public void SaveConfig(List<int> selectedOptions)
    {
        ConfigData configData = new ConfigData { selectedOptions = selectedOptions };
        string json = JsonUtility.ToJson(configData, true);

        File.WriteAllText(configPath, json);
        Debug.Log("[SavingSystem] 설정 저장 완료!");
    }

    public List<int> LoadConfig(int optionCount)
    {
        if (!File.Exists(configPath))
        {
            Debug.LogWarning("[SavingSystem] 설정 파일이 없어요… 기본값으로 불러올게요…");
            return Enumerable.Repeat(0, optionCount).ToList();
        }

        string json = File.ReadAllText(configPath);
        ConfigData configData = JsonUtility.FromJson<ConfigData>(json);

        // 혹시 저장된 옵션 개수가 부족할 경우 대비…
        while (configData.selectedOptions.Count < optionCount)
            configData.selectedOptions.Add(0);

        return configData.selectedOptions;
    }
    #endregion
}
[System.Serializable]
class ConfigData
{
    public List<int> selectedOptions = new();
}