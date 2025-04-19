using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SimpleJSON;

public class GlobalValue
{
    public static int UserGold;
    public static int CurStage = 1;

    [System.Serializable]
    public class MyPokemonData
    {
        public int PokemonIdx;
        public bool IsShiny;
    }
    public static Dictionary<int, MyPokemonData> MyPokemon = new Dictionary<int, MyPokemonData>();

    public static float GameSpeed = 1f;
    public static float HpBarSpeed = 1.0f;
    public static float ExpBarSpeed = 1.0f;

    [Header("Sound")]
    public static float MasterVolume = 0.5f;
    public static float BGMVolume = 0.5f;
    public static float UIVolume = 0.5f;


    public static void CatchPokemon(PokemonBase pokemon, bool isShiny)
    {
        int id = pokemon.EvolutionBase.PokemonIndex;
        if (!MyPokemon.ContainsKey(id))
        {
            MyPokemon[id] = new MyPokemonData { PokemonIdx = id, IsShiny = isShiny };
            SavingSystem.Instance?.SaveGame();
        }
    }
    public static void SetBasicStartPokemon()
    {
        int[] defaultPokemons = { 1, 4, 7 };  // 기본 제공 포켓몬 ID 목록

        foreach (int pokemonId in defaultPokemons)
        {
            if (!MyPokemon.ContainsKey(pokemonId))  // 이미 존재하는 경우 덮어씌우지 않음
            {
                MyPokemon[pokemonId] = new MyPokemonData { PokemonIdx = pokemonId, IsShiny = false };
            }
        }
    }

    public static List<int> StartPokemonList = new List<int> { 1, 4, 7, 10, 13, 16, 19, 21, 23, 25, 27, 29, 32, 37, 41, 43, 46, 48, 50, 52, 54, 56, 58, 60, 63, 66, 69, 72, 74, 77, 79, 81, 83, 84, 86, 88, 90, 92, 95, 96, 98, 100, 102, 104, 108, 109, 111, 114, 115, 116, 118, 120, 123, 127, 128, 129, 131, 132, 133, 137, 138, 140, 142, 144, 145, 146, 147, 150, 151 };
}

