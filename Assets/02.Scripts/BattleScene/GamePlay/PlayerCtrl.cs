using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayerCtrl : MonoBehaviour
{
    [SerializeField] Sprite sprite;
    [SerializeField] string name;
    // List<PokemonSaveData> pokemons;


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
        var saveData = new PlayerSaveData()
        {
            pokemons = GetComponent<PokemonParty>().Party.Select(p => p.GetSaveData()).ToList()
        };
        return saveData;
    }
    public void RestoreState(object state)
    {
        var saveData = (PlayerSaveData)state;

        GetComponent<PokemonParty>().Party = saveData.pokemons.Select(s => new Pokemon(s)).ToList();
    }
}

[System.Serializable]
public class PlayerSaveData
{
    public List<PokemonSaveData> pokemons;
}
