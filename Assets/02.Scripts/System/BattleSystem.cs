using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;

public enum BattleState
{
    Start,
    ActionSelection,
    MoveSelection,
    PerformMove,
    Busy,
    PartyScreen,
    BattleOver
}
public class BattleSystem : MonoBehaviour
{
    [SerializeField] BattleUnit playerUnit;
    [SerializeField] BattleUnit enemyUnit;
    [SerializeField] BattleDialogBox dialogBox;

    //Party
    [SerializeField] PartyScreen partyScreen;

    public event Action<bool> OnBattleOver;

    BattleState state;
    int currentAction = 0;
    int currentSkill = 0;
    int currentMember = 0;
    int skillCount = 0;


    PokemonParty playerParty;
    Pokemon wildPokemon;

    private void Awake()
    {
        ConditionsDB.Init();
    }

    private void Start()
    {
        currentAction = 0;
        playerParty = FindObjectOfType<PokemonParty>().GetComponent<PokemonParty>();
        wildPokemon = FindObjectOfType<MapArea>().GetComponent<MapArea>().GetRandomWildPokemon();
        StartCoroutine(SetUpBattle());
    }

    public void Update()
    {
        if (state == BattleState.ActionSelection)
        {
            HandleActionSelection();
        }
        else if (state == BattleState.MoveSelection)
        {
            HandleSkillSelection();
        }
        else if (state == BattleState.PartyScreen)
        {
            HandlePartyScreenSelection();
        }
        if (Input.GetKeyDown(KeyCode.G))
        {
            Debug.Log(currentMember);
        }
        if (Input.GetKeyDown(KeyCode.F))
        {
            Debug.Log(playerUnit.BattlePokemon.PokemonBase.PokemonName);
            Debug.Log(playerUnit.BattlePokemon.Attack);
            Debug.Log(playerUnit.BattlePokemon.Rankup[0]);
        }
    }
    public IEnumerator SetUpBattle()
    {
        playerUnit.SetUp(playerParty.GetHealthyPokemon());
        enemyUnit.SetUp(wildPokemon);

        partyScreen.Init();

        dialogBox.SetSkillNames(playerUnit.BattlePokemon.Skills);

        skillCount = playerUnit.BattlePokemon.Skills.Count;

        yield return dialogBox.TypeDialog($"앗! 야생 {enemyUnit.BattlePokemon.PokemonBase.PokemonName}{GetCorrectParticle(enemyUnit.BattlePokemon.PokemonBase.PokemonName, true)} \n튀어나왔다!");

        // yield return dialogBox.TypeDialog($"앗! 야생 {enemyUnit.BattlePokemon.PokemonBase.PokemonName}가 \n튀어나왔다!");
        // yield return new WaitForSeconds(1f);

        ChooseFirstTurn();
    }
    void ChooseFirstTurn()
    {
        if (playerUnit.BattlePokemon.Speed >= enemyUnit.BattlePokemon.Speed)
        {
            ActionSelection();
        }
        else
        {
            StartCoroutine(EnemyMove());
        }
    }
    void BattleOver(bool won)
    {
        state = BattleState.BattleOver;
        playerParty.Pokemons.ForEach(pok => pok.OnBattleOver());
        OnBattleOver(won);
    }
    void ActionSelection()
    {
        state = BattleState.ActionSelection;
        dialogBox.SetDialog($"{playerUnit.BattlePokemon.PokemonBase.PokemonName}{GetCorrectParticle(playerUnit.BattlePokemon.PokemonBase.PokemonName, false)} 무엇을 할까?");
        dialogBox.EnableActionSelector(true);
    }
    void OpenPartyScreen()
    {
        state = BattleState.PartyScreen;
        partyScreen.SetPartyData(playerParty.Pokemons);
        partyScreen.gameObject.SetActive(true);
    }
    void SkillSelection()
    {
        state = BattleState.MoveSelection;
        dialogBox.EnableActionSelector(false);
        dialogBox.EnableDialogText(false);
        dialogBox.EnableSkillSelector(true);
    }
    IEnumerator PlayerMove()
    {
        state = BattleState.PerformMove;

        var skill = playerUnit.BattlePokemon.Skills[currentSkill];

        yield return RunSkill(playerUnit, enemyUnit, skill);
        if (state == BattleState.PerformMove)
        {
            StartCoroutine(EnemyMove());
        }
    }
    IEnumerator EnemyMove()
    {
        state = BattleState.PerformMove;
        var skill = enemyUnit.BattlePokemon.GetRandomSkill();

        yield return RunSkill(enemyUnit, playerUnit, skill);
        if (state == BattleState.PerformMove)
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
        if (skill.SkillPP <= 0)
        {
            // 스킬 사용 불가 메시지 출력
            yield return dialogBox.TypeDialog($"기술의 남은 포인트가 없다!");

            // 행동 선택 상태로 복귀
            if (sourceUnit.IsPlayerUnit)
            {
                ActionSelection();
            }
            else
            {
                StartCoroutine(EnemyMove());
            }
            yield break; // 현재 실행 종료
        }
        // while (dialogBox.IsTyping())
        // {
        //     yield return null; // 이전 텍스트 출력이 끝날 때까지 대기
        // }

        skill.SkillPP--;

        yield return dialogBox.TypeDialog($"{sourceUnit.BattlePokemon.PokemonBase.PokemonName}의 {skill.SkillBase.SkillName}!");
        //공격 애니메이션

        //피격 애니메이션

        if (skill.SkillBase.CategoryKey == CategoryKey.Status)
        {
            yield return RunSkillEffects(skill, sourceUnit.BattlePokemon, targetUnit.BattlePokemon);
        }
        else
        {
            var (startHp, endHp, damageDetails) = targetUnit.BattlePokemon.TakeDamage(skill, sourceUnit.BattlePokemon);

            StartCoroutine(targetUnit.BattleHud.UpdateHp());
            // yield return targetUnit.BattleHud.UpdateHp();
            if (targetUnit.BattleHud.hpbar_Text != null)
            {
                StartCoroutine(targetUnit.BattleHud.AnimateTextHp(startHp, endHp));
            }
            StartCoroutine(ShowDamageDetails(damageDetails));

        }

        if (targetUnit.BattlePokemon.PokemonHp <= 0)
        {
            yield return dialogBox.TypeDialog($"{targetUnit.BattlePokemon.PokemonBase.PokemonName}{GetCorrectParticle(targetUnit.BattlePokemon.PokemonBase.PokemonName, false)} 쓰러졌다!");
            //애니메이션 재생

            //플레이어 승리 (다음스테이지로)
            yield return new WaitForSeconds(2.0f);
            CheckForBattleOver(targetUnit);
        }

        //상태이상 처리
        sourceUnit.BattlePokemon.OnAfterTurn();
        yield return ShowStatusChanges(sourceUnit.BattlePokemon);
        yield return StartCoroutine(sourceUnit.BattleHud.UpdateHp());
        // yield return targetUnit.BattleHud.UpdateHp();
        // if (sourceUnit.BattleHud.hpbar_Text != null)
        // {
        //     StartCoroutine(sourceUnit.BattleHud.AnimateTextHp(startHp, endHp));
        // }
        if (sourceUnit.BattlePokemon.PokemonHp <= 0)
        {
            yield return dialogBox.TypeDialog($"{sourceUnit.BattlePokemon.PokemonBase.PokemonName}{GetCorrectParticle(sourceUnit.BattlePokemon.PokemonBase.PokemonName, false)} 쓰러졌다!");
            //애니메이션 재생

            //플레이어 승리 (다음스테이지로)
            yield return new WaitForSeconds(2.0f);
            CheckForBattleOver(sourceUnit);
        }
    }

    IEnumerator RunSkillEffects(Skill skill, Pokemon sourceUnit, Pokemon targetUnit)
    {
        var effects = skill.SkillBase.Effects;
        //RankUp
        if (effects.Rankup != null)
        {
            if (skill.SkillBase.Target == SkillTarget.Self)
            {
                sourceUnit.ApplyRankups(skill.SkillBase.Effects.Rankup);
            }
            else
            {
                targetUnit.ApplyRankups(skill.SkillBase.Effects.Rankup);
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

    IEnumerator ShowStatusChanges(Pokemon pokemon)
    {
        while (pokemon.StatusCngMsg.Count > 0)
        {
            string message = pokemon.StatusCngMsg.Dequeue();
            yield return dialogBox.TypeDialog(message);
        }
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
            BattleOver(true);
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
            if (currentAction == 1)
            {//볼
                Debug.Log("볼");
            }
            if (currentAction == 2)
            {//포켓몬
                OpenPartyScreen();
                // Debug.Log("포켓몬");
            }
            if (currentAction == 3)
            {//도망친다
                Debug.Log("도망");
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
                // return;
            }

            dialogBox.EnableSkillSelector(false);
            dialogBox.EnableDialogText(true);
            StartCoroutine(PlayerMove());
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
        currentMember = Mathf.Clamp(currentMember, 0, playerParty.Pokemons.Count - 1);
        partyScreen.UpdateMemberSelection(currentMember);
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space))
        {
            //포켓몬 교체
            var selectedMember = playerParty.Pokemons[currentMember];
            if (selectedMember.PokemonHp <= 0)
            {
                partyScreen.SetMessageText($"{playerParty.Pokemons[currentMember].PokemonBase.PokemonName}{GetCorrectParticle(playerParty.Pokemons[currentMember].PokemonBase.PokemonName, false)} 싸울 수 있는 \n기력이 남아 있지 않습니다!");
                return;
            }
            if (selectedMember == playerUnit.BattlePokemon)
            {
                //능력치보기, 놓아주기, 그만두기 구현
                partyScreen.SetMessageText($"이미 전투 중인 포켓몬으로 교체 할 수 없습니다!");
                return;
            }
            partyScreen.gameObject.SetActive(false);
            state = BattleState.Busy;

            // 현재 전투 중인 포켓몬 (인덱스 0에 있는 포켓몬)
            var currentBattlePokemon = playerParty.Pokemons[0];

            // 교체 작업: 교체할 포켓몬을 0번 인덱스로, 나가있는 포켓몬을 교체할 포켓몬의 인덱스로 이동
            playerParty.Pokemons[0] = selectedMember;
            playerParty.Pokemons[currentMember] = currentBattlePokemon;

            StartCoroutine(SwitchPokemon(selectedMember));
            selectedMember.ResetRankup();
        }
        else if (Input.GetKeyDown(KeyCode.Backspace))
        {
            partyScreen.gameObject.SetActive(false);
            ActionSelection();
        }
    }
    IEnumerator SwitchPokemon(Pokemon newPokemon)
    {
        // bool isFainted = true;
        if (playerUnit.BattlePokemon.PokemonHp > 0)
        {
            // isFainted = false;
            yield return dialogBox.TypeDialog($"돌아와 {playerUnit.BattlePokemon.PokemonBase.PokemonName}!");
            //사망애니메이션
            yield return new WaitForSeconds(1.5f);

            playerUnit.SetUp(newPokemon);
            dialogBox.SetSkillNames(newPokemon.Skills);

            skillCount = newPokemon.Skills.Count;

            yield return dialogBox.TypeDialog($"가랏! {newPokemon.PokemonBase.PokemonName}!");
            StartCoroutine(EnemyMove());
        }
        else        //포켓몬이 쓰러졌을때
        {
            playerUnit.SetUp(newPokemon);
            dialogBox.SetSkillNames(newPokemon.Skills);

            skillCount = newPokemon.Skills.Count;

            yield return dialogBox.TypeDialog($"가랏! {newPokemon.PokemonBase.PokemonName}!");
            ActionSelection();
        }
    }
    string GetCorrectParticle(string name, bool subject)    //은는이가
    {
        char lastChar = name[name.Length - 1];
        int unicode = (int)lastChar;
        bool endsWithConsonant = (unicode - 44032) % 28 != 0; // 44032는 '가'의 유니코드, 28는 받침의 수


        if (subject)
        {
            return endsWithConsonant ? "이" : "가";
        }
        else
        {
            return endsWithConsonant ? "은" : "는";
        }
    }
}
