using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayerCtrl : MonoBehaviour, ISaveable
{
    public static PlayerCtrl Instance { get; private set; }
    [SerializeField] Sprite sprite;
    [SerializeField] string name;
    // List<PokemonSaveData> pokemons;
    private PokemonParty party;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            PokemonDB.Init();
            SkillDB.Init();
            ConditionsDB.Init();
            // GlobalValue.LoadGameInfo();
            DontDestroyOnLoad(this.gameObject);

            party = GetComponent<PokemonParty>();
        }
        else
        {
            Destroy(gameObject);
        }
    }
    public PokemonParty Party => party;
    public string TrainerName
    {
        get => name;
    }
    public Sprite TrainerSprite
    {
        get => sprite;
    }
    public object CaptureState()
    {
        // return new PlayerSaveData
        // {
        //     saveParty = GetComponent<PokemonParty>().Party.Select(p => p.GetSaveData()).ToList(),
        //     // gold = GameManager.Inst.gold,
        //     // stageIndex = GameManager.Inst.CurrentStageIndex
        // };
        var saveData = new PlayerSaveData()
        {
            saveParty = GetComponent<PokemonParty>().Party.Select(p => p.GetSaveData()).ToList(),

            userGold = GlobalValue.UserGold,
            curStage = GlobalValue.CurStage,
            myPokemonKeys = GlobalValue.MyPokemon.Keys.ToList(),
            myPokemonValues = GlobalValue.MyPokemon.Values.ToList()
        };
        return saveData;
    }

    public void RestoreState(object state)
    {
        var data = (PlayerSaveData)state;
        GetComponent<PokemonParty>().Party = data.saveParty.Select(s => new Pokemon(s)).ToList();

        // 글로벌값 복원
        GlobalValue.UserGold = data.userGold;
        GlobalValue.CurStage = data.curStage;
        GlobalValue.MyPokemon = new Dictionary<int, GlobalValue.MyPokemonData>();
        for (int i = 0; i < data.myPokemonKeys.Count; i++)
        {
            GlobalValue.MyPokemon[data.myPokemonKeys[i]] = data.myPokemonValues[i];
        }
        Debug.LogWarning(GlobalValue.CurStage);
    }
}

[System.Serializable]
public class PlayerSaveData
{
    public List<PokemonSaveData> saveParty;
    public int userGold;
    public int curStage;
    public List<int> myPokemonKeys;
    public List<GlobalValue.MyPokemonData> myPokemonValues;
}
