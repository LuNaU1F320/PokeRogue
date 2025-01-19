using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class PokemonParty : MonoBehaviour
{
    [SerializeField] List<Pokemon> party;
    // void Awake()
    // {
    //     DontDestroyOnLoad(this.gameObject);
    // }
    public List<Pokemon> Party
    {
        get
        {
            return party;

        }
    }
    private void Awake()
    {
        foreach (var pokemon in party)
        {
            pokemon.Init();
        }
    }
    public Pokemon GetHealthyPokemon()
    {
        return party.Where(x => x.PokemonHp > 0).FirstOrDefault();
    }
    public void AddPokemon(Pokemon newPokemon)
    {
        if (party.Count < 6)
        {
            party.Add(newPokemon);
        }
        else
        {
            //놓아줄 포켓몬 선택
        }
    }
}
