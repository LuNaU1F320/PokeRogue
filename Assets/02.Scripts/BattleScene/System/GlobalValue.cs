using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SimpleJSON;

public class GlobalValue
{

    public static int UserGold;
    public static int CurStage;

    public struct MyPokemonData
    {
        public int PokemonIdx;
        public bool IsShiny;
    }
    public static Dictionary<int, MyPokemonData> MyPokemon = new Dictionary<int, MyPokemonData>();

    public static float GameSpeed = 1f;
    public static float HpBarSpeed = 1.0f;
    public static float ExpBarSpeed = 1.0f;


    public static void SaveGameInfo()
    {
        PlayerPrefs.SetInt("UserGold", UserGold);
        PlayerPrefs.SetInt("CurStage", CurStage);


        // JSON 객체 생성
        JSONNode json = new JSONObject();
        foreach (var kvp in MyPokemon)
        {
            JSONNode pokemonNode = new JSONObject();
            pokemonNode["PokemonIdx"] = kvp.Value.PokemonIdx;
            pokemonNode["IsShiny"] = kvp.Value.IsShiny;

            json[kvp.Key.ToString()] = pokemonNode;
        }

        PlayerPrefs.SetString("MyPokemon", json.ToString());
        Debug.Log("저장된 JSON 데이터: " + PlayerPrefs.GetString("MyPokemon", ""));

        PlayerPrefs.Save();
    }
    public static void LoadGameInfo()
    {
        if (MyPokemon == null)
        {
            MyPokemon = new Dictionary<int, MyPokemonData>();
        }

        UserGold = PlayerPrefs.GetInt("UserGold", 0);
        CurStage = PlayerPrefs.GetInt("CurStage", 1);


        string myPokemonJson = PlayerPrefs.GetString("MyPokemon", "");
        // Debug.Log("로드된 JSON 데이터: " + myPokemonJson);  // JSON 출력

        if (!string.IsNullOrEmpty(myPokemonJson))
        {
            JSONNode json = JSON.Parse(myPokemonJson);

            if (MyPokemon == null)
            {
                MyPokemon = new Dictionary<int, MyPokemonData>();
            }
            else
            {
                MyPokemon.Clear();
            }

            foreach (KeyValuePair<string, JSONNode> kvp in json)
            {
                int pokemonIdx = kvp.Value["PokemonIdx"].AsInt;
                bool isShiny = kvp.Value["IsShiny"].AsBool;

                MyPokemon[int.Parse(kvp.Key)] = new MyPokemonData
                {
                    PokemonIdx = pokemonIdx,
                    IsShiny = isShiny
                };
            }
        }

        SetBasicStartPokemon();
    }
    public static void SaveSetting(List<int> selectedOptions)
    {
        for (int i = 0; i < selectedOptions.Count; i++)
        {
            PlayerPrefs.SetInt("Option_" + i, selectedOptions[i]);
        }

        PlayerPrefs.Save();

        // Debug.Log($"[설정 저장] GameSpeed: {GameSpeed}, HpBarSpeed: {HpBarSpeed}");
    }
    public static List<int> LoadSetting(int optionCount)
    {
        List<int> indices = new List<int>();
        for (int i = 0; i < optionCount; i++)
        {
            indices.Add(PlayerPrefs.GetInt("Option_" + i, 0));
        }
        return indices;
    }

    public static void CatchPokemon(PokemonBase pokemon, bool isShiny)
    {
        int id = pokemon.EvolutionBase.PokemonIndex;
        if (!MyPokemon.ContainsKey(id))
        {
            MyPokemon[id] = new MyPokemonData { PokemonIdx = id, IsShiny = isShiny };
            SaveGameInfo();
        }
    }

    static void SetBasicStartPokemon()
    {
        int[] defaultPokemons = { 1, 4, 7 };  // 기본 제공 포켓몬 ID 목록

        foreach (int pokemonId in defaultPokemons)
        {
            if (!MyPokemon.ContainsKey(pokemonId))  // 이미 존재하는 경우 덮어씌우지 않음
            {
                MyPokemon[pokemonId] = new MyPokemonData { PokemonIdx = pokemonId, IsShiny = false };
            }
        }

        SaveGameInfo();  // 변경된 데이터 저장
    }

    public static List<int> StartPokemonList = new List<int> { 1, 4, 7, 10, 13, 16, 19, 21, 23, 25, 27, 29, 32, 37, 41, 43, 46, 48, 50, 52, 54, 56, 58, 60, 63, 66, 69, 72, 74, 77, 79, 81, 83, 84, 86, 88, 90, 92, 95, 96, 98, 100, 102, 104, 108, 109, 111, 114, 115, 116, 118, 120, 123, 127, 128, 129, 131, 132, 133, 137, 138, 140, 142, 144, 145, 146, 147, 150, 151 };
}

