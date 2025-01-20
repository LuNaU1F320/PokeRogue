using System;
using UnityEngine;
using UnityEngine.UI;

public enum GameState
{
    None,
    Battle,
    Dialog,
    Busy
}

public class GameManager : MonoBehaviour
{
    // public event Action OnEncountered;
    [SerializeField] BattleSystem battleSystem;
    [SerializeField] PokemonParty PlayerParty;
    [SerializeField] MapArea mapArea;
    [SerializeField] Text Stage_Text;
    private int StageCount;
    [SerializeField] Text Gold_Text;

    GameState state;

    public static GameManager Inst { get; private set; }


    private void Awake()
    {
        Inst = this;
        ConditionsDB.Init();
    }

    void Start()
    {
        if (battleSystem == null || PlayerParty == null || mapArea == null)
        {
            return;
        }
        StartBattle();
        StageCount = 1;
        Stage_Text.text = "마을 - 1";
        // OnEncountered += StartBattle;
        battleSystem.OnBattleOver += EndBattle;
    }

    // Update is called once per frame
    void Update()
    {

    }
    void StartBattle()
    {
        // battleSystem.gameObject.SetActive(true);

        // var playerParty = GetComponent<PokemonParty>();
        // var wildPokemon = FindObjectOfType<MapArea>().GetComponent<MapArea>().GetRandomWildPokemon();

        var wildPokemon = mapArea.GetRandomWildPokemon();

        var refWildPokemon = new Pokemon(wildPokemon.PokemonBase, wildPokemon.PokemonLevel);

        battleSystem.StartBattle(PlayerParty, refWildPokemon);
    }
    public void StartTrainerBattle(TrainerCtrl trainer)
    {
        battleSystem.gameObject.SetActive(true);

        var playerParty = GetComponent<PokemonParty>();
        var trainerParty = trainer.GetComponent<PokemonParty>();

        battleSystem.StartTrainerBattle(playerParty, trainerParty);
    }
    void EndBattle(bool won)
    {
        if (won)
        {
            StageCount++;
            Stage_Text.text = $"마을 - {StageCount}";
            StartBattle();
        }
        else
        {
            Debug.Log("전투에서 패배했습니다...");
        }
    }
}
