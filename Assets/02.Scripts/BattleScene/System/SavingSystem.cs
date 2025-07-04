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
        InitializeGlobalConfig();
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

            // Debug.Log($"[SavingSystem] MyPokemon 복원 결과: {GlobalValue.MyPokemon?.Count ?? -1}개");

            // if (GlobalValue.MyPokemon != null)
            // {
            //     foreach (var kvp in GlobalValue.MyPokemon)
            //     {
            //         Debug.Log($" - ID: {kvp.Key}, IsShiny: {kvp.Value.IsShiny}");
            //     }
            // }
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
    public void SaveConfig(int tabIndex, List<int> selectedOptions)
    {
        ConfigDataSet dataSet;

        if (File.Exists(configPath))
        {
            string existingJson = File.ReadAllText(configPath);
            dataSet = JsonUtility.FromJson<ConfigDataSet>(existingJson);
        }
        else
        {
            dataSet = new ConfigDataSet();
        }

        while (dataSet.tabOptions.Count <= tabIndex)
        {
            dataSet.tabOptions.Add(new TabOptionData());
        }

        dataSet.tabOptions[tabIndex].selectedOptions = new List<int>(selectedOptions);

        // 로그 확인
        Debug.Log($"[SavingSystem] tabOptions.Count = {dataSet.tabOptions.Count}");
        for (int i = 0; i < dataSet.tabOptions.Count; i++)
        {
            Debug.Log($"탭 {i} 옵션: {string.Join(", ", dataSet.tabOptions[i].selectedOptions)}");
        }

        string json = JsonUtility.ToJson(dataSet, true);
        File.WriteAllText(configPath, json);
        Debug.Log($"[SavingSystem] 탭 {tabIndex} 설정 저장 완료!");
    }

    public List<int> LoadConfig(int tabIndex, int optionCount)
    {
        if (!File.Exists(configPath))
        {
            Debug.LogWarning("[SavingSystem] 설정 파일이 없어요… 기본값으로 불러올게요.");
            return Enumerable.Repeat(0, optionCount).ToList();
        }

        string json = File.ReadAllText(configPath);
        ConfigDataSet dataSet = JsonUtility.FromJson<ConfigDataSet>(json);

        if (tabIndex >= dataSet.tabOptions.Count)
        {
            Debug.LogWarning($"[SavingSystem] 탭 {tabIndex} 설정 없음. 기본값으로 불러옵니다.");
            return Enumerable.Repeat(0, optionCount).ToList();
        }

        List<int> loaded = dataSet.tabOptions[tabIndex].selectedOptions;

        while (loaded.Count < optionCount)
            loaded.Add(0);

        return loaded;
    }
    #endregion

    private void InitializeGlobalConfig()
    {
        float[] volumeLevels = { 0.0f, 0.1f, 0.2f, 0.3f, 0.4f, 0.5f, 0.6f, 0.7f, 0.8f, 0.9f, 1.0f };
        float[] gameSpeeds = { 1f, 1.5f, 2f, 2.5f, 3f, 4f, 5f };
        float[] hpBarSpeeds = { 0.5f, 1.0f, 2.0f, 10.0f };
        float[] expBarSpeeds = { 1.0f, 1.5f, 2.0f, 10.0f };

        // 🎮 게임 설정 (탭 0)
        var gameOptions = SavingSystem.Instance.LoadConfig(0, 3);
        GlobalValue.GameSpeed = gameSpeeds[Mathf.Clamp(gameOptions[0], 0, gameSpeeds.Length - 1)];
        GlobalValue.HpBarSpeed = hpBarSpeeds[Mathf.Clamp(gameOptions[1], 0, hpBarSpeeds.Length - 1)];
        GlobalValue.ExpBarSpeed = expBarSpeeds[Mathf.Clamp(gameOptions[2], 0, expBarSpeeds.Length - 1)];

        // 🔊 오디오 설정 (탭 2)
        var audioOptions = SavingSystem.Instance.LoadConfig(2, 3);
        GlobalValue.MasterVolume = volumeLevels[Mathf.Clamp(audioOptions[0], 0, volumeLevels.Length - 1)];
        GlobalValue.BGMVolume = volumeLevels[Mathf.Clamp(audioOptions[1], 0, volumeLevels.Length - 1)];
        GlobalValue.UIVolume = volumeLevels[Mathf.Clamp(audioOptions[2], 0, volumeLevels.Length - 1)];

        // 🔁 즉시 사운드 반영
        if (Sound_Manager.Instance != null)
        {
            Sound_Manager.Instance.SoundVolume();
        }
    }
}
[System.Serializable]
public class TabOptionData
{
    public List<int> selectedOptions = new();
}
[System.Serializable]
public class ConfigDataSet
{
    public List<TabOptionData> tabOptions = new();
}

