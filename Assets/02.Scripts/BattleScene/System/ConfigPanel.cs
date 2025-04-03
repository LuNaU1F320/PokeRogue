using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public enum ConfigState
{
    Config_Right,
    Setting
}
public class ConfigPanel : MonoBehaviour
{
    [HideInInspector] public ConfigState state = ConfigState.Config_Right;
    [SerializeField] public GameObject Panel;
    [SerializeField] Color highlightedColor;
    // [SerializeField] public GameObject Config_Right;
    [SerializeField] List<Text> configTexts;
    [SerializeField] GameObject SettingPanel;
    [SerializeField] private Image selectionFrame; // 현재 선택된 행을 강조할 테두리 이미지

    [SerializeField] private List<Transform> configRows;
    private List<List<Text>> configOptions = new List<List<Text>>();
    private List<int> selectedOptions = new List<int>(); // 각 행별로 선택된 옵션 인덱스 저장
    private int currentRow = 0;

    void Start()
    {
        Panel.SetActive(false);
        SettingPanel.SetActive(false);
        InitializeConfigOptions();
        LoadOptions();
        UpdateSelection();
    }
    public void Update()
    {
        if (state == ConfigState.Setting && SettingPanel.activeSelf)
        {
            if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                MoveSelection(-1); // 위로 이동
            }
            else if (Input.GetKeyDown(KeyCode.DownArrow))
            {
                MoveSelection(1); // 아래로 이동
            }
            else if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                ChangeOption(-1); // 왼쪽으로 옵션 변경
            }
            else if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                ChangeOption(1); // 오른쪽으로 옵션 변경
            }

            if (Input.GetKey(KeyCode.Backspace))
            {
                // SaveOptions(); // Backspace 눌렀을 때 값 저장
                // GlobalValue.SaveSetting();
                SaveOptions();
                SettingPanel.SetActive(false);
                state = ConfigState.Config_Right;
            }
        }
    }
    private void InitializeConfigOptions()
    {
        configOptions.Clear();
        selectedOptions.Clear();

        foreach (Transform row in configRows) // 각 행을 순회
        {
            List<Text> rowOptions = new List<Text>();
            foreach (Transform option in row) // 행 내 모든 옵션을 찾아 추가
            {
                Text text = option.GetComponent<Text>();
                if (text != null)
                {
                    rowOptions.Add(text);
                }
            }
            configOptions.Add(rowOptions);
            selectedOptions.Add(0); // 초기 선택값을 첫 번째 옵션으로 설정
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

        UpdateSelection(); // UI 업데이트
    }

    private void ChangeOption(int direction)
    {
        int maxIndex = configOptions[currentRow].Count - 1;
        selectedOptions[currentRow] += direction;
        selectedOptions[currentRow] = Mathf.Clamp(selectedOptions[currentRow], 0, maxIndex);

        if (currentRow == 0)
        {
            ChangeGameSpeed(selectedOptions[currentRow]);
        }
        else if (currentRow == 1)
        {
            ChangeHpBarSpeed(selectedOptions[currentRow]);
        }
        else if (currentRow == 2)
        {
            ChangeExpBarSpeed(selectedOptions[currentRow]);
        }

        UpdateSelection(); // UI 업데이트
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
    private void SaveOptions()
    {
        // GlobalValue.SaveSetting(selectedOptions);
        SavingSystem.Instance.SaveConfig(selectedOptions);
    }
    private void LoadOptions()
    {
        List<int> loaded = SavingSystem.Instance.LoadConfig(configRows.Count);

        // List<int> loaded = GlobalValue.LoadSetting(configRows.Count);
        for (int i = 0; i < loaded.Count; i++)
        {
            selectedOptions[i] = loaded[i];
        }
        ChangeGameSpeed(selectedOptions[0]);
        ChangeHpBarSpeed(selectedOptions[1]);
        ChangeExpBarSpeed(selectedOptions[2]);
        UpdateSelection();
    }
}
