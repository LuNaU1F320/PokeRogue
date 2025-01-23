using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Unity.VisualScripting;

public enum BattleState
{
    Start,
    ActionSelection,
    SkillSelection,
    RunningTurn,
    Busy,
    PartyScreen,
    SkillToForget,
    BattleOver
}
public enum BattleAction
{
    Skill,
    SwitchPokemon,
    UseItem,
    Run
}
public class BattleSystem : MonoBehaviour
{
    [SerializeField] BattleUnit playerUnit;
    [SerializeField] BattleUnit enemyUnit;
    [SerializeField] BattleDialogBox dialogBox;
    [SerializeField] GameObject Pokeball;

    [SerializeField] SpriteRenderer PlayerSprite;
    [SerializeField] SpriteRenderer TrainerSprite;

    //Party
    [SerializeField] PartyScreen partyScreen;
    [SerializeField] SkillSelectScreen skillSelectScreen;

    // public event Action<bool> OnBattleOver;

    BattleState state;
    BattleState? preState;
    int currentAction = 0;
    int currentSkill = 0;
    int currentMember = 0;
    int skillCount = 0;
    int escapeAttempts = 0;

    PokemonParty playerParty;
    PokemonParty trainerParty;
    Pokemon wildPokemon;
    bool isTrainerBattle = false;

    PlayerCtrl player;
    TrainerCtrl trainer;

    SkillBase skillToLearn;

    private void Start()
    {
        state = BattleState.Start;
        currentAction = 0;
        // playerParty = FindObjectOfType<PokemonParty>().GetComponent<PokemonParty>();
        // wildPokemon = FindObjectOfType<MapArea>().GetComponent<MapArea>().GetRandomWildPokemon();
        // StartCoroutine(SetUpBattle());
    }
    public void StartBattle(PokemonParty playerParty, Pokemon wildPokemon)
    {
        state = BattleState.Start;
        isTrainerBattle = false;
        this.playerParty = playerParty;
        this.wildPokemon = wildPokemon;
        isTrainerBattle = false;
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
    public void Update()
    {
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
            Action<int> onSkillSelected = (skillIndex) =>
            {
                skillSelectScreen.gameObject.SetActive(false);
                if (skillIndex == PokemonBase.MaxNumOfSkills)
                {
                    //배우지않음

                }
                else
                {
                    var selectedSkill = playerUnit.BattlePokemon.Skills[skillIndex].SkillBase;
                    //텍스트출력
                    playerUnit.BattlePokemon.Skills[skillIndex] = new Skill(skillToLearn);
                }
                skillToLearn = null;
                state = BattleState.RunningTurn;

            };
            skillSelectScreen.HandleSkillSelection(onSkillSelected);
            /*
            if (skillSelectScreen.SelectionMade)
            {
                int skillIndex = skillSelectScreen.SelectedSkillIndex;
                skillSelectScreen.SelectionMade = false; // 상태 초기화

                if (skillIndex == PokemonBase.MaxNumOfSkills)
                {
                    // 배우지 않음 처리
                    skillToLearn = null;
                }
                else
                {
                    // 선택된 스킬을 배우는 처리
                    var selectedSkill = playerUnit.BattlePokemon.Skills[skillIndex].SkillBase;
                    playerUnit.BattlePokemon.Skills[skillIndex] = new Skill(skillToLearn);
                }

                skillToLearn = null;
                state = BattleState.RunningTurn;
            }
            */
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
            // Debug.Log($"state {state}");
            Debug.Log($"{playerUnit.BattlePokemon.PokemonGen}");
        }
    }

    public IEnumerator SetUpBattle()
    {
        if (isTrainerBattle == false)
        {
            playerUnit.SetUp(playerParty.GetHealthyPokemon());
            enemyUnit.SetUp(wildPokemon);

            dialogBox.SetSkillNames(playerUnit.BattlePokemon.Skills);

            skillCount = playerUnit.BattlePokemon.Skills.Count;

            yield return dialogBox.TypeDialog($"앗! 야생 {enemyUnit.BattlePokemon.P_Base.PokemonName}{GetCorrectParticle(enemyUnit.BattlePokemon.P_Base.PokemonName, "subject")} \n튀어나왔다!");
        }
        else
        {
            playerUnit.gameObject.SetActive(false);
            enemyUnit.gameObject.SetActive(false);

            PlayerSprite.gameObject.SetActive(true);
            TrainerSprite.gameObject.SetActive(true);

            PlayerSprite.sprite = player.TrainerSprite;
            TrainerSprite.sprite = trainer.TrainerSprite;

            yield return dialogBox.TypeDialog($"{trainer.TrainerName}{/*은는이가*/""}이 배틀을 걸어왔다!");
        }
        escapeAttempts = 0;
        partyScreen.Init();
        ActionSelection();
    }
    void BattleOver(bool won)
    {
        state = BattleState.BattleOver;
        StopAllCoroutines();
        GameManager.Inst.EndBattle(won);
    }
    void ActionSelection()
    {
        state = BattleState.ActionSelection;
        dialogBox.SetDialog($"{playerUnit.BattlePokemon.P_Base.PokemonName}{GetCorrectParticle(playerUnit.BattlePokemon.P_Base.PokemonName, "topic")} 무엇을 할까?");
        dialogBox.EnableActionSelector(true);
    }
    void OpenPartyScreen()
    {
        state = BattleState.PartyScreen;
        partyScreen.Init();
        partyScreen.SetPartyData(playerParty.Party);
        partyScreen.gameObject.SetActive(true);
    }
    void SkillSelection()
    {
        state = BattleState.SkillSelection;
        dialogBox.EnableActionSelector(false);
        dialogBox.EnableDialogText(false);
        dialogBox.EnableSkillSelector(true);
    }
    IEnumerator ChooseSkillToForget(Pokemon pokemon, SkillBase newSkill)
    {
        state = BattleState.Busy;
        yield return dialogBox.TypeDialog($"{newSkill.SkillName}대신 다른 기술을 잊게 하겠습니까?");
        //예/아니오

        yield return dialogBox.TypeDialog($"어느 기술을 잊게 하고싶은가?");
        skillSelectScreen.gameObject.SetActive(true);
        skillSelectScreen.SetSkill(pokemon.Skills.Select(x => x.SkillBase).ToList(), newSkill);
        skillToLearn = newSkill;

        state = BattleState.SkillToForget;
    }
    IEnumerator RunTurns(BattleAction playerAction)
    {
        state = BattleState.RunningTurn;
        if (playerAction == BattleAction.Skill)
        {
            playerUnit.BattlePokemon.CurrentSkill = playerUnit.BattlePokemon.Skills[currentSkill];
            enemyUnit.BattlePokemon.CurrentSkill = enemyUnit.BattlePokemon.GetRandomSkill();

            int playerPriority = playerUnit.BattlePokemon.CurrentSkill.SkillBase.Priority;
            int enemyPriority = enemyUnit.BattlePokemon.CurrentSkill.SkillBase.Priority;


            // 스피드 체크
            bool playerTurnFirst = true;
            //우선도 체크
            if (enemyPriority > playerPriority)
            {
                playerTurnFirst = false;
            }
            else if (playerPriority == enemyPriority)
            {
                if (playerUnit.BattlePokemon.Speed == enemyUnit.BattlePokemon.Speed)
                {
                    playerTurnFirst = UnityEngine.Random.Range(0, 2) == 0;
                }
                playerTurnFirst = playerUnit.BattlePokemon.Speed > enemyUnit.BattlePokemon.Speed;
            }

            var firstUnit = playerTurnFirst ? playerUnit : enemyUnit;
            var secondUnit = playerTurnFirst ? enemyUnit : playerUnit;


            var secondPokemon = secondUnit.BattlePokemon;

            //선턴
            yield return RunSkill(firstUnit, secondUnit, firstUnit.BattlePokemon.CurrentSkill);
            yield return RunAfterTrun(firstUnit);
            if (state == BattleState.BattleOver)
            {
                yield break;
            }
            if (secondPokemon.PokemonHp > 0)
            {
                //후턴
                yield return RunSkill(secondUnit, firstUnit, secondUnit.BattlePokemon.CurrentSkill);
                yield return RunAfterTrun(secondUnit);
                if (state == BattleState.BattleOver)
                {
                    yield break;
                }
            }
        }
        else
        {
            if (playerAction == BattleAction.SwitchPokemon)
            {
                var selectedPokemon = playerParty.Party[currentMember];
                state = BattleState.Busy;
                yield return SwitchPokemon(selectedPokemon);
            }
            else if (playerAction == BattleAction.UseItem)
            {
                dialogBox.EnableActionSelector(false);
                yield return ThrowPokeball();
            }
            else if (playerAction == BattleAction.Run)
            {
                yield return TryToRun();
            }

            var enemySkill = enemyUnit.BattlePokemon.GetRandomSkill();
            yield return RunSkill(enemyUnit, playerUnit, enemySkill);
            yield return RunAfterTrun(enemyUnit);
            if (state == BattleState.BattleOver)
            {
                yield break;
            }
        }

        if (state != BattleState.BattleOver)
        {
            ActionSelection();
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
        /*
        // 모든 스킬의 PP가 0인지 확인
        if (sourceUnit.BattlePokemon.Skills.TrueForAll(s => s.SkillPP <= 0))
        {
            // "발버둥" 기본 기술 사용
            yield return dialogBox.TypeDialog($"{sourceUnit.BattlePokemon.PokemonBase.PokemonName}은(는) 사용할 스킬이 없습니다! 발버둥을 사용합니다!");
            skill = struggleSkill; // 발버둥 기술로 대체
        }
        */

        // if (skill.SkillPP <= 0)
        // {
        //     // 행동 선택 상태로 복귀
        //     if (sourceUnit.IsPlayerUnit)
        //     {
        //         // 스킬 사용 불가 메시지 출력
        //         yield return dialogBox.TypeDialog($"기술의 남은 포인트가 없다!");
        //         ActionSelection();
        //     }
        //     yield break; // 현재 실행 종료
        // }

        skill.SkillPP--;

        yield return dialogBox.TypeDialog($"{sourceUnit.BattlePokemon.P_Base.PokemonName}의 {skill.SkillBase.SkillName}!");

        //공격 애니메이션

        //피격 애니메이션

        if (CheckSkillHits(skill, sourceUnit.BattlePokemon, targetUnit.BattlePokemon))
        {
            if (skill.SkillBase.CategoryKey == CategoryKey.Status)
            {
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
                var (startHp, endHp, damageDetails) = targetUnit.BattlePokemon.TakeDamage(skill, sourceUnit.BattlePokemon);

                StartCoroutine(targetUnit.BattleHud.UpdateHp());
                yield return ShowDamageDetails(damageDetails);
            }
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

                // yield return dialogBox.TypeDialog($"{targetUnit.BattlePokemon.PokemonBase.PokemonName}{GetCorrectParticle(targetUnit.BattlePokemon.PokemonBase.PokemonName, false)} 쓰러졌다!");
                // //애니메이션 재생

                // //플레이어 승리 (다음스테이지로)
                // yield return new WaitForSeconds(1.5f);
                // CheckForBattleOver(targetUnit);
                // GameManager.Inst.AddGold();
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
        if (state == BattleState.BattleOver)
        {
            yield break;
        }
        yield return new WaitUntil(() => state == BattleState.RunningTurn);
        yield return new WaitForSeconds(1.2f);
        //상태이상 처리
        sourceUnit.BattlePokemon.OnAfterTurn();
        yield return ShowStatusChanges(sourceUnit.BattlePokemon);
        yield return sourceUnit.BattleHud.UpdateHp();
        if (sourceUnit.BattlePokemon.PokemonHp <= 0)
        {
            yield return HandlePokemonFainted(sourceUnit);
            yield return new WaitUntil(() => state == BattleState.RunningTurn);
            // yield return dialogBox.TypeDialog($"{sourceUnit.BattlePokemon.PokemonBase.PokemonName}{GetCorrectParticle(sourceUnit.BattlePokemon.PokemonBase.PokemonName, false)} 쓰러졌다!");
            // /*
            // 사망 애니메이션 재생

            // */

            // yield return new WaitForSeconds(2.0f);
        }
    }
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
        //애니메이션 재생

        //플레이어 승리
        yield return new WaitForSeconds(1.5f);

        if (!faintedUnit.IsPlayerUnit)
        {
            int expYield = faintedUnit.BattlePokemon.P_Base.ExpYield;
            int enemyLevel = faintedUnit.BattlePokemon.PokemonLevel;
            float trainerBonus = (isTrainerBattle) ? 1.5f : 1.0f;

            int expGain = Mathf.FloorToInt(expYield * enemyLevel * trainerBonus / 7);
            playerUnit.BattlePokemon.PokemonExp += expGain;
            yield return dialogBox.TypeDialog($"{playerUnit.BattlePokemon.P_Base.PokemonName}{GetCorrectParticle(playerUnit.BattlePokemon.P_Base.PokemonName, "topic")}\n{expGain}경험치를 얻었다!");
            yield return playerUnit.BattleHud.SetExpSmooth();

            while (playerUnit.BattlePokemon.CheckForLevelUp())
            {
                playerUnit.BattleHud.SetLevel();
                yield return dialogBox.TypeDialog($"{playerUnit.BattlePokemon.P_Base.PokemonName}{GetCorrectParticle(playerUnit.BattlePokemon.P_Base.PokemonName, "topic")}\n레벨{playerUnit.BattlePokemon.PokemonLevel}으로 올랐다!");

                var newSkill = playerUnit.BattlePokemon.GetLearnableSkill();
                if (newSkill != null)
                {
                    if (playerUnit.BattlePokemon.Skills.Count < PokemonBase.MaxNumOfSkills)
                    {
                        playerUnit.BattlePokemon.LearnSkill(newSkill);
                        yield return dialogBox.TypeDialog($"{playerUnit.BattlePokemon.P_Base.PokemonName}{GetCorrectParticle(playerUnit.BattlePokemon.P_Base.PokemonName, "topic")}새로\n{newSkill.SkillBase.SkillName}{GetCorrectParticle(newSkill.SkillBase.SkillName, "object")} 배웠다!");
                        dialogBox.SetSkillNames(playerUnit.BattlePokemon.Skills);
                    }
                    else
                    {
                        //스킬 잊기
                        yield return dialogBox.TypeDialog($"{playerUnit.BattlePokemon.P_Base.PokemonName}{GetCorrectParticle(playerUnit.BattlePokemon.P_Base.PokemonName, "topic")}새로 \n{newSkill.SkillBase.SkillName}{GetCorrectParticle(newSkill.SkillBase.SkillName, "object")} 배우고 싶다!...");
                        yield return dialogBox.TypeDialog($"그러나 {playerUnit.BattlePokemon.P_Base.PokemonName}{GetCorrectParticle(playerUnit.BattlePokemon.P_Base.PokemonName, "topic")}기술을 4개\n알고 있으므로 더 이상 배울 수 없다!");
                        yield return ChooseSkillToForget(playerUnit.BattlePokemon, newSkill.SkillBase);
                        yield return new WaitUntil(() => state != BattleState.SkillToForget);
                        yield return new WaitForSeconds(2.0f);
                    }
                }

                yield return playerUnit.BattleHud.SetExpSmooth(true);
            }
            yield return new WaitForSeconds(1.0f);
        }
        CheckForBattleOver(faintedUnit);
        GameManager.Inst.AddGold();
    }
    void CheckForBattleOver(BattleUnit faintedUnit)
    {
        if (faintedUnit.IsPlayerUnit)
        {
            var nextPokemon = playerParty.GetHealthyPokemon();
            if (nextPokemon != null)
            {
                OpenPartyScreen();
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
                BattleOver(true);
            }
            else
            {
                var nextPokemon = trainerParty.GetHealthyPokemon();
                if (nextPokemon != null)
                {
                    //다음포케

                }
                else
                {
                    BattleOver(true);
                }
            }
        }
    }
    IEnumerator ShowDamageDetails(Pokemon.DamageDetails damageDetails)
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
                StartCoroutine(RunTurns(BattleAction.UseItem));
            }
            else if (currentAction == 2)
            {//포켓몬
                preState = state;
                OpenPartyScreen();
                // Debug.Log("포켓몬");
            }
            else if (currentAction == 3)
            {//도망친다
                // Debug.Log("도망");
                StartCoroutine(RunTurns(BattleAction.Run));
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
            if (skill.SkillPP == 0)
            {
                // 스킬 사용 불가 메시지 출력
                dialogBox.EnableSkillSelector(false);
                dialogBox.EnableDialogText(true);
                StartCoroutine(dialogBox.TypeDialog($"기술의 남은 포인트가 없다!"));
                ActionSelection();
                return;
            }

            dialogBox.EnableSkillSelector(false);
            dialogBox.EnableDialogText(true);
            StartCoroutine(RunTurns(BattleAction.Skill));
        }
        else if (Input.GetKeyDown(KeyCode.Backspace))
        {
            dialogBox.EnableSkillSelector(false);
            dialogBox.EnableDialogText(true);
            ActionSelection();
        }
    }
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

            if (preState == BattleState.ActionSelection)
            {
                preState = null;
                StartCoroutine(RunTurns(BattleAction.SwitchPokemon));
                dialogBox.EnableActionSelector(false);
            }
            //포켓몬이 쓰러졌을때
            else
            {
                state = BattleState.Busy;
                StartCoroutine(SwitchPokemon(selectedMember));
            }
        }
        else if (Input.GetKeyDown(KeyCode.Backspace))
        {
            partyScreen.gameObject.SetActive(false);
            ActionSelection();
        }
    }
    IEnumerator SwitchPokemon(Pokemon newPokemon)
    {
        playerUnit.BattlePokemon.CureVolatileStatus();
        playerUnit.BattlePokemon.ResetRankup();

        yield return dialogBox.TypeDialog($"돌아와 {playerUnit.BattlePokemon.P_Base.PokemonName}!");
        //사망애니메이션
        yield return new WaitForSeconds(1.5f);

        // 현재 전투 중인 포켓몬 (인덱스 0에 있는 포켓몬)
        var currentBattlePokemon = playerParty.Party[0];

        // 교체 작업: 교체할 포켓몬을 0번 인덱스로, 나가있는 포켓몬을 교체할 포켓몬의 인덱스로 이동
        playerParty.Party[0] = newPokemon;
        playerParty.Party[currentMember] = currentBattlePokemon;

        playerUnit.SetUp(newPokemon);
        dialogBox.SetSkillNames(newPokemon.Skills);

        skillCount = newPokemon.Skills.Count;

        yield return dialogBox.TypeDialog($"가랏! {newPokemon.P_Base.PokemonName}!");
        state = BattleState.RunningTurn;
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
            default:
                throw new ArgumentException("Invalid particle type");
        }
    }
    IEnumerator ThrowPokeball()
    {
        state = BattleState.Busy;

        if (isTrainerBattle)
        {
            yield return dialogBox.TypeDialog("다른 트레이너의 포켓몬은 잡을 수 없다!");
            state = BattleState.RunningTurn;
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
            Destroy(pokeball);
            yield return dialogBox.TypeDialog($"신난다-!\n야생 {enemyUnit.BattlePokemon.P_Base.PokemonName}을 잡았다!");
            BattleOver(true);
            GameManager.Inst.AddGold();
        }
        else
        {
            yield return dialogBox.TypeDialog($"씨발!");
            Destroy(pokeball);
            state = BattleState.RunningTurn;
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
    IEnumerator TryToRun()
    {
        state = BattleState.Busy;

        if (isTrainerBattle)
        {
            yield return dialogBox.TypeDialog("");
            state = BattleState.RunningTurn;
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
                state = BattleState.RunningTurn;
                // yield break;

            }
        }
    }
}
