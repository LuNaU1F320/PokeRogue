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
        GetComponent<PokemonParty>().Party = data.pokemons.Select(p => new Pokemon(p)).ToList();

        foreach (var s in data.pokemons.SelectMany(p => p.skills))
        {
            Debug.Log($"[검사] 저장된 skillIndex: {s.skillIdx}, pp: {s.pp}");
            var sb = SkillDB.GetSkillByIdx(s.skillIdx);
            if (sb == null)
                Debug.LogError($"[RestoreState] SkillBase 찾을 수 없음! index: {s.skillIdx}");
            else
                Debug.Log($"[RestoreState] SkillBase 로딩 성공: {sb.SkillName}");
        }
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
