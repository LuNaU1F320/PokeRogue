using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class PokemonParty : MonoBehaviour
{
    [SerializeField] List<Pokemon> pokemons;
    // void Awake()
    // {
    //     DontDestroyOnLoad(this.gameObject);
    // }
    public List<Pokemon> Pokemons
    {
        get
        {
            return pokemons;

        }
    }
    private void Start()
    {
        foreach (var pokemon in pokemons)
        {
            pokemon.Init();
        }
    }
    public Pokemon GetHealthyPokemon()
    {
        return pokemons.Where(x => x.PokemonHp > 0).FirstOrDefault();
    }
}
