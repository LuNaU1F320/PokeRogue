using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;

public enum BattleState
{
    Start,
    PlayerAction,
    PlayerMove,
    EnemyMove,
    Busy,
    PartyScreen
}
public class BattleSystem : MonoBehaviour
{
    [SerializeField] BattleUnit playerUnit;
    [SerializeField] BattleHud playerHud;
    [SerializeField] BattleUnit enemyUnit;
    [SerializeField] BattleHud enemyHud;
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
    private void Start()
    {
        currentAction = 0;
        playerParty = FindObjectOfType<PokemonParty>().GetComponent<PokemonParty>();
        wildPokemon = FindObjectOfType<MapArea>().GetComponent<MapArea>().GetRandomWildPokemon();
        StartCoroutine(SetUpBattle());
    }

    public void Update()
    {
        if (state == BattleState.PlayerAction)
        {
            HandleActionSelection();
        }
        else if (state == BattleState.PlayerMove)
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
    }

    public IEnumerator SetUpBattle()
    {
        playerUnit.SetUp(playerParty.GetHealthyPokemon());
        enemyUnit.SetUp(wildPokemon);
        playerHud.SetHud(playerUnit.BattlePokemon);
        enemyHud.SetHud(enemyUnit.BattlePokemon);

        partyScreen.Init();

        dialogBox.SetSkillNames(playerUnit.BattlePokemon.Skills);

        skillCount = playerUnit.BattlePokemon.Skills.Count;

        yield return dialogBox.TypeDialog($"앗! 야생 {enemyUnit.BattlePokemon.PokemonBase.PokemonName}가 \n튀어나왔다!");
        // yield return new WaitForSeconds(1f);

        PlayerAction();

    }
    void PlayerAction()
    {
        state = BattleState.PlayerAction;
        dialogBox.SetDialog($"{playerUnit.BattlePokemon.PokemonBase.PokemonName}은(는) 무엇을 할까?");
        dialogBox.EnableActionSelector(true);
    }
    void OpenPartyScreen()
    {
        state = BattleState.PartyScreen;
        partyScreen.SetPartyData(playerParty.Pokemons);
        partyScreen.gameObject.SetActive(true);
    }
    void PlayerMove()
    {
        state = BattleState.PlayerMove;
        dialogBox.EnableActionSelector(false);
        dialogBox.EnableDialogText(false);
        dialogBox.EnableSkillSelector(true);
    }
    IEnumerator PerformPlayerMove()
    {
        state = BattleState.Busy;

        var skill = playerUnit.BattlePokemon.Skills[currentSkill];
        if (skill.SkillPP <= 0)
        {
            skill.SkillPP--;
        }
        while (dialogBox.IsTyping())
        {
            yield return null; // 이전 텍스트 출력이 끝날 때까지 대기
        }
        yield return dialogBox.TypeDialog($"{playerUnit.BattlePokemon.PokemonBase.PokemonName}의 {skill.SkillBase.SkillName}!");
        // yield return new WaitForSeconds(1.0f);

        var (startHp, endHp, damageDetails) = enemyUnit.BattlePokemon.TakeDamage(skill, playerUnit.BattlePokemon);
        yield return enemyHud.UpdateHp();
        yield return StartCoroutine(ShowDamageDetails(damageDetails));
        // yield return enemyHud.AnimateTextHp();
        if (damageDetails.Fainted == true)
        {
            yield return dialogBox.TypeDialog($"{enemyUnit.BattlePokemon.PokemonBase.PokemonName}은(는) 쓰려졌다!");
            //애니메이션 재생

            //플레이어 승리 (다음스테이지로)
            yield return new WaitForSeconds(2.0f);
            OnBattleOver(true);
        }
        else
        {
            StartCoroutine(EnemyMove());
        }
    }
    IEnumerator EnemyMove()
    {
        state = BattleState.EnemyMove;
        var skill = enemyUnit.BattlePokemon.GetRandomSkill();
        skill.SkillPP--;
        yield return dialogBox.TypeDialog($"{enemyUnit.BattlePokemon.PokemonBase.PokemonName}의 {skill.SkillBase.SkillName}!");
        // yield return new WaitForSeconds(1.0f);

        var (startHp, endHp, damageDetails) = playerUnit.BattlePokemon.TakeDamage(skill, playerUnit.BattlePokemon);
        // yield return playerHud.UpdateHp();
        StartCoroutine(playerHud.UpdateHp());
        StartCoroutine(playerHud.AnimateTextHp(startHp, endHp));
        yield return StartCoroutine(ShowDamageDetails(damageDetails));
        if (damageDetails.Fainted == true)
        {
            yield return dialogBox.TypeDialog($"{playerUnit.BattlePokemon.PokemonBase.PokemonName}은(는) 쓰려졌다!");
            //애니메이션 재생

            yield return new WaitForSeconds(2.0f);

            var nextPokemon = playerParty.GetHealthyPokemon();
            if (nextPokemon != null)
            {
                OpenPartyScreen();
                // playerUnit.SetUp(nextPokemon);
                // playerHud.SetHud(nextPokemon);
                // dialogBox.SetSkillNames(nextPokemon.Skills);

                // skillCount = nextPokemon.Skills.Count;

                // yield return dialogBox.TypeDialog($"가랏! {nextPokemon.PokemonBase.PokemonName}!");
                // PlayerAction();
            }
            else
            {
                //뒤짐
                OnBattleOver(false);
            }
        }
        else
        {
            PlayerAction();
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
                PlayerMove();
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
            dialogBox.EnableSkillSelector(false);
            dialogBox.EnableDialogText(true);
            StartCoroutine(PerformPlayerMove());
        }
        else if (Input.GetKeyDown(KeyCode.Backspace))
        {
            dialogBox.EnableSkillSelector(false);
            dialogBox.EnableDialogText(true);
            PlayerAction();
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
                partyScreen.SetMessageText($"{playerParty.Pokemons[currentMember].PokemonBase.PokemonName}은/는 싸울 수 있는 \n기력이 남아 있지 않습니다!");
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
            StartCoroutine(SwitchPokemon(selectedMember));
        }
        else if (Input.GetKeyDown(KeyCode.Backspace))
        {
            partyScreen.gameObject.SetActive(false);
            PlayerAction();
        }
    }
    IEnumerator SwitchPokemon(Pokemon newPokemon)
    {
        if (playerUnit.BattlePokemon.PokemonHp > 0)
        {
            yield return dialogBox.TypeDialog($"돌아와 {playerUnit.BattlePokemon.PokemonBase.PokemonName}!");
            //사망애니메이션
            yield return new WaitForSeconds(1.5f);

            playerUnit.SetUp(newPokemon);
            playerHud.SetHud(newPokemon);
            dialogBox.SetSkillNames(newPokemon.Skills);

            skillCount = newPokemon.Skills.Count;

            yield return dialogBox.TypeDialog($"가랏! {newPokemon.PokemonBase.PokemonName}!");
            StartCoroutine(EnemyMove());
        }
        else
        {
            playerUnit.SetUp(newPokemon);
            playerHud.SetHud(newPokemon);
            dialogBox.SetSkillNames(newPokemon.Skills);

            skillCount = newPokemon.Skills.Count;

            yield return dialogBox.TypeDialog($"가랏! {newPokemon.PokemonBase.PokemonName}!");
            PlayerMove();
        }
    }
}
