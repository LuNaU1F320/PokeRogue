using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BattleState
{
    Start,
    PlayerAction,
    PlayerMove,
    EnemyMove,
    Busy
}
public class BattleSystem : MonoBehaviour
{
    [SerializeField] BattleUnit playerUnit;
    [SerializeField] BattleHud playerHud;
    [SerializeField] BattleUnit enemyUnit;
    [SerializeField] BattleHud enemyHud;
    [SerializeField] BattleDialogBox dialogBox;

    BattleState state;
    int currentAction = 0;
    int currentSkill = 0;
    int skillCount = 0;

    // Start is called before the first frame update
    private void Start()
    {
        StartCoroutine(SetUpBattle());
    }
    private void Update()
    {
        if (state == BattleState.PlayerAction)
        {
            HandleActionSelection();
        }
        else if (state == BattleState.PlayerMove)
        {
            HandleSkillSelection();
        }
        if (Input.GetKeyDown(KeyCode.G))
        {
            Debug.Log(state);
            Debug.Log(currentAction);
        }
    }

    public IEnumerator SetUpBattle()
    {
        playerUnit.SetUp();
        playerHud.SetHud(playerUnit.BattlePokemon);
        enemyUnit.SetUp();
        enemyHud.SetHud(enemyUnit.BattlePokemon);

        dialogBox.SetSkillNames(playerUnit.BattlePokemon.Skills);

        skillCount = playerUnit.BattlePokemon.Skills.Count;

        yield return dialogBox.TypeDialog($"앗! 야생 {enemyUnit.BattlePokemon.PokemonBase.PokemonName}가 \n튀어나왔다!");
        yield return new WaitForSeconds(1f);

        PlayerAction();

    }
    void PlayerAction()
    {
        state = BattleState.PlayerAction;
        StartCoroutine(dialogBox.TypeDialog($"{playerUnit.BattlePokemon.PokemonBase.PokemonName}은(는) 무엇을 할까?"));
        dialogBox.EnableActionSelector(true);
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
        yield return dialogBox.TypeDialog($"{playerUnit.BattlePokemon.PokemonBase.PokemonName}의 {skill.SkillBase.SkillName}!");
        yield return new WaitForSeconds(1.0f);

        bool isFainted = enemyUnit.BattlePokemon.TakeDamage(skill, playerUnit.BattlePokemon);
        yield return enemyHud.UpdateHp();
        if (isFainted == true)
        {
            yield return dialogBox.TypeDialog($"{enemyUnit.BattlePokemon.PokemonBase.PokemonName}은(는) 쓰려졌다!");
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

        yield return dialogBox.TypeDialog($"{enemyUnit.BattlePokemon.PokemonBase.PokemonName}의 {skill.SkillBase.SkillName}!");
        yield return new WaitForSeconds(1.0f);

        bool isFainted = playerUnit.BattlePokemon.TakeDamage(skill, playerUnit.BattlePokemon);
        yield return playerHud.UpdateHp();
        if (isFainted == true)
        {
            yield return dialogBox.TypeDialog($"{playerUnit.BattlePokemon.PokemonBase.PokemonName}은(는) 쓰려졌다!");
        }
        else
        {
            PlayerAction();
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
        dialogBox.UpdateActionSelection(currentAction);
        if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return))
        {
            if (currentAction == 0)
            {//fight
                PlayerMove();
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
        /*
        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
           if (currentSkill < 2)
           {
               currentSkill = currentSkill + 2;
           }
        }
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
           if (currentSkill < 3)
           {
               ++currentSkill;
           }
        }
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
           if (1 < currentSkill)
           {
               currentSkill = currentSkill - 2;
           }
        }
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
           if (0 < currentSkill)
           {
               --currentSkill;
           }
        }

        dialogBox.UpdateSkillSelection(currentSkill, playerUnit.BattlePokemon.Skills[currentSkill]);
        */
    }
}
