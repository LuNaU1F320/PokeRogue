using System;
using UnityEngine;
using UnityEngine.UI;

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
    public PlayerCtrl playerCtrl;
    [SerializeField] BattleSystem battleSystem;
    PokemonParty PlayerParty;
    [SerializeField] MapArea mapArea;
    [SerializeField] Text Stage_Text;
    private int StageCount;
    [SerializeField] Text Gold_Text;
    private int UserGold;
    [HideInInspector] public bool isRun = true;
    [SerializeField] float GameSpeed = 1.0f;

    GameState state;

    public static GameManager Inst { get; private set; }


    private void Awake()
    {
        Inst = this;
        // SkillDB.Init();
        // PokemonDB.Init();
        // ConditionsDB.Init();
        Time.timeScale = GameSpeed;
        playerCtrl = FindObjectOfType<PlayerCtrl>();
    }

    void Start()
    {
        PlayerParty = FindObjectOfType<PokemonParty>().GetComponent<PokemonParty>();
        StartBattle();
        if (battleSystem == null || PlayerParty == null || mapArea == null)
        {
            return;
        }
        StageCount = 1;
        UserGold = 1000;
        Stage_Text.text = $"마을 - {StageCount}";
        Gold_Text.text = $"￡{UserGold}";
        // OnEncountered += StartBattle;
        // battleSystem.OnBattleOver += EndBattle;
        // EvolutionManager.Inst.OnStartEvolution += () => state = GameState.Evolution;
        // EvolutionManager.Inst.OnCompleteEvolution += () => state = GameState.None;
    }

    // Update is called once per frame
    void Update()
    {

    }
    void StartBattle()
    {
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
        if (won)
        {
            StopAllCoroutines();
            StageCount++;
            Stage_Text.text = $"마을 - {StageCount}";
            // StartCoroutine(PlayerParty.CheckForEvolutions());
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
