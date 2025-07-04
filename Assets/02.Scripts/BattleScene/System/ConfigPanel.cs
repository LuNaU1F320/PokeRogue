using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
public enum ConfigState
{
    Config_Right,
    Setting
}
public class ConfigPanel : MonoBehaviour
{
    public static ConfigPanel Inst;
    [HideInInspector] public ConfigState state = ConfigState.Config_Right;
    [SerializeField] Color highlightedColor;
    Color OriginColor = new Color32(250, 180, 50, 255);
    [SerializeField] List<Text> configTexts;
    [SerializeField] GameObject Config_Right;
    [SerializeField] public GameObject SettingPanel;
    [SerializeField] private Image selectionFrame;
    [SerializeField] private List<Text> tabText;
    [SerializeField] private List<GameObject> configTabs; // 일반/디스플레이/오디오 등 탭들
    private int currentTab = 0;

    [SerializeField] private List<Transform> configRowsParents; // 탭별로 하나씩
    private List<Transform> configRows = new List<Transform>();
    private List<List<Text>> configOptions = new List<List<Text>>();
    private List<int> selectedOptions = new List<int>(); // 각 행별로 선택된 옵션 인덱스 저장
    private int currentConfig;
    private int currentRow = 0;

    [HideInInspector] public PlayerCtrl playerCtrl;

    void Awake()
    {
        Inst = this;
        playerCtrl = FindObjectOfType<PlayerCtrl>();
    }
    public void StartSetting()
    {
        state = ConfigState.Config_Right;
        ChangeTab(0);
    }
    public void Update()
    {
        if (state == ConfigState.Config_Right)
        {
            HandleConfigSelection();
        }
        if (state == ConfigState.Setting && SettingPanel.activeSelf)
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                Sound_Manager.Instance.PlayGUISound("UI/menu_open");
                SettingPanel.SetActive(false);
                state = ConfigState.Config_Right;
                this.gameObject.SetActive(false);
            }
            if (Input.GetKeyDown(KeyCode.F))
            {
                Sound_Manager.Instance.PlayGUISound("UI/select");
                SaveOptions();
                ChangeTab(-1); // 이전 탭
            }
            else if (Input.GetKeyDown(KeyCode.R))
            {
                Sound_Manager.Instance.PlayGUISound("UI/select");
                SaveOptions();
                ChangeTab(1); // 다음 탭
            }

            if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                MoveSelection(-1);
            }
            else if (Input.GetKeyDown(KeyCode.DownArrow))
            {
                MoveSelection(1);
            }
            else if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                ChangeOption(-1);
            }
            else if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                ChangeOption(1);
            }

            // if (Input.GetKey(KeyCode.Backspace))
            // {
            //     SaveOptions();
            //     Sound_Manager.Instance.PlayGUISound("UI/menu_open");
            //     SettingPanel.SetActive(false);
            //     state = ConfigState.Config_Right;
            // }
            if (Input.GetKeyDown(KeyCode.Backspace))
            {
                if (currentTab == 1)
                {
                    int fullscreenIndex = Mathf.Clamp(selectedOptions[0], 0, 2);
                    FullScreenMode newMode = FullScreenMode.Windowed;
                    switch (fullscreenIndex)
                    {
                        case 0: newMode = FullScreenMode.Windowed; break;
                        case 1: newMode = FullScreenMode.ExclusiveFullScreen; break;
                        case 2: newMode = FullScreenMode.FullScreenWindow; break;
                    }

                    // 해상도 선택: 전체화면 모드에서는 currentResolutionIndex, 창 모드에서는 selectedOptions[1]
                    int resolutionIndex = (newMode == FullScreenMode.Windowed) ? selectedOptions[1] : currentResolutionIndex;
                    resolutionIndex = Mathf.Clamp(resolutionIndex, 0, resolutionOptions.Length - 1);
                    Vector2Int res = resolutionOptions[resolutionIndex];

                    Screen.SetResolution(res.x, res.y, newMode);
                    Debug.Log($"[ConfigPanel] 수동 적용: {res.x}x{res.y}, 모드: {newMode}");
                }
                // else
                {
                    for (int i = 0; i < selectedOptions.Count; i++)
                    {
                        ApplyOptionValue(currentTab, i, selectedOptions[i]);
                    }
                }

                SaveOptions();
                Sound_Manager.Instance.PlayGUISound("UI/menu_open");
                SettingPanel.SetActive(false);
                state = ConfigState.Config_Right;
            }
            if (Input.GetKeyDown(KeyCode.Space))
            {
                if (currentTab == 1)
                {
                    int fullscreenIndex = Mathf.Clamp(selectedOptions[0], 0, 2);
                    FullScreenMode newMode = FullScreenMode.Windowed;
                    switch (fullscreenIndex)
                    {
                        case 0: newMode = FullScreenMode.Windowed; break;
                        case 1: newMode = FullScreenMode.ExclusiveFullScreen; break;
                        case 2: newMode = FullScreenMode.FullScreenWindow; break;
                    }

                    // 해상도 선택: 전체화면 모드에서는 currentResolutionIndex, 창 모드에서는 selectedOptions[1]
                    int resolutionIndex = (newMode == FullScreenMode.Windowed) ? selectedOptions[1] : currentResolutionIndex;
                    resolutionIndex = Mathf.Clamp(resolutionIndex, 0, resolutionOptions.Length - 1);
                    Vector2Int res = resolutionOptions[resolutionIndex];

                    Screen.SetResolution(res.x, res.y, newMode);
                    Debug.Log($"[ConfigPanel] 수동 적용: {res.x}x{res.y}, 모드: {newMode}");
                }
                // else
                {
                    for (int i = 0; i < selectedOptions.Count; i++)
                    {
                        ApplyOptionValue(currentTab, i, selectedOptions[i]);
                    }
                }
                SaveOptions();
            }
        }
    }
    private void ChangeTab(int direction)
    {
        currentTab += direction;
        currentTab = Mathf.Clamp(currentTab, 0, configTabs.Count - 1);

        for (int i = 0; i < configTabs.Count; i++)
        {
            bool isSelected = (i == currentTab);

            configTabs[i].SetActive(isSelected);
            tabText[i].color = isSelected ? highlightedColor : OriginColor;
        }
        InitializeConfigOptions();
        LoadOptions();
        UpdateSelection();
    }
    private void InitializeConfigOptions()
    {
        configOptions.Clear();
        selectedOptions.Clear();
        configRows.Clear();

        var parent = configRowsParents[currentTab];

        for (int i = 0; i < parent.childCount; i++)
        {
            Transform row = parent.GetChild(i);
            configRows.Add(row);

            List<Text> rowOptions = new List<Text>();
            foreach (Transform option in row)
            {
                Text text = option.GetComponent<Text>();
                if (text != null)
                {
                    rowOptions.Add(text);
                }
            }

            configOptions.Add(rowOptions);
            selectedOptions.Add(0); // 각 행의 기본 선택값 0
        }
    }
    private void UpdateSelection()
    {
        for (int i = 0; i < configOptions.Count; i++) // 행 반복
        {
            for (int j = 0; j < configOptions[i].Count; j++) // 열 반복
            {
                if (selectedOptions[i] == j) // 현재 선택된 옵션
                {
                    configOptions[i][j].color = highlightedColor;
                }
                else
                {
                    configOptions[i][j].color = Color.white;
                }
            }
        }
    }
    private void MoveSelection(int direction)
    {
        currentRow += direction;
        currentRow = Mathf.Clamp(currentRow, 0, configOptions.Count - 1);
        Vector3 newPos = selectionFrame.transform.position;
        newPos.y = configRows[currentRow].position.y; // 선택된 행의 Y값을 가져옴
        selectionFrame.transform.position = newPos;
        Sound_Manager.Instance.PlayGUISound("UI/select");

        UpdateSelection(); // UI 업데이트
    }

    private void ChangeOption(int direction)
    {
        int maxIndex = configOptions[currentRow].Count - 1;
        selectedOptions[currentRow] += direction;
        selectedOptions[currentRow] = Mathf.Clamp(selectedOptions[currentRow], 0, maxIndex);
        Sound_Manager.Instance.PlayGUISound("UI/select");
        ApplyOptionValue(currentTab, currentRow, selectedOptions[currentRow]);
        UpdateSelection();
    }

    #region  SettingPanel
    private void ChangeGameSpeed(int optionIndex)
    {
        float[] speeds = { 1f, 1.5f, 2f, 2.5f, 3f, 4f, 5f };
        float selectedSpeed = speeds[Mathf.Clamp(optionIndex, 0, speeds.Length - 1)];

        GlobalValue.GameSpeed = selectedSpeed;
        Time.timeScale = selectedSpeed;
    }
    private void ChangeHpBarSpeed(int optionIndex)
    {
        float[] HpBarspeeds = { 0.5f, 1.0f, 2.0f, 10.0f };
        if (optionIndex >= 0 && optionIndex < HpBarspeeds.Length)
        {
            GlobalValue.HpBarSpeed = HpBarspeeds[optionIndex];
            HpBar.HpBarSpeed = GlobalValue.HpBarSpeed;
        }
    }
    private void ChangeExpBarSpeed(int optionIndex)
    {
        float[] speeds = { 1.0f, 1.5f, 2.0f, 10.0f };
        if (optionIndex >= 0 && optionIndex < speeds.Length)
        {
            GlobalValue.ExpBarSpeed = speeds[optionIndex];
        }
    }
    #endregion
    #region Display

    private int currentResolutionIndex = 0;
    private readonly Vector2Int[] resolutionOptions = {
    new Vector2Int(1920, 1080),
    new Vector2Int(1280, 720),
    new Vector2Int(854, 480),
    new Vector2Int(640, 360)
};

    // private void ChangeFullscreenMode(int index)
    // {
    //     index = Mathf.Clamp(index, 0, 2);

    //     FullScreenMode newMode = FullScreenMode.Windowed;

    //     switch (index)
    //     {
    //         case 0: newMode = FullScreenMode.Windowed; break;
    //         case 1: newMode = FullScreenMode.ExclusiveFullScreen; break;
    //         case 2: newMode = FullScreenMode.FullScreenWindow; break;
    //     }

    //     Vector2Int res = resolutionOptions[Mathf.Clamp(currentResolutionIndex, 0, resolutionOptions.Length - 1)];
    //     Screen.SetResolution(res.x, res.y, newMode);

    //     Debug.Log($"[ConfigPanel] 창 모드 설정됨 → {newMode} @ {res.x}x{res.y}");
    // }
    // private void ChangeResolution(int index)
    // {
    //     index = Mathf.Clamp(index, 0, resolutionOptions.Length - 1);
    //     currentResolutionIndex = index;

    //     if (Screen.fullScreenMode == FullScreenMode.Windowed)
    //     {
    //         Vector2Int res = resolutionOptions[index];
    //         Screen.SetResolution(res.x, res.y, FullScreenMode.Windowed);
    //         Debug.Log($"[ConfigPanel] 해상도 변경: {res.x}x{res.y}");
    //     }
    //     else
    //     {
    //         Debug.LogWarning("[ConfigPanel] 해상도는 창 모드에서만 변경할 수 있어요!");
    //     }
    // }
    #endregion
    #region Sound
    private readonly float[] volumeLevels = { 0.0f, 0.1f, 0.2f, 0.3f, 0.4f, 0.5f, 0.6f, 0.7f, 0.8f, 0.9f, 1.0f };

    private void ChangeMasterVolume(int index)
    {
        index = Mathf.Clamp(index, 0, volumeLevels.Length - 1);
        GlobalValue.MasterVolume = volumeLevels[index];
        Sound_Manager.Instance.SoundVolume();
    }

    private void ChangeBGMVolume(int index)
    {
        index = Mathf.Clamp(index, 0, volumeLevels.Length - 1);
        GlobalValue.BGMVolume = volumeLevels[index];
        Sound_Manager.Instance.SoundVolume();
    }

    private void ChangeUIVolume(int index)
    {
        index = Mathf.Clamp(index, 0, volumeLevels.Length - 1);
        GlobalValue.UIVolume = volumeLevels[index];
    }

    #endregion
    public void SettingSelection()
    {
        SettingPanel.SetActive(true);
        state = ConfigState.Setting;
    }

    public void ConfigSelection(int selectedAction)
    {
        for (int i = 0; i < configTexts.Count; i++)
        {
            if (i == selectedAction)
            {
                configTexts[i].color = highlightedColor;
            }
            else
            {
                configTexts[i].color = Color.white;
            }
        }
    }
    #region Config
    void HandleConfigSelection()
    {
        if (state == ConfigState.Config_Right)
        {
            if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                --currentConfig;
                Sound_Manager.Instance.PlayGUISound("UI/select");
            }
            if (Input.GetKeyDown(KeyCode.DownArrow))
            {
                ++currentConfig;
                Sound_Manager.Instance.PlayGUISound("UI/select");
            }
            currentConfig = Mathf.Clamp(currentConfig, 0, 5);
            ConfigSelection(currentConfig);
            if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return))
            {
                if (currentConfig == 0)
                {//게임설정
                    Sound_Manager.Instance.PlayGUISound("UI/menu_open");
                    SettingSelection();
                }
                else if (currentConfig == 1)
                {//도감
                    Sound_Manager.Instance.PlayGUISound("UI/select");
                    Debug.Log("도감");
                }
                else if (currentConfig == 2)
                {//데이터관리
                    Sound_Manager.Instance.PlayGUISound("UI/select");
                    Debug.Log("데이터 관리");
                }
                else if (currentConfig == 3)
                {//커뮤니티
                    Sound_Manager.Instance.PlayGUISound("UI/select");
                    Debug.Log("커뮤니티");
                }
                else if (currentConfig == 4)
                {//저장 후 나가기
                    Sound_Manager.Instance.PlayGUISound("UI/select");
                    var savingSystem = FindObjectOfType<SavingSystem>();
                    if (savingSystem != null)
                    {
                        savingSystem.SaveGame();
                        playerCtrl.party.Party.Clear();
                        SceneManager.LoadScene("LobbyScene");
                    }
                    else
                    {
                        Debug.LogWarning("SavingSystem을 찾지 못했어요… 저장 실패!");
                    }
                }
                else if (currentConfig == 5)
                {//로그아웃
                    Sound_Manager.Instance.PlayGUISound("UI/select");
                    Debug.Log("로그아웃");
                }
            }
        }
    }
    #endregion

    private void SaveOptions()
    {
        SavingSystem.Instance.SaveConfig(currentTab, selectedOptions);
    }
    private void LoadOptions()
    {
        List<int> loaded = SavingSystem.Instance.LoadConfig(currentTab, configRows.Count);

        for (int i = 0; i < Mathf.Min(selectedOptions.Count, loaded.Count); i++)
        {
            selectedOptions[i] = loaded[i];
        }

        // 설정 적용
        for (int i = 0; i < selectedOptions.Count; i++)
        {
            ApplyOptionValue(currentTab, i, selectedOptions[i]);
        }

        UpdateSelection();
    }
    private void ApplyOptionValue(int tab, int row, int value)
    {
        if (tab == 0) // 게임 설정
        {
            switch (row)
            {
                case 0: ChangeGameSpeed(value); break;
                case 1: ChangeHpBarSpeed(value); break;
                case 2: ChangeExpBarSpeed(value); break;
            }
        }
        else if (tab == 1) // 디스플레이 설정
        {
            switch (row)
            {
                // case 0: ChangeResolution(value); break;
                // case 1: ChangeFullscreen(value); break;
            }
        }
        else if (tab == 2) // 오디오 설정
        {
            switch (row)
            {
                case 0: ChangeMasterVolume(value); break;
                case 1: ChangeBGMVolume(value); break;
                case 2: ChangeUIVolume(value); break;
            }
        }
    }

}
