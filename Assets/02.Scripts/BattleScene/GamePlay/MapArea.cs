using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapArea : MonoBehaviour
{
    [SerializeField] List<Pokemon> wildPokemons;
    public Pokemon GetRandomWildPokemon()
    {
        // var wildPokemon = wildPokemons[Random.Range(0, wildPokemons.Count)];
        // wildPokemon.Init();
        // return wildPokemon;

        var basePokemon = wildPokemons[Random.Range(0, wildPokemons.Count)];
        var wildPokemon = new Pokemon(basePokemon.P_Base, Random.Range(4, 8));
        wildPokemon.Init();

        return wildPokemon;
    }
}
