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
    PlayerCtrl playerCtrl;
    [SerializeField] BattleSystem battleSystem;
    PokemonParty PlayerParty;
    [SerializeField] MapArea mapArea;
    [SerializeField] Text Stage_Text;
    private int StageCount;
    [SerializeField] Text Gold_Text;
    private int UserGold;
    [HideInInspector] public bool isRun = true;

    GameState state;

    public static GameManager Inst { get; private set; }


    private void Awake()
    {
        Inst = this;
        ConditionsDB.Init();
        PokemonDB.Init();
        SkillDB.Init();
        Time.timeScale = 5.0f;
    }

    void Start()
    {
        PlayerParty = FindObjectOfType<PokemonParty>().GetComponent<PokemonParty>();
        if (battleSystem == null || PlayerParty == null || mapArea == null)
        {
            return;
        }
        StartBattle();
        StageCount = 1;
        UserGold = 1000;
        Stage_Text.text = $"마을 - {StageCount}";
        Gold_Text.text = $"￡{UserGold}";
        // OnEncountered += StartBattle;
        // battleSystem.OnBattleOver += EndBattle;
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

        var refWildPokemon = new Pokemon(wildPokemon.P_Base, wildPokemon.PokemonLevel);

        battleSystem.StartBattle(PlayerParty, refWildPokemon);
    }
    public void StartTrainerBattle(TrainerCtrl trainer)
    {
        battleSystem.gameObject.SetActive(true);

        var playerParty = GetComponent<PokemonParty>();
        var trainerParty = trainer.GetComponent<PokemonParty>();

        battleSystem.StartTrainerBattle(playerParty, trainerParty);
    }
    public void EndBattle(bool won)
    {
        // var playerParty = playerCtrl.GetComponent<PokemonParty>();

        if (won)
        {
            StopAllCoroutines();
            StartCoroutine(PlayerParty.CheckForEvolutions());
            StageCount++;
            Stage_Text.text = $"마을 - {StageCount}";
            StartBattle();
        }
        else
        {
            Debug.Log("전투에서 패배했습니다...");
        }
    }
    public void AddGold()
    {
        UserGold += ((StageCount + 10) / 10) * 1000;

        Gold_Text.text = $"￡{UserGold}";
    }
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
