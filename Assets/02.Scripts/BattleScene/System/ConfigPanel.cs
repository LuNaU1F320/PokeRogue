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
    public ConfigState state = ConfigState.Config_Right;
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
        UpdateSelection(); // 처음 실행 시 선택된 항목 표시
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
                SaveOptions(); // Backspace 눌렀을 때 값 저장
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

        if (currentRow == 0) // 첫 번째 행이 게임 속도 설정
        {
            ChangeGameSpeed(selectedOptions[currentRow]);
        }
        else if (currentRow == 1)
        {
            ChangeHpBarSpeed(selectedOptions[currentRow]);
        }

        UpdateSelection(); // UI 업데이트
    }
    #region  SettingPanel
    // 게임 속도에 대한 설정 (0: 느리게, 1: 기본, 2: 빠르게)
    private void ChangeGameSpeed(int optionIndex)
    {
        switch (optionIndex)
        {
            case 0:
                Time.timeScale = 1f; // 기본 속도 (1배속)
                break;
            case 1:
                Time.timeScale = 1.5f;
                break;
            case 2:
                Time.timeScale = 2f;
                break;
            case 3:
                Time.timeScale = 2.5f;
                break;
            case 4:
                Time.timeScale = 3f;
                break;
            case 5:
                Time.timeScale = 4f;
                break;
            case 6:
                Time.timeScale = 5f;
                break;
            default:
                Time.timeScale = 1f;
                break;
        }
    }
    private void ChangeHpBarSpeed(int optionIndex)
    {
        switch (optionIndex)
        {
            case 0:
                HpBar.HpBarSpeed = 0.5f;
                break;
            case 1:
                HpBar.HpBarSpeed = 2.0f;
                break;
            case 2:
                HpBar.HpBarSpeed = 3.0f;
                break;
            case 3:
                Time.timeScale = 100f;
                break;
            default:
                Time.timeScale = 1f;
                break;
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
        for (int i = 0; i < selectedOptions.Count; i++)
        {
            PlayerPrefs.SetInt("Option_" + i, selectedOptions[i]);
        }
        PlayerPrefs.Save(); // 변경 사항 저장
    }

    // PlayerPrefs에서 값을 불러오기
    private void LoadOptions()
    {
        for (int i = 0; i < selectedOptions.Count; i++)
        {
            if (PlayerPrefs.HasKey("Option_" + i)) // 값이 존재하면 불러오기
            {
                selectedOptions[i] = PlayerPrefs.GetInt("Option_" + i);
            }
        }
    }

}
