using System.IO;
using UnityEngine;

public class SavingSystem : MonoBehaviour
{
    const string SaveFileName = "save.json";

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
            // Debug.Log("[SavingSystem] 불러오기 완료!");
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
}
