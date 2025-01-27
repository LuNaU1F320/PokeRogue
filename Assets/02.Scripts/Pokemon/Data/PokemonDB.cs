using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PokemonDB
{
    static Dictionary<int, PokemonBase> pokemons;

    public static void Init()
    {
        pokemons = new Dictionary<int, PokemonBase>();
        var pokemonArray = Resources.LoadAll<PokemonBase>("");
        foreach (var pokemon in pokemonArray)
        {
            // Debug.Log("불러온 포켓몬: " + pokemon.name + " (Index: " + pokemon.PokemonIndex + ")");

            if (pokemons.ContainsKey(pokemon.PokemonIndex))
            {
                Debug.Log("겹치는 포켓몬 존재");
                continue;
            }
            pokemons[pokemon.PokemonIndex] = pokemon;
        }
    }
    public static PokemonBase GetPokemonByIndex(int index)
    {
        if (pokemons.ContainsKey(index) == false)
        {
            Debug.Log("포켓몬 찾을 수 없음");
            return null;
        }
        return pokemons[index];
    }
}
