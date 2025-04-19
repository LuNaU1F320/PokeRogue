using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Linq;

public enum GameState
{
    None,
    Battle,
    Dialog,
    Evolution,
    Busy
}

public class GameManager : MonoBehaviour
{
    public static GameManager Inst { get; private set; }
    [HideInInspector] public static BattleState state;
    BattleState preState;
    [HideInInspector] public bool isRun = true;


    [Header("System")]
    [SerializeField] BattleSystem battleSystem;
    [HideInInspector] public PlayerCtrl playerCtrl;
    [HideInInspector] PokemonParty playerParty;
    [SerializeField] MapArea mapArea;
    [SerializeField] public PartyScreen partyScreen;
    [SerializeField] public SkillSelectScreen skillSelectScreen;
    [SerializeField] ConfigPanel configPanel;
    [SerializeField] GameObject ConfirmBox;

    [Header("UI")]
    [SerializeField] Text Stage_Text;
    [SerializeField] Text Gold_Text;

    [Header("Selection")]
    [SerializeField] public BattleDialogBox dialogBox;
    [HideInInspector] int currentAction = 0;
    [HideInInspector] public int currentSkill = 0;
    [HideInInspector] public int currentMember = 0;
    [HideInInspector] public int currentSelection = 0;
    [HideInInspector] public int currentConfirm = 0;
    [HideInInspector] public int skillCount = 0;

    private void Awake()
    {
        Inst = this;
        Time.timeScale = GlobalValue.GameSpeed;
        playerCtrl = FindObjectOfType<PlayerCtrl>();
    }

    void Start()
    {
        playerParty = playerCtrl.Party;
        if (battleSystem == null || mapArea == null)
        {
            return;
        }
        if (GlobalValue.CurStage == 0)
        {
            GlobalValue.CurStage = 1;
        }
        GlobalValue.UserGold = 1000;
        Stage_Text.text = $"마을 - {GlobalValue.CurStage}";
        Gold_Text.text = $"￡{GlobalValue.UserGold}";


        currentAction = 0;
        configPanel.StartSetting();
        StartBattle();
    }
    void Update()
    {
        Debug.Log(state);
        if (state == BattleState.BattleOver || state == BattleState.Evolution)
        {
            return;
        }
        if (state == BattleState.Action)
        {
            HandleActionSelection();
        }
        else if (state == BattleState.Skill)
        {
            HandleSkillSelection();
        }
        else if (state == BattleState.PartyScreen)
        {
            HandlePartyScreenSelection();
        }
        else if (state == BattleState.SkillToForget)
        {
            HandleLearnSkillSelection();
        }
        else if (state == BattleState.ConfirmBox)
        {
            HandleConfirmBoxSelection();
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (state == BattleState.ConfigSelection)
            {
                if (configPanel.state == ConfigState.Config_Right)
                {
                    if (Input.GetKeyDown(KeyCode.Escape))
                    {
                        configPanel.gameObject.SetActive(false);
                        state = preState;
                    }
                }
            }
            else
            {
                configPanel.gameObject.SetActive(true);
                preState = state;
                state = BattleState.ConfigSelection;
            }
        }
        // if (Input.GetKeyDown(KeyCode.G))
        // {
        //     Debug.Log(currentMember);
        // }
        // if (Input.GetKeyDown(KeyCode.F))
        // {
        //     Debug.Log(playerUnit.BattlePokemon.Attack);
        //     Debug.Log(playerUnit.BattlePokemon.Rankup[0]);
        // }
        // if (Input.GetKeyDown(KeyCode.T))
        // {
        //     Debug.Log($"{playerUnit.BattlePokemon.PokemonGen}");
        // }
    }
    void StartBattle()
    {
        var wildPokemon = mapArea.GetRandomWildPokemon();
        var refWildPokemon = new Pokemon(wildPokemon.P_Base, wildPokemon.PokemonLevel);

        Sound_Manager.Instance.PlayBGM("BGM/plains");
        battleSystem.StartBattle(playerCtrl.Party, refWildPokemon);
    }
    // public void StartTrainerBattle(TrainerCtrl trainer)
    // {
    //     battleSystem.gameObject.SetActive(true);

    //     var playerParty = GetComponent<PokemonParty>();
    //     var trainerParty = trainer.GetComponent<PokemonParty>();

    //     battleSystem.StartTrainerBattle(playerParty, trainerParty);
    // }
    public void EndBattle(bool won)
    {
        if (won)
        {
            // StopAllCoroutines();
            state = BattleState.None;
            StopAllCoroutines(); // 🔥 여기서도 한 번 더 안전망
            GlobalValue.CurStage++;
            Stage_Text.text = $"마을 - {GlobalValue.CurStage}";
            // StartBattle();
            StartCoroutine(StartNextBattleWithDelay());
        }
        else
        {
            Debug.Log("전투에서 패배했습니다...");
            playerCtrl.party.Party.Clear();
            GlobalValue.CurStage = 1;
            GlobalValue.UserGold = 1000;
            playerCtrl.CaptureState();
            var savingSystem = FindObjectOfType<SavingSystem>();
            if (savingSystem != null)
            {
                savingSystem.SaveGame();
            }
            else
            {
                Debug.Log("저장 실패...");
            }
            SceneManager.LoadScene("LobbyScene");
        }
    }
    IEnumerator StartNextBattleWithDelay()
    {
        yield return new WaitForSeconds(0.5f); // 약간의 여유
        StartBattle();
    }
    public void AddGold()
    {
        GlobalValue.UserGold += ((GlobalValue.CurStage + 10) / 10) * 1000;

        Gold_Text.text = $"￡{GlobalValue.UserGold}";
    }
    public string GetCorrectParticle(string name, string particleType)    //은는이가
    {
        char lastChar = name[name.Length - 1];
        int unicode = (int)lastChar;
        bool endsWithConsonant = (unicode - 44032) % 28 != 0; // 44032는 '가'의 유니코드, 28는 받침의 수


        switch (particleType)
        {
            case "subject": // 이/가
                { return endsWithConsonant ? "이" : "가"; }
            case "topic": // 은/는
                { return endsWithConsonant ? "은" : "는"; }
            case "object": // 을/를
                { return endsWithConsonant ? "을" : "를"; }
            case "objectTo": // 로/으로
                { return endsWithConsonant ? "로" : "으로"; }
            default:
                throw new ArgumentException("Invalid particle type");
        }
    }
    #region HandleState
    public void ActionSelection()
    {
        state = BattleState.Action;
        dialogBox.EnableActionSelector(true);
        StartCoroutine(dialogBox.TypeDialog($"{battleSystem.playerUnit.BattlePokemon.P_Base.PokemonName}{GetCorrectParticle(battleSystem.playerUnit.BattlePokemon.P_Base.PokemonName, "topic")} 무엇을 할까?"));
    }
    public void SkillSelection()
    {
        state = BattleState.Skill;
        dialogBox.EnableActionSelector(false);
        dialogBox.EnableDialogText(false);
        dialogBox.EnableSkillSelector(true);
    }
    public void OpenPartyScreen()
    {
        state = BattleState.Busy;
        partyScreen.Init();
        partyScreen.SetPartyData(playerParty.Party);
        partyScreen.gameObject.SetActive(true);
        state = BattleState.PartyScreen;
    }
    public void ConfirmBoxSelection()
    {
        state = BattleState.Busy;
        currentConfirm = 0;
        ConfirmBox.SetActive(true);
        state = BattleState.ConfirmBox;
    }
    #endregion
    #region Selection
    void HandleActionSelection()
    {
        if (state == BattleState.Dialog)
        {
            return;
        }
        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            if (currentAction < 2)
            {
                currentAction = currentAction + 2;
            }
        }
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            if (currentAction < 3)
            {
                ++currentAction;
            }
        }
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            if (1 < currentAction)
            {
                currentAction = currentAction - 2;
            }
        }
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            if (0 < currentAction)
            {
                --currentAction;
            }
        }
        currentAction = Mathf.Clamp(currentAction, 0, 3);
        dialogBox.UpdateActionSelection(currentAction);
        if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return))
        {
            if (currentAction == 0)
            {//싸운다
                SkillSelection();
            }
            else if (currentAction == 1)
            {//볼
                StartCoroutine(battleSystem.RunTurns(BattleAction.UseItem));
            }
            else if (currentAction == 2)
            {//포켓몬
                preState = state;
                OpenPartyScreen();
            }
            else if (currentAction == 3)
            {//도망친다
                StartCoroutine(battleSystem.RunTurns(BattleAction.Run));
            }
        }
    }
    void HandleSkillSelection()
    {
        if (state == BattleState.Dialog)
        {
            return;
        }
        if (Input.GetKeyDown(KeyCode.DownArrow) && currentSkill + 2 < skillCount)
        {
            currentSkill += 2;
        }
        if (Input.GetKeyDown(KeyCode.RightArrow) && currentSkill + 1 < skillCount)
        {
            currentSkill++;
        }
        if (Input.GetKeyDown(KeyCode.UpArrow) && currentSkill - 2 >= 0)
        {
            currentSkill -= 2;
        }
        if (Input.GetKeyDown(KeyCode.LeftArrow) && currentSkill - 1 >= 0)
        {
            currentSkill--;
        }

        currentSkill = Mathf.Clamp(currentSkill, 0, skillCount - 1);

        if (skillCount > 0)
        {
            dialogBox.UpdateSkillSelection(currentSkill, battleSystem.playerUnit.BattlePokemon.Skills[currentSkill]);
        }

        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space))
        {
            /*
            // 모든 스킬의 PP가 0인지 확인
            if (sourceUnit.BattlePokemon.Skills.TrueForAll(s => s.SkillPP <= 0))
            {
                // "발버둥" 기본 기술 사용
                yield return dialogBox.TypeDialog($"{sourceUnit.BattlePokemon.PokemonBase.PokemonName}은(는) 사용할 스킬이 없습니다! 발버둥을 사용합니다!");
                skill = struggleSkill; // 발버둥 기술로 대체
            }
            */
            var skill = battleSystem.playerUnit.BattlePokemon.Skills[currentSkill];
            if (skill.PP == 0)
            {
                // 스킬 사용 불가 메시지 출력
                dialogBox.EnableSkillSelector(false);
                dialogBox.EnableDialogText(true);
                StartCoroutine(dialogBox.TypeDialog($"기술의 남은 포인트가 없다!"));
                SkillSelection();
                return;
            }

            dialogBox.EnableSkillSelector(false);
            dialogBox.EnableDialogText(true);
            StartCoroutine(battleSystem.RunTurns(BattleAction.Skill));
        }
        else if (Input.GetKeyDown(KeyCode.Backspace))
        {
            dialogBox.EnableSkillSelector(false);
            dialogBox.EnableDialogText(true);
            ActionSelection();
        }
    }
    #endregion
    #region PartySystem
    void HandlePartyScreenSelection()
    {
        if (state == BattleState.Dialog)
        {
            return;
        }
        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            currentMember++;
        }
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            currentMember++;
        }
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            currentMember--;
        }
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            currentMember = 0;
        }
        currentMember = Mathf.Clamp(currentMember, 0, playerParty.Party.Count - 1);
        partyScreen.UpdateMemberSelection(currentMember);
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space))
        {
            //포켓몬 교체
            var selectedMember = playerParty.Party[currentMember];
            if (selectedMember.PokemonHp <= 0)
            {
                partyScreen.SetMessageText($"{playerParty.Party[currentMember].P_Base.PokemonName}{GetCorrectParticle(playerParty.Party[currentMember].P_Base.PokemonName, "topic")} 싸울 수 있는 \n기력이 남아 있지 않습니다!");
                return;
            }
            if (selectedMember == battleSystem.playerUnit.BattlePokemon)
            {
                //능력치보기, 놓아주기, 그만두기 구현
                partyScreen.SetMessageText($"이미 전투 중인 포켓몬으로 교체 할 수 없습니다!");
                return;
            }

            partyScreen.gameObject.SetActive(false);
            StartCoroutine(battleSystem.RunTurns(BattleAction.SwitchPokemon));
            dialogBox.EnableActionSelector(false);
        }
        else if (Input.GetKeyDown(KeyCode.Backspace))
        {
            partyScreen.gameObject.SetActive(false);
            ActionSelection();
        }
    }
    #endregion
    #region LearnSkill
    public void HandleLearnSkillSelection()
    {
        if (state == BattleState.Dialog)
        {
            return;
        }
        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            currentSelection++;
        }
        else if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            currentSelection--;
        }

        currentSelection = Mathf.Clamp(currentSelection, 0, PokemonBase.MaxNumOfSkills);
        skillSelectScreen.UpdateSkillSelection(currentSelection);

        if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return))
        {
            skillSelectScreen.gameObject.SetActive(false);
            state = BattleState.Busy;
        }
        else if (Input.GetKeyDown(KeyCode.Backspace))
        {
            skillSelectScreen.gameObject.SetActive(false);
            battleSystem.cancelSelected = true;
            state = BattleState.Busy;
        }
    }
    #endregion
    #region  Confirm Box
    public bool HandleConfirmBoxSelection()
    {
        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            currentConfirm++;
        }
        else if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            currentConfirm--;
        }
        currentConfirm = Mathf.Clamp(currentConfirm, 0, 1);
        dialogBox.ConfirmBoxSelection(currentConfirm);
        if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return))
        {
            {
                if (currentConfirm == 0)
                {
                    ConfirmBox.SetActive(false);

                    state = BattleState.Busy;
                    return true;
                }
                else
                {
                    ConfirmBox.SetActive(false);

                    state = BattleState.Busy;
                    return false;
                }
            }
        }
        else if (Input.GetKeyDown(KeyCode.Backspace))
        {
            ConfirmBox.SetActive(false);

            state = BattleState.Busy;
            return false;
        }
        else
        {
            return true;
        }
    }
    #endregion
}



/*
  public getLevelForWave(): number {
    const levelWaveIndex = this.gameMode.getWaveForDifficulty(this.waveIndex);
    const baseLevel = 1 + levelWaveIndex / 2 + Math.pow(levelWaveIndex / 25, 2);
    const bossMultiplier = 1.2;

    if (this.gameMode.isBoss(this.waveIndex)) {
      const ret = Math.floor(baseLevel * bossMultiplier);
      if (this.battleSpec === BattleSpec.FINAL_BOSS || !(this.waveIndex % 250)) {
        return Math.ceil(ret / 25) * 25;
      }
      let levelOffset = 0;
      if (!this.gameMode.isWaveFinal(this.waveIndex)) {
        levelOffset = Math.round(Phaser.Math.RND.realInRange(-1, 1) * Math.floor(levelWaveIndex / 10));
      }
      return ret + levelOffset;
    }

    let levelOffset = 0;

    const deviation = 10 / levelWaveIndex;
    levelOffset = Math.abs(this.randSeedGaussForLevel(deviation));

    return Math.max(Math.round(baseLevel + levelOffset), 1);
  }







*/
