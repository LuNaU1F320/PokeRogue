using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;

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
    [SerializeField] Text Gold_Text;
    [HideInInspector] public bool isRun = true;
    [SerializeField] float GameSpeed = 1.0f;

    GameState state;

    public static GameManager Inst { get; private set; }


    private void Awake()
    {
        Inst = this;
        Time.timeScale = GameSpeed;
        playerCtrl = FindObjectOfType<PlayerCtrl>();
    }

    void Start()
    {
        PlayerParty = FindObjectOfType<PokemonParty>().GetComponent<PokemonParty>();
        if (battleSystem == null || PlayerParty == null || mapArea == null)
        {
            return;
        }
        if (GlobalValue.CurStage == 0)
        {
            GlobalValue.CurStage = 1;
        }
        GlobalValue.UserGold = 1000;
        Stage_Text.text = $"마을 - {GlobalValue.CurStage}";
        Gold_Text.text = $"￡{GlobalValue.UserGold}";

        StartBattle().Forget();
    }

    // Update is called once per frame
    void Update()
    {

    }
    public async UniTask StartBattle()
    {
        var wildPokemon = mapArea.GetRandomWildPokemon();

        var refWildPokemon = new Pokemon(wildPokemon.P_Base, wildPokemon.PokemonLevel);

        // battleSystem.StartBattle(PlayerParty, refWildPokemon);
        await battleSystem.StartBattle(PlayerParty, refWildPokemon);
    }
    // public void StartTrainerBattle(TrainerCtrl trainer)
    // {
    //     battleSystem.gameObject.SetActive(true);

    //     var playerParty = GetComponent<PokemonParty>();
    //     var trainerParty = trainer.GetComponent<PokemonParty>();

    //     battleSystem.StartTrainerBattle(playerParty, trainerParty);
    // }
    public void EndBattle(bool won)
    {
        if (won)
        {
            // StopAllCoroutines();
            GlobalValue.CurStage++;
            Stage_Text.text = $"마을 - {GlobalValue.CurStage}";
            // StartCoroutine(PlayerParty.CheckForEvolutions());
            StartBattle().Forget();
        }
        else
        {
            Debug.Log("전투에서 패배했습니다...");
            playerCtrl.party.Party.Clear();
            GlobalValue.CurStage = 1;
            GlobalValue.UserGold = 1000;
            playerCtrl.CaptureState();
            var savingSystem = FindObjectOfType<SavingSystem>();
            if (savingSystem != null)
            {
                savingSystem.SaveGame();
            }
            else
            {
                Debug.Log("저장 실패...");
            }
            SceneManager.LoadScene("LobbyScene");
        }
    }
    public void AddGold()
    {
        GlobalValue.UserGold += ((GlobalValue.CurStage + 10) / 10) * 1000;

        Gold_Text.text = $"￡{GlobalValue.UserGold}";
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
