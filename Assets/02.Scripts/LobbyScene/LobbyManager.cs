using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.IO;

public class LobbyManager : MonoBehaviour
{
    [SerializeField] List<Text> SelectionTexts;
    [SerializeField] GameObject LoadingPanel;
    [SerializeField] Image LoadingBar;
    [SerializeField] Text LoadingText;
    [SerializeField] GameObject Tutorial_Panel;
    [SerializeField] ConfigPanel configPanel;
    int currentSelection = 0;
    // Start is called before the first frame update
    void Start()
    {
        bool isFirstPlay = !PlayerPrefs.HasKey("FirstPlay");

        if (isFirstPlay)
        {
            Tutorial_Panel.SetActive(true);
        }

        configPanel.StartSetting();
    }

    // Update is called once per frame
    void Update()
    {
        if (Tutorial_Panel.activeSelf && Input.GetKeyDown(KeyCode.Backspace))
        {
            OnTutorialFinished();

        }

        if (!configPanel.gameObject.activeSelf)
        {
            HandleActionSelection();
        }
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (configPanel.gameObject.activeSelf)
            {
                if (configPanel.state == ConfigState.Config_Right)
                {
                    if (Input.GetKeyDown(KeyCode.Escape))
                    {
                        configPanel.gameObject.SetActive(false);
                    }
                }
            }
            else
            {
                SavingSystem.Instance.LoadGame();
                configPanel.gameObject.SetActive(true);
            }
        }
    }
    void OnTutorialFinished()
    {
        PlayerPrefs.SetInt("FirstPlay", 1);
        PlayerPrefs.Save();
        Tutorial_Panel.SetActive(false);
    }
    void HandleActionSelection()
    {
        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            currentSelection++;
            if (currentSelection > 4)
            {
                currentSelection = 0;
            }
        }
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            currentSelection--;
            if (currentSelection < 0)
            {
                currentSelection = 4;
            }
        }
        UpdateActionSelection(currentSelection);
        if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return))
        {
            if (currentSelection == 0)
            {
                string savePath = Path.Combine(Application.persistentDataPath, "save.json");

                if (File.Exists(savePath))
                {
                    FindObjectOfType<SavingSystem>().LoadGame();
                    var playerCtrl = FindObjectOfType<PlayerCtrl>();

                    // 2. party 또는 party.Party가 null/비었는지 확인
                    if (playerCtrl == null || playerCtrl.party == null || playerCtrl.party.Party == null || playerCtrl.party.Party.Count == 0)
                    {
                        Debug.Log("세이브에는 파티가 없습니다. 로딩하지 않습니다.");
                        return;
                    }
                    LoadingManager.LoadScene("BattleScene");
                    // SceneManager.LoadScene("BattleScene");
                }
                else
                {
                    Debug.LogWarning("저장 파일이 없어요… 새 게임을 시작해야 해요!");
                    // 선택지: NewGame으로 전환할 수도 있음
                }
            }
            if (currentSelection == 1)  //NewGame
            {
                SavingSystem.Instance.LoadGame();
                GlobalValue.UserGold = 0;
                GlobalValue.CurStage = 1;
                PlayerCtrl player = FindObjectOfType<PlayerCtrl>();
                if (player != null)
                {
                    player.GetComponent<PokemonParty>().Party.Clear();
                }
                GlobalValue.SetBasicStartPokemon();
                LoadingManager.LoadScene("PartyScene");
            }
            if (currentSelection == 2)  //LoadGame
            {
                Debug.Log("LoadGame");
            }
            if (currentSelection == 3)
            {
                SavingSystem.Instance.LoadGame();
                configPanel.state = ConfigState.Setting;
                configPanel.SettingPanel.SetActive(true);
                configPanel.gameObject.SetActive(true);
            }
            if (currentSelection == 4)
            {
                Application.Quit();
            }
        }
    }
    public void UpdateActionSelection(int selectedAction)
    {
        for (int i = 0; i < SelectionTexts.Count; i++)
        {
            if (i == selectedAction)
            {
                SelectionTexts[i].color = Color.cyan;
            }
            else
            {
                SelectionTexts[i].color = Color.white;
            }
        }
    }
}
