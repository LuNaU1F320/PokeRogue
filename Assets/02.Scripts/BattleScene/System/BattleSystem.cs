using System.Collections;
using System;
using UnityEngine;
using System.Linq;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;

public enum BattleState
{
    Start,
    ActionSelection,
    SkillSelection,
    RunningTurn,
    Busy,
    PartyScreen,
    SkillToForget,
    ConfirmBox,
    Evolution,
    BattleOver,
    ConfigSelection
}
public enum BattleActionType
{
    Skill,
    Switch,
    Catch,
    Run
}

public class BattleAction
{
    public BattleActionType Type;
    public Skill Skill;
    public int SwitchIndex;
}

public class BattleSystem : MonoBehaviour
{
    public static BattleSystem Inst;
    [SerializeField] BattleUnit playerUnit;
    [SerializeField] BattleUnit enemyUnit;
    [SerializeField] BattleDialogBox dialogBox;
    [SerializeField] ConfigPanel configPanel;
    [SerializeField] GameObject Pokeball;
    [SerializeField] GameObject ConfirmBox;

    [SerializeField] SpriteRenderer PlayerSprite;
    [SerializeField] SpriteRenderer TrainerSprite;

    //Party
    [SerializeField] PartyScreen partyScreen;
    [SerializeField] SkillSelectScreen skillSelectScreen;

    [HideInInspector] public BattleState state;
    BattleState? preState;
    int currentAction = 0;
    int currentSkill = 0;
    int currentMember = 0;
    int currentSelection = 0;
    int currentConfirm = 0;
    int currentConfig = 0;
    int skillCount = 0;

    PokemonParty playerParty;
    PokemonParty trainerParty;
    Pokemon wildPokemon;
    bool isTrainerBattle = false;

    PlayerCtrl player;
    [SerializeField] Image PlayerImage;
    TrainerCtrl trainer;

    SkillBase skillToLearn;
    public BattleAction SelectedAction { get; private set; }

    private void Awake()
    {
        Inst = this;
        player = FindObjectOfType<PlayerCtrl>();
    }
    private void Start()
    {
        state = BattleState.Start;
        currentAction = 0;
        PlayerImage.sprite = player.TrainerSprite;
        PlayerImage.gameObject.SetActive(false);
    }
    public async UniTask StartBattle(PokemonParty playerParty, Pokemon wildPokemon)
    {
        state = BattleState.Start;

        this.playerParty = playerParty;
        this.wildPokemon = wildPokemon;
        isTrainerBattle = false;

        await SetUpBattle();
        await StartTurnLoop();
    }
    public async UniTask SetUpBattle()
    {
        if (isTrainerBattle == false)
        {
            playerUnit.SetUp(playerParty.GetHealthyPokemon());
            enemyUnit.SetUp(wildPokemon);
            dialogBox.SetSkillNames(playerUnit.BattlePokemon.Skills);
            skillCount = playerUnit.BattlePokemon.Skills.Count;

            await dialogBox.TypeDialog($"앗! 야생 {enemyUnit.BattlePokemon.P_Base.PokemonName}{GameManager.Inst.GetCorrectParticle(enemyUnit.BattlePokemon.P_Base.PokemonName, "subject")} \n튀어나왔다!");
        }
        else
        {
            // playerUnit.gameObject.SetActive(false);
            // enemyUnit.gameObject.SetActive(false);

            // PlayerSprite.gameObject.SetActive(true);
            // TrainerSprite.gameObject.SetActive(true);

            // PlayerSprite.sprite = player.TrainerSprite;
            // TrainerSprite.sprite = trainer.TrainerSprite;

            // yield return dialogBox.TypeDialog($"{trainer.TrainerName}{/*은는이가*/""}이 배틀을 걸어왔다!");
        }

        partyScreen.Init();
        await ActionSelection();
    }
    public void Update()
    {
        if (state == BattleState.BattleOver || state == BattleState.Evolution)
        {
            return;
        }
        if (state == BattleState.ActionSelection)
        {
            HandleActionSelection();
        }
        else if (state == BattleState.SkillSelection)
        {
            HandleSkillSelection();
        }
        else if (state == BattleState.PartyScreen)
        {
            HandlePartyScreenSelection();
        }
        // else if (state == BattleState.SkillToForget)
        // {
        //     HandleLearnSkillSelection();
        // }
        else if (state == BattleState.ConfirmBox)
        {
            HandleConfirmBoxSelection();
        }
        else if (state == BattleState.ConfigSelection)
        {
            HandleConfigSelection();
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            // Debug.Log(state);
            if (state == BattleState.ConfigSelection)
            {
                if (configPanel.state == ConfigState.Config_Right)
                {
                    if (Input.GetKeyDown(KeyCode.Escape))
                    {
                        configPanel.Panel.SetActive(false);
                        state = preState ?? BattleState.RunningTurn;
                    }
                }
            }
            else
            {
                configPanel.Panel.SetActive(true);
                preState = state;
                state = BattleState.ConfigSelection;
                currentConfig = 0;
                configPanel.state = ConfigState.Config_Right;
            }
        }
    }
    void BattleOver(bool won)
    {
        state = BattleState.BattleOver;
        // StartCoroutine(playerParty.CheckForEvolutions());
        GameManager.Inst.EndBattle(won);
    }

    public async UniTask WaitForPlayerAction()
    {
        await UniTask.WaitUntil(() => SelectedAction != null);
    }
    public void SetSelectedAction(BattleAction action)
    {
        SelectedAction = action;
    }

    private async UniTask StartTurnLoop()
    {
        while (state != BattleState.BattleOver)
        {
            await WaitForPlayerAction();
            await RunPlayerTurn(SelectedAction);
            if (state == BattleState.BattleOver)
            {
                break;
            }
            await RunEnemyTurn();
            await ActionSelection();
        }
    }
    private async UniTask RunPlayerTurn(BattleAction action)
    {
        state = BattleState.RunningTurn;

        switch (action.Type)
        {
            case BattleActionType.Skill:
                await RunSkill(playerUnit, enemyUnit, action.Skill);
                break;
            case BattleActionType.Catch:
                // await TryCatchPokemonAsync();
                break;
            case BattleActionType.Run:
                // await TryToRunAsync();
                break;
            case BattleActionType.Switch:
                // await SwitchPokemonAsync(action.SwitchIndex);
                break;
        }

        await RunAfterTurn(enemyUnit);

        if (enemyUnit.BattlePokemon.PokemonHp <= 0)
        {
            await HandlePokemonFainted(enemyUnit);
            await CheckBattleOver(enemyUnit);
        }

        SelectedAction = null;
    }
    private async UniTask RunEnemyTurn()
    {
        state = BattleState.RunningTurn;
        var enemySkill = enemyUnit.BattlePokemon.GetRandomSkill();
        await RunSkill(enemyUnit, playerUnit, enemySkill);

        await RunAfterTurn(playerUnit);

        if (playerUnit.BattlePokemon.PokemonHp <= 0)
        {
            await HandlePokemonFainted(playerUnit);
            await CheckBattleOver(playerUnit);
        }
    }

    void OpenPartyScreen()
    {
        state = BattleState.PartyScreen;
        partyScreen.Init();
        partyScreen.SetPartyData(playerParty.Party);
        partyScreen.gameObject.SetActive(true);
    }
    public async UniTask ActionSelection()
    {
        state = BattleState.ActionSelection;
        dialogBox.EnableActionSelector(true);

        var name = playerUnit.BattlePokemon.P_Base.PokemonName;
        await dialogBox.TypeDialog($"{name}{GameManager.Inst.GetCorrectParticle(name, "topic")} 무엇을 할까?");
    }
    void SkillSelection()
    {
        state = BattleState.SkillSelection;
        dialogBox.EnableActionSelector(false);
        dialogBox.EnableDialogText(false);
        dialogBox.EnableSkillSelector(true);
    }
    public void ConfirmBoxSelection()
    {
        state = BattleState.ConfirmBox;
        currentConfirm = 0;
        ConfirmBox.SetActive(true);
    }
    // IEnumerator ChooseSkillToForget(Pokemon pokemon, SkillBase newSkill)
    // {
    //     state = BattleState.Busy;
    //     yield return dialogBox.TypeDialog($"어느 기술을 잊게 하고싶은가?");
    //     skillSelectScreen.gameObject.SetActive(true);
    //     skillSelectScreen.SetPokemonData(pokemon);
    //     skillSelectScreen.SetSkill(pokemon.Skills.Select(x => x.SkillBase).ToList(), newSkill);
    //     skillToLearn = newSkill;

    //     state = BattleState.SkillToForget;
    // }


    #region RunningTurn

    public async UniTask RunSkill(BattleUnit sourceUnit, BattleUnit targetUnit, Skill skill)
    {
        // PP 소모
        skill.PP--;

        // 기술 이름 출력
        await dialogBox.TypeDialog($"{sourceUnit.BattlePokemon.P_Base.PokemonName}의 {skill.SkillBase.SkillName}!");

        // 공격 애니메이션
        sourceUnit.PlayAttackAnimation();
        await UniTask.Delay(500);

        // 피격 애니메이션
        targetUnit.PlayHitAnimation();
        await UniTask.Delay(300);

        if (CheckSkillHits(skill, sourceUnit, targetUnit))
        {
            if (skill.SkillBase.CategoryKey == CategoryKey.Status)
            {
                if (targetUnit.BattlePokemon.Status != null)
                {
                    await dialogBox.TypeDialog("효과가 없는 것 같다...");
                }
                else
                {
                    await RunSkillEffect(skill.SkillBase.Effects, sourceUnit.BattlePokemon, targetUnit.BattlePokemon, skill.SkillBase.Target);
                }
            }
            else
            {
                // 데미지 처리
                var (startHp, endHp, damageDetails) = targetUnit.BattlePokemon.TakeDamage(skill, sourceUnit.BattlePokemon);
                await targetUnit.BattleHud.UpdateHpAsync();
                await ShowDamageDetailsAsync(damageDetails);
            }

            // 부가효과 처리
            if (skill.SkillBase.SecondaryEffects != null && skill.SkillBase.SecondaryEffects.Count > 0 && targetUnit.BattlePokemon.PokemonHp > 0)
            {
                foreach (var secondary in skill.SkillBase.SecondaryEffects)
                {
                    var rnd = UnityEngine.Random.Range(1, 101);
                    if (rnd <= secondary.Chance)
                    {
                        await RunSkillEffect(secondary, sourceUnit.BattlePokemon, targetUnit.BattlePokemon, secondary.Target);
                    }
                }
            }

            // 쓰러짐 처리
            if (targetUnit.BattlePokemon.PokemonHp <= 0)
            {
                await HandlePokemonFainted(targetUnit);
            }
        }
        else
        {
            // 공격 빗나감 처리
            if (sourceUnit.IsPlayerUnit)
            {
                await dialogBox.TypeDialog($"상대 {targetUnit.BattlePokemon.P_Base.PokemonName}에게는 \n맞지 않았다!");
            }
            else
            {
                await dialogBox.TypeDialog($"{targetUnit.BattlePokemon.P_Base.PokemonName}에게는 맞지 않았다!");
            }
        }
    }
    // 기술이 명중하는지 여부 판단
    private bool CheckSkillHits(Skill skill, BattleUnit attacker, BattleUnit target)
    {
        if (skill.SkillBase.AlwaysHits)
        {
            return true;
        }

        float skillAccuracy = skill.SkillBase.SkillAccuracy;
        int accuracy = attacker.BattlePokemon.Rankup[Stat.Accuracy];
        int evasion = target.BattlePokemon.Rankup[Stat.Evasion];

        var rankupValues = new float[] { 1f, 1.5f, 2f, 2.5f, 3f, 3.5f, 4f };

        if (accuracy > 0)
        {
            skillAccuracy *= rankupValues[accuracy];
        }
        else
        {
            skillAccuracy /= rankupValues[-accuracy];
        }

        if (evasion > 0)
        {
            skillAccuracy /= rankupValues[evasion];
        }
        else
        {
            skillAccuracy *= rankupValues[-evasion];
        }

        return UnityEngine.Random.Range(1, 101) <= skillAccuracy;
    }

    // 기술의 효과(랭크업, 상태이상 등)를 적용하는 메서드
    public async UniTask RunSkillEffect(SkillEffects effects, Pokemon sourceUnit, Pokemon targetUnit, SkillTarget skillTarget)
    {
        // 능력치 변화 (랭크업)
        if (effects.Rankup != null)
        {
            if (skillTarget == SkillTarget.Self)
            {
                sourceUnit.ApplyRankups(effects.Rankup);
            }
            else
            {
                targetUnit.ApplyRankups(effects.Rankup);
            }
        }

        // 상태이상 부여
        if (effects.Status != ConditionID.None)
        {
            targetUnit.SetStatus(effects.Status);
        }

        // 일시적 상태이상
        if (effects.VolatileStatus != ConditionID.None)
        {
            targetUnit.SetVolatileStatus(effects.VolatileStatus);
        }

        // 상태 변화 메시지 출력 (양쪽 모두)
        await ShowStatusChangesAsync(sourceUnit);
        await ShowStatusChangesAsync(targetUnit);
    }
    // 대미지 세부 정보에 따라 대사 출력
    public async UniTask ShowDamageDetailsAsync(DamageDetails damageDetails)
    {
        if (damageDetails.Critical > 1f)
        {
            await dialogBox.TypeDialog("급소에 맞았다!");
        }

        if (damageDetails.TypeEffectiveness > 1f)
        {
            await dialogBox.TypeDialog("효과가 굉장했다!");
        }
        else if (damageDetails.TypeEffectiveness < 1f && damageDetails.TypeEffectiveness > 0)
        {
            await dialogBox.TypeDialog("효과가 별로인 듯하다...");
        }
        else if (Mathf.Approximately(damageDetails.TypeEffectiveness, 0f))
        {
            await dialogBox.TypeDialog("효과가 없는 것 같다...");
        }
    }
    // 턴 종료 후 상태이상 데미지, 쓰러짐 처리 등을 담당
    public async UniTask RunAfterTurn(BattleUnit unit)
    {
        if (state == BattleState.BattleOver)
        {
            return;
        }

        await UniTask.WaitUntil(() => state == BattleState.RunningTurn);
        await UniTask.Delay(1200);

        unit.BattlePokemon.OnAfterTurn();
        await ShowStatusChangesAsync(unit.BattlePokemon);
        await unit.BattleHud.UpdateHpAsync();

        if (unit.BattlePokemon.PokemonHp <= 0)
        {
            await HandlePokemonFainted(unit);
            if (state != BattleState.RunningTurn)
            {
                await UniTask.WaitUntil(() => state == BattleState.RunningTurn);
            }
        }
    }
    // 상태 변화 메시지 출력
    public async UniTask ShowStatusChangesAsync(Pokemon pokemon)
    {
        while (pokemon.StatusCngMsg.Count > 0)
        {
            string message = pokemon.StatusCngMsg.Dequeue();
            await dialogBox.TypeDialog(message);
        }
    }
    // 포켓몬 쓰러짐 연출 및 메시지 처리
    public async UniTask HandlePokemonFainted(BattleUnit unit)
    {
        if (unit.BattlePokemon.PokemonHp > 0)
        {
            return;
        }

        await dialogBox.TypeDialog($"{unit.BattlePokemon.P_Base.PokemonName}이 쓰러졌다!");
        unit.PlayFaintAnimation();
        await UniTask.Delay(1500); // 기존 대기 시간 반영

        if (!unit.IsPlayerUnit)
        {
            int expYield = unit.BattlePokemon.P_Base.ExpYield;
            int enemyLevel = unit.BattlePokemon.PokemonLevel;
            float trainerBonus = isTrainerBattle ? 1.5f : 1.0f;

            int expGain = Mathf.FloorToInt(expYield * enemyLevel * trainerBonus / 7);
            playerUnit.BattlePokemon.PokemonExp += expGain;

            await dialogBox.TypeDialog($"{playerUnit.BattlePokemon.P_Base.PokemonName}이 {expGain} 경험치를 얻었다!");
            await playerUnit.BattleHud.SetExpSmooth();
            await CheckLearnableSkillAsync();
        }
        await CheckBattleOver(unit);
        GameManager.Inst.AddGold();
    }
    // 전투 종료 여부 판단 및 종료 처리 (진화 포함)
    public async UniTask CheckBattleOver(BattleUnit faintedUnit)
    {
        if (faintedUnit.IsPlayerUnit)
        {
            var nextPokemon = playerParty.GetHealthyPokemon();
            if (nextPokemon != null)
            {
                OpenPartyScreen();
                await UniTask.WaitUntil(() => state == BattleState.Busy);
            }
            else
            {
                BattleOver(false);
            }
        }
        else
        {
            if (!isTrainerBattle)
            {
                await playerParty.CheckForEvolutionsAsync();
                await UniTask.Delay(500); // 진화 마무리 대기

                BattleOver(true);
            }
            else
            {
                var nextPokemon = trainerParty.GetHealthyPokemon();
                if (nextPokemon != null)
                {
                    await playerParty.CheckForEvolutionsAsync();
                }
                else
                {
                    await playerParty.CheckForEvolutionsAsync();
                    BattleOver(true);
                }
            }
        }
    }
    #endregion
    #region LearnSkill
    bool cancelSelected;
    // 레벨업 후 배울 수 있는 기술이 있는지 확인하고 처리하는 메서드
    public async UniTask CheckLearnableSkillAsync()
    {
        while (playerUnit.BattlePokemon.CheckForLevelUp())
        {
            playerUnit.BattleHud.SetLevel();
            await dialogBox.TypeDialog($"{playerUnit.BattlePokemon.P_Base.PokemonName}{GetCorrectParticle(playerUnit.BattlePokemon.P_Base.PokemonName, "topic")} 레벨 {playerUnit.BattlePokemon.PokemonLevel}로 올랐다!");

            var newSkill = playerUnit.BattlePokemon.GetLearnableSkill();
            if (newSkill != null)
            {
                if (playerUnit.BattlePokemon.Skills.Count < PokemonBase.MaxNumOfSkills)
                {
                    playerUnit.BattlePokemon.LearnSkill(newSkill);
                    await dialogBox.TypeDialog($"{playerUnit.BattlePokemon.P_Base.PokemonName}{GetCorrectParticle(playerUnit.BattlePokemon.P_Base.PokemonName, "topic")} 새로 {newSkill.SkillBase.SkillName}{GetCorrectParticle(newSkill.SkillBase.SkillName, "object")} 배웠다!");
                    dialogBox.SetSkillNames(playerUnit.BattlePokemon.Skills);
                }
                else
                {
                    bool isFinalDecisionMade = false;
                    while (!isFinalDecisionMade)
                    {
                        await dialogBox.TypeDialog($"{playerUnit.BattlePokemon.P_Base.PokemonName}{GetCorrectParticle(playerUnit.BattlePokemon.P_Base.PokemonName, "topic")} 새로 {newSkill.SkillBase.SkillName}{GetCorrectParticle(newSkill.SkillBase.SkillName, "object")} 배우고 싶어한다!");
                        await dialogBox.TypeDialog("하지만 기술이 4개이므로 다른 기술을 잊어야 한다.");
                        await dialogBox.TypeDialog($"{newSkill.SkillBase.SkillName} 대신 다른 기술을 잊게 하겠습니까?");

                        ConfirmBoxSelection();
                        await UniTask.WaitUntil(() => state != BattleState.ConfirmBox);
                        bool isConfirmed = HandleConfirmBoxSelection();

                        if (isConfirmed)
                        {
                            // await ChooseSkillToForget(playerUnit.BattlePokemon, newSkill.SkillBase);
                            await UniTask.WaitUntil(() => state == BattleState.Busy);

                            if (cancelSelected || currentSelection == PokemonBase.MaxNumOfSkills)
                            {
                                cancelSelected = false;

                                await dialogBox.TypeDialog($"그럼... {newSkill.SkillBase.SkillName}{GetCorrectParticle(newSkill.SkillBase.SkillName, "object")} 배우는 것을 포기하겠습니까?");
                                ConfirmBoxSelection();
                                await UniTask.WaitUntil(() => state != BattleState.ConfirmBox);
                                bool giveUp = HandleConfirmBoxSelection();

                                if (giveUp)
                                {
                                    await dialogBox.TypeDialog($"{playerUnit.BattlePokemon.P_Base.PokemonName}{GetCorrectParticle(playerUnit.BattlePokemon.P_Base.PokemonName, "topic")} 결국 {newSkill.SkillBase.SkillName}{GetCorrectParticle(newSkill.SkillBase.SkillName, "object")} 배우지 않았다!");
                                    skillToLearn = null;
                                    isFinalDecisionMade = true;
                                }
                                else
                                {
                                    // await ChooseSkillToForget(playerUnit.BattlePokemon, newSkill.SkillBase);
                                    await UniTask.WaitUntil(() => state == BattleState.SkillToForget);
                                }
                            }
                            else
                            {
                                var oldSkill = playerUnit.BattlePokemon.Skills[currentSelection].SkillBase;
                                playerUnit.BattlePokemon.Skills[currentSelection] = new Skill(newSkill.SkillBase);

                                await dialogBox.TypeDialog("1, 2, ... 짠!");
                                await dialogBox.TypeDialog($"{playerUnit.BattlePokemon.P_Base.PokemonName}{GetCorrectParticle(playerUnit.BattlePokemon.P_Base.PokemonName, "topic")} {oldSkill.SkillName}{GetCorrectParticle(oldSkill.SkillName, "object")} 깨끗이 잊었다!");
                                await dialogBox.TypeDialog($"그리고 {newSkill.SkillBase.SkillName}{GetCorrectParticle(newSkill.SkillBase.SkillName, "object")} 배웠다!");

                                dialogBox.SetSkillNames(playerUnit.BattlePokemon.Skills);
                                skillToLearn = null;
                                isFinalDecisionMade = true;
                            }
                        }
                        else
                        {
                            await dialogBox.TypeDialog($"그럼... {newSkill.SkillBase.SkillName}{GetCorrectParticle(newSkill.SkillBase.SkillName, "object")} 배우는 것을 포기하겠습니까?");
                            ConfirmBoxSelection();
                            await UniTask.WaitUntil(() => state != BattleState.ConfirmBox);
                            bool isReallyConfirmed = HandleConfirmBoxSelection();

                            if (isReallyConfirmed)
                            {
                                await dialogBox.TypeDialog($"{playerUnit.BattlePokemon.P_Base.PokemonName}{GetCorrectParticle(playerUnit.BattlePokemon.P_Base.PokemonName, "topic")} 결국 {newSkill.SkillBase.SkillName}{GetCorrectParticle(newSkill.SkillBase.SkillName, "object")} 배우지 않았다!");
                                skillToLearn = null;
                                isFinalDecisionMade = true;
                            }
                        }
                    }
                }
            }
            await playerUnit.BattleHud.SetExpSmooth(true);
        }
    }
    #endregion

    void HandleActionSelection()
    {
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

            }
            else if (currentAction == 2)
            {//포켓몬
                state = BattleState.PartyScreen;
                OpenPartyScreen();
            }
            else if (currentAction == 3)
            {//도망친다
                // await turnController.ExecutePlayerTurnAsync(new BattleAction { Type = BattleActionType.Run });
            }
        }
    }

    void HandleSkillSelection()
    {
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
            dialogBox.UpdateSkillSelection(currentSkill, playerUnit.BattlePokemon.Skills[currentSkill]);
        }

        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space))
        {
            var skill = playerUnit.BattlePokemon.Skills[currentSkill];
            if (skill.PP == 0)
            {
                dialogBox.EnableSkillSelector(false);
                dialogBox.EnableDialogText(true);
                dialogBox.TypeDialog("기술의 남은 포인트가 없다!").Forget();
                ActionSelection().Forget();
                return;
            }
            dialogBox.EnableSkillSelector(false);
            dialogBox.EnableDialogText(true);
            SetSelectedAction(new BattleAction
            {
                Type = BattleActionType.Skill,
                Skill = skill
            });
            state = BattleState.RunningTurn;
        }
        else if (Input.GetKeyDown(KeyCode.Backspace))
        {
            dialogBox.EnableSkillSelector(false);
            dialogBox.EnableDialogText(true);
            ActionSelection().Forget();
        }
    }

    #region PartySystem
    void HandlePartyScreenSelection()
    {
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
            if (selectedMember == playerUnit.BattlePokemon)
            {
                //능력치보기, 놓아주기, 그만두기 구현
                partyScreen.SetMessageText($"이미 전투 중인 포켓몬으로 교체 할 수 없습니다!");
                return;
            }

            partyScreen.gameObject.SetActive(false);
            // 액션 선택 중 교체
            if (preState == BattleState.ActionSelection)
            {
                preState = null;
                SetSelectedAction(new BattleAction
                {
                    Type = BattleActionType.Switch,
                    SwitchIndex = currentMember
                });
            }
            //포켓몬이 쓰러졌을때
            else
            {
                state = BattleState.Busy;
                SwitchPokemon(playerParty.Party[currentMember]).Forget();
            }
        }
        else if (Input.GetKeyDown(KeyCode.Backspace))
        {
            partyScreen.gameObject.SetActive(false);
            ActionSelection().Forget();
        }
    }
    public async UniTask SwitchPokemon(Pokemon newPokemon)
    {
        playerUnit.BattlePokemon.CureVolatileStatus();
        playerUnit.BattlePokemon.ResetRankup();

        await dialogBox.TypeDialog($"돌아와 {playerUnit.BattlePokemon.P_Base.PokemonName}!");
        //사망애니메이션
        playerUnit.PlayFaintAnimation();
        await UniTask.Delay(1000);

        // 현재 전투 중인 포켓몬 (인덱스 0에 있는 포켓몬)
        var currentBattlePokemon = playerParty.Party[0];

        // 교체 작업: 교체할 포켓몬을 0번 인덱스로, 나가있는 포켓몬을 교체할 포켓몬의 인덱스로 이동
        playerParty.Party[0] = newPokemon;
        playerParty.Party[currentMember] = currentBattlePokemon;

        playerUnit.SetUp(newPokemon);
        dialogBox.SetSkillNames(newPokemon.Skills);

        skillCount = newPokemon.Skills.Count;

        await dialogBox.TypeDialog($"가랏! {newPokemon.P_Base.PokemonName}!");
        state = BattleState.RunningTurn;
    }
    #endregion
    #region LearnSkill
    public void HandleLearnSkillSelection()
    {
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
            cancelSelected = true;
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
    #region Config
    void HandleConfigSelection()
    {
        if (configPanel.state == ConfigState.Config_Right)
        {
            if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                --currentConfig;
            }
            if (Input.GetKeyDown(KeyCode.DownArrow))
            {
                ++currentConfig;
            }
            currentConfig = Mathf.Clamp(currentConfig, 0, 5);
            configPanel.ConfigSelection(currentConfig);
            if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return))
            {
                if (currentConfig == 0)
                {//게임설정
                    configPanel.SettingSelection();
                }
                else if (currentConfig == 1)
                {//도감
                    Debug.Log("도감");
                }
                else if (currentConfig == 2)
                {//데이터관리
                    Debug.Log("데이터 관리");
                }
                else if (currentConfig == 3)
                {//커뮤니티
                    Debug.Log("커뮤니티");
                }
                else if (currentConfig == 4)
                {//저장 후 나가기
                    var savingSystem = FindObjectOfType<SavingSystem>();
                    if (savingSystem != null)
                    {
                        savingSystem.SaveGame();
                        playerParty.Party.Clear();
                        // Debug.Log("저장 완료!");
                        SceneManager.LoadScene("LobbyScene");
                        // Debug.Log("저장 후 나가기");
                    }
                    else
                    {
                        Debug.LogWarning("SavingSystem을 찾지 못했어요… 저장 실패!");
                    }
                }
                else if (currentConfig == 5)
                {//로그아웃
                    Debug.Log("로그아웃");
                }
            }
        }
    }
    #endregion
    // #region Catch
    // IEnumerator ThrowPokeball()
    // {
    //     state = BattleState.Busy;

    //     if (isTrainerBattle)
    //     {
    //         yield return dialogBox.TypeDialog("다른 트레이너의 포켓몬은 잡을 수 없다!");
    //         state = BattleState.RunningTurn;
    //         yield break;
    //     }

    //     var pokeballObj = Instantiate(Pokeball, playerUnit.transform.position, Quaternion.identity);
    //     var pokeball = pokeballObj.GetComponent<SpriteRenderer>();

    //     //#34 1254
    //     //pokeball.transform.DoMove

    //     int shakeCount = TryToCatchPokemon(enemyUnit.BattlePokemon);
    //     for (int i = 0; i < Math.Min(shakeCount, 3); ++i)
    //     {
    //         yield return new WaitForSeconds(0.5f);
    //         //흔들기 애니메이션
    //     }
    //     if (shakeCount == 4)
    //     {
    //         //잡힘
    //         playerParty.AddPokemon(enemyUnit.BattlePokemon);

    //         GlobalValue.CatchPokemon(enemyUnit.BattlePokemon.P_Base, false);

    //         Destroy(pokeball);
    //         yield return dialogBox.TypeDialog($"신난다-!\n야생 {enemyUnit.BattlePokemon.P_Base.PokemonName}을 잡았다!");
    //         BattleOver(true);
    //     }
    //     else
    //     {
    //         yield return dialogBox.TypeDialog($"!");
    //         Destroy(pokeball);
    //         state = BattleState.RunningTurn;
    //     }
    //     yield return new WaitForSeconds(1.0f);
    // }
    // int TryToCatchPokemon(Pokemon pokemon)
    // {
    //     float a = (3 * pokemon.MaxHp - 2 * pokemon.PokemonHp) * pokemon.P_Base.CatchRate * ConditionsDB.GetStatusBonus(pokemon.Status) / (3 * pokemon.MaxHp);

    //     if (a >= 255)
    //     {
    //         //흔들린 횟수
    //         return 4;
    //     }

    //     float b = 1048560 / Mathf.Sqrt(Mathf.Sqrt(16711680 / a));

    //     int shakeCount = 0;
    //     while (shakeCount < 4)
    //     {
    //         if (UnityEngine.Random.Range(0, 65535) >= b)
    //         {
    //             break;
    //         }
    //         ++shakeCount;
    //     }
    //     return shakeCount;
    // }
    // #endregion

    // IEnumerator TryToRun()
    // {
    //     state = BattleState.Busy;

    //     if (isTrainerBattle)
    //     {
    //         yield return dialogBox.TypeDialog("");
    //         state = BattleState.RunningTurn;
    //         yield break;
    //     }

    //     ++escapeAttempts;

    //     int playerSpeed = playerUnit.BattlePokemon.Speed;
    //     int enemySpeed = enemyUnit.BattlePokemon.Speed;

    //     if (enemySpeed <= playerSpeed)
    //     {
    //         yield return dialogBox.TypeDialog("무사히 도망쳤다!");
    //         BattleOver(true);
    //     }
    //     else
    //     {
    //         float f = (playerSpeed * 128) / (enemySpeed + 30 * escapeAttempts);
    //         f = f % 256;

    //         if (UnityEngine.Random.Range(0, 256) < f)
    //         {
    //             yield return dialogBox.TypeDialog("무사히 도망쳤다!");
    //             BattleOver(true);
    //         }
    //         else
    //         {
    //             yield return dialogBox.TypeDialog("도망칠 수 없었다!");
    //             state = BattleState.RunningTurn;
    //             // yield break;

    //         }
    //     }
    // }


    string GetCorrectParticle(string name, string particleType)    //은는이가
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
}
