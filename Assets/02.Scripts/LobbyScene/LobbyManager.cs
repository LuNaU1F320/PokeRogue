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
    int currentSelection = 0;
    void Awake()
    {
        // SkillDB.Init();
        // PokemonDB.Init();
    }
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        HandleActionSelection();
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
            if (currentSelection == 0)  //Continue
            {
                // Debug.Log("Continue");
                string savePath = Path.Combine(Application.persistentDataPath, "save.json");

                if (File.Exists(savePath))
                {
                    FindObjectOfType<SavingSystem>().LoadGame();
                    LoadingManager.LoadScene("BattleScene");
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
            if (currentSelection == 3)  //DailyRun
            {
                Debug.Log("DailyRun");
            }
            if (currentSelection == 4)  //Settings
            {
                Debug.Log("Settings");
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

    private IEnumerator LoadGameSceneAsync(string SceneName)
    {
        LoadingPanel.SetActive(true);

        AsyncOperation asyncOperation = SceneManager.LoadSceneAsync(SceneName);

        float Progress = asyncOperation.progress;

        int ProgressPercent = Mathf.RoundToInt(Progress * 100f);

        while (!asyncOperation.isDone)
        {
            LoadingBar.fillAmount = ProgressPercent / 100;
            LoadingText.text = ProgressPercent.ToString();
            yield return null;
        }

        LoadingPanel.SetActive(false);
    }
    IEnumerator LoadScene(string SceneName)
    {

        LoadingPanel.SetActive(true);
        yield return null;
        AsyncOperation op = SceneManager.LoadSceneAsync(SceneName);
        op.allowSceneActivation = false;
        float timer = 0.0f;
        while (!op.isDone)
        {
            yield return null;
            timer += Time.deltaTime;
            if (op.progress < 0.9f)
            {
                LoadingBar.fillAmount = Mathf.Lerp(LoadingBar.fillAmount, op.progress, timer);
                LoadingText.text = op.progress.ToString();
                if (LoadingBar.fillAmount >= op.progress)
                {
                    timer = 0f;
                }
            }
            else
            {
                LoadingBar.fillAmount = Mathf.Lerp(LoadingBar.fillAmount, 1f, timer);
                if (LoadingBar.fillAmount == 1.0f)
                {
                    op.allowSceneActivation = true;
                    yield break;
                }
            }
        }

        LoadingPanel.SetActive(false);
    }
}
