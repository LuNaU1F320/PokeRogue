using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public enum GameState { FreeRoam, Battle }

public class GameManager : MonoBehaviour
{
    [SerializeField] PlayerController playerController;
    [SerializeField] BattleSystem battleSystem;
    GameState state;

    // Start is called before the first frame update
    void Start()
    {
        playerController.OnEncountered += StartBattle;
        // battleSystem.OnBattleOver += EndBattle;
    }
    private void Update()
    {
        if (state == GameState.Battle)
        {
            battleSystem.HandleUpdate();
        }
    }
    void EndBattle(bool won)
    {
        // state = GameState.FreeRoam;
        // battleSystem.gameObject.SetActive(false);
    }
    void StartBattle()
    {
        state = GameState.Battle;
        battleSystem.gameObject.SetActive(true);

        var playerParty = playerController.GetComponent<PokemonParty>();
        var wildPokemon = FindObjectOfType<MapArea>().GetComponent<MapArea>().GetRandomWildPokemon();
        battleSystem.StartBattle(playerParty, wildPokemon);
    }
}
