using System.Collections;
using System;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;
using System.Collections.Generic;

public enum BattleAction
{
    Skill,
    SwitchPokemon,
    UseItem,
    Run
}
public enum BattleState
{
    None,
    Start,
    Action,
    Skill,
    RunningTurn,
    Busy,
    PartyScreen,
    SkillToForget,
    ConfirmBox,
    Evolution,
    BattleOver,
    ConfigSelection,
    Dialog
}
public class BattleSystem : MonoBehaviour
{
    public static BattleSystem Inst;
    [SerializeField] public BattleUnit playerUnit;
    [SerializeField] public BattleUnit enemyUnit;
    BattleDialogBox dialogBox;
    // [SerializeField] ConfigPanel configPanel;
    [SerializeField] GameObject Pokeball;
    // [SerializeField] GameObject ConfirmBox;

    //Party
    // [SerializeField] PartyScreen partyScreen;
    // [SerializeField] SkillSelectScreen skillSelectScreen;

    // [HideInInspector] public static BattleState state;
    BattleState preState;
    // int currentAction = 0;
    // int currentSkill = 0;
    // int currentMember = 0;
    // int currentSelection = 0;
    // int currentConfirm = 0;
    int escapeAttempts = 0;

    PokemonParty playerParty;
    PokemonParty trainerParty;
    Pokemon wildPokemon;
    bool isTrainerBattle = false;

    PlayerCtrl player;
    Image PlayerImage;
    TrainerCtrl trainer;

    SkillBase skillToLearn;

    private void Awake()
    {
        Inst = this;

        player = GameManager.Inst.playerCtrl;
        dialogBox = GameManager.Inst.dialogBox;
    }
    private void Start()
    {
        // state = BattleState.Start;
        // currentAction = 0;
        // PlayerImage.sprite = player.TrainerSprite;
        // PlayerImage.gameObject.SetActive(false);
    }
    public void StartBattle(PokemonParty playerParty, Pokemon wildPokemon)
    {
        GameManager.state = BattleState.Start;
        isTrainerBattle = false;
        this.playerParty = playerParty;
        this.wildPokemon = wildPokemon;
        StartCoroutine(SetUpBattle());
    }
    public void StartTrainerBattle(PokemonParty playerParty, PokemonParty trainerParty)
    {
        this.playerParty = playerParty;
        this.trainerParty = trainerParty;

        isTrainerBattle = true;

        player = playerParty.GetComponent<PlayerCtrl>();
        trainer = trainerParty.GetComponent<TrainerCtrl>();

        StartCoroutine(SetUpBattle());
    }
    /*
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
        if (Input.GetKeyDown(KeyCode.G))
        {
            Debug.Log(currentMember);
        }
        if (Input.GetKeyDown(KeyCode.F))
        {
            Debug.Log(playerUnit.BattlePokemon.Attack);
            Debug.Log(playerUnit.BattlePokemon.Rankup[0]);
        }
        if (Input.GetKeyDown(KeyCode.T))
        {
            Debug.Log($"{playerUnit.BattlePokemon.PokemonGen}");
        }
    }
*/
    public IEnumerator SetUpBattle()
    {
        if (isTrainerBattle == false)
        {
            playerUnit.SetUp(playerParty.GetHealthyPokemon());
            enemyUnit.SetUp(wildPokemon);

            dialogBox.SetSkillNames(playerUnit.BattlePokemon.Skills);
            GameManager.Inst.skillCount = playerUnit.BattlePokemon.Skills.Count;
            yield return dialogBox.TypeDialog($"앗! 야생 {enemyUnit.BattlePokemon.P_Base.PokemonName}{GetCorrectParticle(enemyUnit.BattlePokemon.P_Base.PokemonName, "subject")} \n튀어나왔다!");
        }
        else
        {
            playerUnit.gameObject.SetActive(false);
            enemyUnit.gameObject.SetActive(false);

            PlayerImage.gameObject.SetActive(true);
            // PlayerImage.gameObject.SetActive(true);

            PlayerImage.sprite = player.TrainerSprite;
            // PlayerImage.sprite = trainer.TrainerSprite;

            yield return dialogBox.TypeDialog($"{trainer.TrainerName}{/*은는이가*/""}이 배틀을 걸어왔다!");
        }
        escapeAttempts = 0;
        GameManager.Inst.partyScreen.Init();
        GameManager.Inst.ActionSelection();
    }
    void BattleOver(bool won)
    {
        if (GameManager.state == BattleState.Dialog)
        {
            StartCoroutine(new WaitUntil(() => GameManager.state != BattleState.Dialog));
            GameManager.state = BattleState.BattleOver;
        }
        GameManager.state = BattleState.BattleOver;
        StopAllCoroutines();
        GameManager.Inst.EndBattle(won);
    }
    void ChooseSkillToForget(Pokemon pokemon, SkillBase newSkill)
    {
        GameManager.state = BattleState.Busy;
        StartCoroutine(dialogBox.TypeDialog($"어느 기술을 잊게 하고싶은가?"));
        GameManager.Inst.skillSelectScreen.gameObject.SetActive(true);
        GameManager.Inst.skillSelectScreen.SetPokemonData(pokemon);
        GameManager.Inst.skillSelectScreen.SetSkill(pokemon.Skills.Select(x => x.SkillBase).ToList(), newSkill);
        skillToLearn = newSkill;

        GameManager.state = BattleState.SkillToForget;
    }

    #region BattleSystem
    public IEnumerator RunTurns(BattleAction playerAction)
    {
        GameManager.state = BattleState.RunningTurn;
        if (playerAction == BattleAction.Skill)
        {
            playerUnit.BattlePokemon.CurrentSkill = playerUnit.BattlePokemon.Skills[GameManager.Inst.currentSkill];
            enemyUnit.BattlePokemon.CurrentSkill = enemyUnit.BattlePokemon.GetRandomSkill();

            if (playerUnit.BattlePokemon.CurrentSkill == null || enemyUnit.BattlePokemon.CurrentSkill == null)
            {
                yield break;
            }
            int playerPriority = playerUnit.BattlePokemon.CurrentSkill.SkillBase.Priority;
            int enemyPriority = enemyUnit.BattlePokemon.CurrentSkill.SkillBase.Priority;

            int playerSpeed = playerUnit.BattlePokemon.Speed;
            int enemySpeed = enemyUnit.BattlePokemon.Speed;

            Debug.Log($"🕊️ 플레이어 스킬 우선도: {playerPriority}, 속도: {playerSpeed}");
            Debug.Log($"🐍 적 스킬 우선도: {enemyPriority}, 속도: {enemySpeed}");

            // 스피드 체크
            bool playerTurnFirst = true;
            //우선도 체크
            if (enemyPriority > playerPriority)
            {
                playerTurnFirst = false;
            }
            else if (playerPriority == enemyPriority)
            {
                if (playerSpeed == enemySpeed)
                {
                    playerTurnFirst = UnityEngine.Random.Range(0, 2) == 0;
                }
                else
                {
                    playerTurnFirst = playerSpeed > enemySpeed;
                }
            }

            var firstUnit = playerTurnFirst ? playerUnit : enemyUnit;
            var secondUnit = playerTurnFirst ? enemyUnit : playerUnit;

            var targetOfFirst = secondUnit;
            var targetOfSecond = firstUnit;

            // 1. 선공자 행동
            yield return RunSkill(firstUnit, targetOfFirst, firstUnit.BattlePokemon.CurrentSkill);
            yield return RunAfterTrun(firstUnit);

            if (GameManager.state == BattleState.BattleOver)
            {
                yield break;
            }
            // 선공자가 공격한 대상 쓰러짐 확인
            if (targetOfFirst.BattlePokemon == null || targetOfFirst.BattlePokemon.PokemonHp <= 0)
            {
                Debug.Log("⛔ 뒤짐");
                yield return HandlePokemonFainted(targetOfFirst);
                yield return CheckForBattleOver(targetOfFirst);
                yield break;
            }

            // 2. 후공자 행동 (자기와 대상 모두 살아 있을 때만)
            if (secondUnit.BattlePokemon != null && secondUnit.BattlePokemon.PokemonHp > 0 && targetOfSecond.BattlePokemon != null && targetOfSecond.BattlePokemon.PokemonHp > 0)
            {
                yield return RunSkill(secondUnit, targetOfSecond, secondUnit.BattlePokemon.CurrentSkill);
                yield return RunAfterTrun(secondUnit);

                if (GameManager.state == BattleState.BattleOver)
                {
                    yield break;
                }
                if (targetOfSecond.BattlePokemon != null && targetOfSecond.BattlePokemon.PokemonHp <= 0)
                {
                    yield return HandlePokemonFainted(targetOfSecond);
                    yield return CheckForBattleOver(targetOfSecond);
                    yield break;
                }
            }
            else
            {
                Debug.Log("⛔ 후공자 또는 대상이 쓰러진 상태. 후공 행동 생략.");
                yield break;
            }
        }
        //스킬 외 행동
        else
        {
            Debug.Log($"▶ 플레이어가 {playerAction} 선택");

            if (playerAction == BattleAction.SwitchPokemon)
            {
                var selectedPokemon = playerParty.Party[GameManager.Inst.currentMember];
                preState = BattleState.RunningTurn;
                GameManager.state = BattleState.Busy;
                yield return SwitchPokemon(selectedPokemon);
            }
            else if (playerAction == BattleAction.UseItem)
            {
                preState = GameManager.state;
                GameManager.state = BattleState.Busy;
                dialogBox.EnableActionSelector(false);
                yield return ThrowPokeball();
            }
            else if (playerAction == BattleAction.Run)
            {
                preState = GameManager.state;
                GameManager.state = BattleState.Busy;
                yield return TryToRun();
            }

            if (GameManager.state == BattleState.BattleOver)
            {
                Debug.Log("🏁 아이템/교체/도망 후 전투 종료됨");
                yield break;
            }

            // 적 턴
            var enemySkill = enemyUnit.BattlePokemon.GetRandomSkill();

            if (enemyUnit.BattlePokemon == null || enemyUnit.BattlePokemon.PokemonHp <= 0)
            {
                Debug.LogWarning("❗ 적 포켓몬이 쓰러졌음. 적 턴 스킵.");
            }
            else
            {
                yield return RunSkill(enemyUnit, playerUnit, enemySkill);
                yield return RunAfterTrun(enemyUnit);

                if (GameManager.state == BattleState.BattleOver)
                {
                    Debug.Log("🏁 적 턴 종료 후 전투 종료됨");
                }

                if (playerUnit.BattlePokemon.PokemonHp <= 0)
                {
                    Debug.Log("⚠️ 플레이어 포켓몬 쓰러짐");
                    yield return HandlePokemonFainted(playerUnit);
                    yield return CheckForBattleOver(playerUnit);
                    yield break;
                }
            }
        }

        //계속 턴 진행
        if (GameManager.state != BattleState.BattleOver)
        {
            // Debug.Log("✅ RunTurns 종료");
            GameManager.Inst.ActionSelection();
        }
    }
    IEnumerator RunSkill(BattleUnit sourceUnit, BattleUnit targetUnit, Skill skill)
    {
        bool canRunSkill = sourceUnit.BattlePokemon.OnBeforeSkill();
        if (canRunSkill == false)
        {
            yield return ShowStatusChanges(sourceUnit.BattlePokemon);
            yield return sourceUnit.BattleHud.UpdateHp();
            yield break;
        }
        yield return ShowStatusChanges(sourceUnit.BattlePokemon);

        //공격 애니메이션
        skill.PP--;

        yield return dialogBox.TypeDialog($"{sourceUnit.BattlePokemon.P_Base.PokemonName}의 {skill.SkillBase.SkillName}!");
        if (CheckSkillHits(skill, sourceUnit.BattlePokemon, targetUnit.BattlePokemon))
        {
            //변화기 처리
            if (skill.SkillBase.CategoryKey == CategoryKey.Status)
            {
                //중복 상태이상 체크
                if (targetUnit.BattlePokemon.Status != null)
                {
                    yield return dialogBox.TypeDialog("효과가 없는 것 같다...");
                }
                else
                {
                    yield return RunSkillEffects(skill.SkillBase.Effects, sourceUnit.BattlePokemon, targetUnit.BattlePokemon, skill.SkillBase.Target);
                }
            }
            else
            {
                //피격 애니메이션
                targetUnit.PlayHitAnimation();
                var (startHp, endHp, damageDetails) = targetUnit.BattlePokemon.TakeDamage(skill, sourceUnit.BattlePokemon);

                StartCoroutine(targetUnit.BattleHud.UpdateHp());
                yield return ShowDamageDetails(damageDetails);
            }
            //추가효과 존재 시
            if (skill.SkillBase.SecondaryEffects != null && skill.SkillBase.SecondaryEffects.Count > 0 && targetUnit.BattlePokemon.PokemonHp > 0)
            {
                foreach (var secondary in skill.SkillBase.SecondaryEffects)
                {
                    var rnd = UnityEngine.Random.Range(1, 101);
                    if (rnd <= secondary.Chance)
                    {
                        yield return RunSkillEffects(secondary, sourceUnit.BattlePokemon, targetUnit.BattlePokemon, secondary.Target);
                    }
                }
            }
            if (targetUnit.BattlePokemon.PokemonHp <= 0)
            {
                yield return HandlePokemonFainted(targetUnit);
            }
        }
        else
        {
            if (sourceUnit.IsPlayerUnit)
            {
                yield return dialogBox.TypeDialog($"상대 {targetUnit.BattlePokemon.P_Base.PokemonName}에게는 \n맞지 않았다!");
            }
            else
            {
                yield return dialogBox.TypeDialog($"{targetUnit.BattlePokemon.P_Base.PokemonName}에게는 맞지 않았다!");
            }
        }
    }
    IEnumerator RunAfterTrun(BattleUnit sourceUnit)
    {
        if (GameManager.state == BattleState.BattleOver)
        {
            yield break;
        }

        yield return new WaitUntil(() => GameManager.state == BattleState.RunningTurn);
        yield return new WaitForSeconds(1.2f);

        sourceUnit.BattlePokemon.OnAfterTurn();

        yield return ShowStatusChanges(sourceUnit.BattlePokemon);
        yield return sourceUnit.BattleHud.UpdateHp();

        //상태이상 데미지로 죽으면 @@@@
        if (sourceUnit.BattlePokemon.PokemonHp <= 0)
        {
            yield return HandlePokemonFainted(sourceUnit);
            yield return new WaitUntil(() => GameManager.state == BattleState.RunningTurn);
            // yield return dialogBox.TypeDialog($"{sourceUnit.BattlePokemon.PokemonBase.PokemonName}{GetCorrectParticle(sourceUnit.BattlePokemon.PokemonBase.PokemonName, false)} 쓰러졌다!");
            // /*
            // 사망 애니메이션 재생

            // */

            // yield return new WaitForSeconds(2.0f);
        }

        yield return new WaitForSeconds(1.0f);

    }
    //상태이상, 랭크업 처리
    IEnumerator RunSkillEffects(SkillEffects effects, Pokemon sourceUnit, Pokemon targetUnit, SkillTarget skillTarget)
    {
        //RankUp
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
        //상태이상
        if (effects.Status != ConditionID.None)
        {
            targetUnit.SetStatus(effects.Status);
        }
        //일시 상태이상
        if (effects.VolatileStatus != ConditionID.None)
        {
            targetUnit.SetVolatileStatus(effects.VolatileStatus);
        }

        yield return ShowStatusChanges(sourceUnit);
        yield return ShowStatusChanges(targetUnit);
    }
    //명중률 계산
    bool CheckSkillHits(Skill skill, Pokemon source, Pokemon target)
    {
        if (skill.SkillBase.AlwaysHits)
        {
            return true;
        }
        float SkillAccuracy = skill.SkillBase.SkillAccuracy;
        int accuracy = source.Rankup[Stat.Accuracy];
        int evasion = target.Rankup[Stat.Evasion];

        var rankupValues = new float[] { 1f, 1.5f, 2f, 2.5f, 3f, 3.5f, 4f };

        if (accuracy > 0)
        {
            SkillAccuracy *= rankupValues[accuracy];
        }
        else
        {
            SkillAccuracy /= rankupValues[-accuracy];
        }
        if (evasion > 0)
        {
            SkillAccuracy /= rankupValues[evasion];
        }
        else
        {
            SkillAccuracy *= rankupValues[-evasion];
        }


        return UnityEngine.Random.Range(1, 101) <= SkillAccuracy;
    }
    //랭크업 메시지 출력
    IEnumerator ShowStatusChanges(Pokemon pokemon)
    {
        while (pokemon.StatusCngMsg.Count > 0)
        {
            string message = pokemon.StatusCngMsg.Dequeue();
            yield return dialogBox.TypeDialog(message);
        }
    }
    IEnumerator HandlePokemonFainted(BattleUnit faintedUnit)
    {
        yield return dialogBox.TypeDialog($"{faintedUnit.BattlePokemon.P_Base.PokemonName}{GetCorrectParticle(faintedUnit.BattlePokemon.P_Base.PokemonName, "topic")} 쓰러졌다!");
        //사망 애니메이션 재생 @@@@
        yield return new WaitForSeconds(1.5f);
        //플레이어 승리
        if (!faintedUnit.IsPlayerUnit)
        {
            int expYield = faintedUnit.BattlePokemon.P_Base.ExpYield;
            int enemyLevel = faintedUnit.BattlePokemon.PokemonLevel;
            float trainerBonus = (isTrainerBattle) ? 1.5f : 1.0f;

            int expGain = Mathf.FloorToInt(expYield * enemyLevel * trainerBonus / 7);
            playerUnit.BattlePokemon.PokemonExp += expGain;
            yield return playerUnit.BattleHud.SetExpSmooth();
            yield return dialogBox.TypeDialog($"{playerUnit.BattlePokemon.P_Base.PokemonName}{GetCorrectParticle(playerUnit.BattlePokemon.P_Base.PokemonName, "topic")}\n{expGain}경험치를 얻었다!");
            yield return CheckLearnableSkill();
        }
        yield return CheckForBattleOver(faintedUnit);
        GameManager.Inst.AddGold();
    }
    public IEnumerator SwitchPokemon(Pokemon newPokemon)
    {
        playerUnit.BattlePokemon.CureVolatileStatus();
        playerUnit.BattlePokemon.ResetRankup();

        yield return dialogBox.TypeDialog($"돌아와 {playerUnit.BattlePokemon.P_Base.PokemonName}!");
        //귀환 애니메이션@@@

        yield return new WaitForSeconds(1.5f);

        // 현재 전투 중인 포켓몬 (인덱스 0에 있는 포켓몬)
        var currentBattlePokemon = playerParty.Party[0];

        // 교체 작업: 교체할 포켓몬을 0번 인덱스로, 나가있는 포켓몬을 교체할 포켓몬의 인덱스로 이동
        playerParty.Party[0] = newPokemon;
        playerParty.Party[GameManager.Inst.currentMember] = currentBattlePokemon;

        playerUnit.SetUp(newPokemon);
        dialogBox.SetSkillNames(newPokemon.Skills);

        GameManager.Inst.skillCount = newPokemon.Skills.Count;

        yield return dialogBox.TypeDialog($"가랏! {newPokemon.P_Base.PokemonName}!");
        GameManager.state = preState;
    }
    [HideInInspector] public bool cancelSelected = false;
    IEnumerator CheckLearnableSkill()
    {
        while (playerUnit.BattlePokemon.CheckForLevelUp())
        {
            playerUnit.BattleHud.SetLevel();
            yield return dialogBox.TypeDialog($"{playerUnit.BattlePokemon.P_Base.PokemonName}{GetCorrectParticle(playerUnit.BattlePokemon.P_Base.PokemonName, "topic")} 레벨 {playerUnit.BattlePokemon.PokemonLevel}로 올랐다!");

            var newSkill = playerUnit.BattlePokemon.GetLearnableSkill();
            if (newSkill != null)
            {
                if (playerUnit.BattlePokemon.Skills.Count < PokemonBase.MaxNumOfSkills)
                {
                    playerUnit.BattlePokemon.LearnSkill(newSkill);
                    yield return dialogBox.TypeDialog($"{playerUnit.BattlePokemon.P_Base.PokemonName}{GetCorrectParticle(playerUnit.BattlePokemon.P_Base.PokemonName, "topic")} 새로 {newSkill.SkillBase.SkillName}{GetCorrectParticle(newSkill.SkillBase.SkillName, "object")} 배웠다!");
                    dialogBox.SetSkillNames(playerUnit.BattlePokemon.Skills);
                }
                else
                {
                    bool isFinalDecisionMade = false;
                    while (!isFinalDecisionMade)
                    {
                        yield return dialogBox.TypeDialog($"{playerUnit.BattlePokemon.P_Base.PokemonName}{GetCorrectParticle(playerUnit.BattlePokemon.P_Base.PokemonName, "topic")} 새로 {newSkill.SkillBase.SkillName}{GetCorrectParticle(newSkill.SkillBase.SkillName, "object")} 배우고 싶어한다!");
                        yield return dialogBox.TypeDialog($"하지만 기술이 4개이므로 다른 기술을 잊어야 한다.");
                        yield return dialogBox.TypeDialog($"{newSkill.SkillBase.SkillName} 대신 다른 기술을 잊게 하겠습니까?");

                        GameManager.Inst.ConfirmBoxSelection();
                        yield return new WaitUntil(() => GameManager.state != BattleState.ConfirmBox);
                        bool isConfirmed = GameManager.Inst.HandleConfirmBoxSelection();

                        if (isConfirmed)
                        {
                            ChooseSkillToForget(playerUnit.BattlePokemon, newSkill.SkillBase);
                            yield return new WaitUntil(() => GameManager.state == BattleState.Busy);

                            if (cancelSelected || GameManager.Inst.currentSelection == PokemonBase.MaxNumOfSkills)
                            {
                                cancelSelected = false;

                                yield return dialogBox.TypeDialog($"그럼... {newSkill.SkillBase.SkillName}{GetCorrectParticle(newSkill.SkillBase.SkillName, "object")} 배우는 것을 포기하겠습니까?");
                                GameManager.Inst.ConfirmBoxSelection();
                                yield return new WaitUntil(() => GameManager.state != BattleState.ConfirmBox);
                                bool giveUp = GameManager.Inst.HandleConfirmBoxSelection();

                                if (giveUp)
                                {
                                    yield return dialogBox.TypeDialog($"{playerUnit.BattlePokemon.P_Base.PokemonName}{GetCorrectParticle(playerUnit.BattlePokemon.P_Base.PokemonName, "topic")} 결국 {newSkill.SkillBase.SkillName}{GetCorrectParticle(newSkill.SkillBase.SkillName, "object")} 배우지 않았다!");
                                    skillToLearn = null;
                                    isFinalDecisionMade = true;
                                }
                                else
                                {
                                    ChooseSkillToForget(playerUnit.BattlePokemon, newSkill.SkillBase);
                                    yield return new WaitUntil(() => GameManager.state == BattleState.SkillToForget);
                                }
                            }
                            else
                            {
                                var oldSkill = playerUnit.BattlePokemon.Skills[GameManager.Inst.currentSelection].SkillBase;
                                playerUnit.BattlePokemon.Skills[GameManager.Inst.currentSelection] = new Skill(newSkill.SkillBase);

                                yield return dialogBox.TypeDialog("1, 2, ... 짠!");
                                yield return dialogBox.TypeDialog($"{playerUnit.BattlePokemon.P_Base.PokemonName}{GetCorrectParticle(playerUnit.BattlePokemon.P_Base.PokemonName, "topic")} {oldSkill.SkillName}{GetCorrectParticle(oldSkill.SkillName, "object")} 깨끗이 잊었다!");
                                yield return dialogBox.TypeDialog($"그리고 {newSkill.SkillBase.SkillName}{GetCorrectParticle(newSkill.SkillBase.SkillName, "object")} 배웠다!");

                                dialogBox.SetSkillNames(playerUnit.BattlePokemon.Skills);
                                skillToLearn = null;
                                isFinalDecisionMade = true;
                            }
                        }
                        else
                        {
                            yield return dialogBox.TypeDialog($"그럼... {newSkill.SkillBase.SkillName}{GetCorrectParticle(newSkill.SkillBase.SkillName, "object")} 배우는 것을 포기하겠습니까?");
                            GameManager.Inst.ConfirmBoxSelection();
                            yield return new WaitUntil(() => GameManager.state != BattleState.ConfirmBox);
                            bool isReallyConfirmed = GameManager.Inst.HandleConfirmBoxSelection();

                            if (isReallyConfirmed)
                            {
                                yield return dialogBox.TypeDialog($"{playerUnit.BattlePokemon.P_Base.PokemonName}{GetCorrectParticle(playerUnit.BattlePokemon.P_Base.PokemonName, "topic")} 결국 {newSkill.SkillBase.SkillName}{GetCorrectParticle(newSkill.SkillBase.SkillName, "object")} 배우지 않았다!");
                                skillToLearn = null;
                                isFinalDecisionMade = true;
                            }
                        }
                    }
                }
            }

            yield return playerUnit.BattleHud.SetExpSmooth(true);
        }
    }
    IEnumerator CheckForBattleOver(BattleUnit faintedUnit)
    {
        if (faintedUnit.IsPlayerUnit)
        {
            var nextPokemon = playerParty.GetHealthyPokemon();
            if (nextPokemon != null)
            {
                preState = GameManager.state;
                GameManager.Inst.OpenPartyScreen();
            }
            else
            {
                BattleOver(false);
            }
        }
        else
        {
            //야생포켓몬
            if (!isTrainerBattle)
            {
                yield return playerParty.CheckForEvolutions();
                yield return new WaitForSeconds(0.5f); // 진화 마무리 대기
                BattleOver(true);
            }
            else
            {
                var nextPokemon = trainerParty.GetHealthyPokemon();
                if (nextPokemon != null)
                {
                    //다음포케
                    yield return playerParty.CheckForEvolutions();

                }
                else
                {
                    yield return playerParty.CheckForEvolutions();

                    BattleOver(true);
                }
            }
        }
    }
    IEnumerator ShowDamageDetails(DamageDetails damageDetails)
    {
        if (damageDetails.Critical > 1f)
        {
            yield return dialogBox.TypeDialog("급소에 맞았다!");
        }
        if (damageDetails.TypeEffectiveness > 1)
        {
            if (damageDetails.TypeEffectiveness > 2)
            {
                yield return dialogBox.TypeDialog("효과가 굉장했다!!");
            }
            else
            {
                yield return dialogBox.TypeDialog("효과가 대단했다!");
            }
        }

        else if (damageDetails.TypeEffectiveness < 1f)
        {
            if (damageDetails.TypeEffectiveness == 0)
            {
                yield return dialogBox.TypeDialog("효과가 없는 듯 하다...");
            }
            else
            {
                yield return dialogBox.TypeDialog("효과가 별로인듯 하다...");
            }
        }
    }
    #endregion
    #region Catch
    IEnumerator ThrowPokeball()
    {
        if (isTrainerBattle)
        {
            yield return dialogBox.TypeDialog("다른 트레이너의 포켓몬은 잡을 수 없다!");
            GameManager.state = preState;
            yield break;
        }

        var pokeballObj = Instantiate(Pokeball, playerUnit.transform.position, Quaternion.identity);
        var pokeball = pokeballObj.GetComponent<SpriteRenderer>();

        //#34 1254
        //pokeball.transform.DoMove

        int shakeCount = TryToCatchPokemon(enemyUnit.BattlePokemon);
        for (int i = 0; i < Math.Min(shakeCount, 3); ++i)
        {
            yield return new WaitForSeconds(0.5f);
            //흔들기 애니메이션
        }
        if (shakeCount == 4)
        {
            //잡힘
            playerParty.AddPokemon(enemyUnit.BattlePokemon);

            GlobalValue.CatchPokemon(enemyUnit.BattlePokemon.P_Base, false);

            Destroy(pokeball);
            yield return dialogBox.TypeDialog($"신난다-!\n야생 {enemyUnit.BattlePokemon.P_Base.PokemonName}을 잡았다!");
            BattleOver(true);
        }
        else
        {
            yield return dialogBox.TypeDialog($"안잡힘!");
            Destroy(pokeball);
            GameManager.state = preState;
        }
        yield return new WaitForSeconds(1.0f);
    }
    int TryToCatchPokemon(Pokemon pokemon)
    {
        float a = (3 * pokemon.MaxHp - 2 * pokemon.PokemonHp) * pokemon.P_Base.CatchRate * ConditionsDB.GetStatusBonus(pokemon.Status) / (3 * pokemon.MaxHp);

        if (a >= 255)
        {
            //흔들린 횟수
            return 4;
        }

        float b = 1048560 / Mathf.Sqrt(Mathf.Sqrt(16711680 / a));

        int shakeCount = 0;
        while (shakeCount < 4)
        {
            if (UnityEngine.Random.Range(0, 65535) >= b)
            {
                break;
            }
            ++shakeCount;
        }
        return shakeCount;
    }
    #endregion

    IEnumerator TryToRun()
    {
        if (isTrainerBattle)
        {
            yield return dialogBox.TypeDialog("트레이너 배틀 중 도망 x");
            GameManager.state = preState;
            yield break;
        }

        ++escapeAttempts;
        int playerSpeed = playerUnit.BattlePokemon.Speed;
        int enemySpeed = enemyUnit.BattlePokemon.Speed;

        if (enemySpeed <= playerSpeed)
        {
            yield return dialogBox.TypeDialog("무사히 도망쳤다!");
            BattleOver(true);
        }
        else
        {
            float f = (playerSpeed * 128) / (enemySpeed + 30 * escapeAttempts);
            f = f % 256;

            if (UnityEngine.Random.Range(0, 256) < f)
            {
                yield return dialogBox.TypeDialog("무사히 도망쳤다!");
                BattleOver(true);
            }
            else
            {
                yield return dialogBox.TypeDialog("도망칠 수 없었다!");
                GameManager.state = preState;
            }
        }
    }


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
