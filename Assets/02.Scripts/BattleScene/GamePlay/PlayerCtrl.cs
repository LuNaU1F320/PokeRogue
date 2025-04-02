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
        SkillDB.Init();
        PokemonDB.Init();
        ConditionsDB.Init();
        GlobalValue.LoadGameInfo();
        if (Instance == null)
        {
            Instance = this;
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
        return new PlayerSaveData
        {
            pokemons = GetComponent<PokemonParty>().Party.Select(p => p.GetSaveData()).ToList(),
            // gold = GameManager.Inst.gold,
            // stageIndex = GameManager.Inst.CurrentStageIndex
        };
    }

    public void RestoreState(object state)
    {
        var data = (PlayerSaveData)state;
        // data.pokemons.Select(s => new Pokemon(s)).ToList();
        GetComponent<PokemonParty>().Party = data.pokemons.Select(p => new Pokemon(p)).ToList();

        // GameManager.Inst.gold = data.gold;
        // GameManager.Inst.CurrentStageIndex = data.stageIndex;
    }
}

[System.Serializable]
public class PlayerSaveData
{
    public List<PokemonSaveData> pokemons;
    public int gold;
    public int stageIndex;
}
