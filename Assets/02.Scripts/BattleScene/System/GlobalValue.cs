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
        PlayerPrefs.Save();
    }
    public static void LoadGameInfo()
    {
        UserGold = PlayerPrefs.GetInt("UserGold", 0);
        CurStage = PlayerPrefs.GetInt("CurStage", 1);

        string myPokemonJson = PlayerPrefs.GetString("MyPokemon", "");
        if (!string.IsNullOrEmpty(myPokemonJson))
        {
            JSONNode json = JSON.Parse(myPokemonJson);
            MyPokemon.Clear();

            foreach (var key in json.Keys)
            {
                int pokemonIdx = json[key.ToString()]["PokemonIdx"].AsInt;
                bool isShiny = json[key.ToString()]["IsShiny"].AsBool;

                MyPokemon[int.Parse(key)] = new MyPokemonData
                {
                    PokemonIdx = pokemonIdx,
                    IsShiny = isShiny
                };
            }

        }
    }

    public static void CatchPokemon(int id, bool isShiny)
    {
        if (!MyPokemon.ContainsKey(id))
        {
            MyPokemon[id] = new MyPokemonData { PokemonIdx = id, IsShiny = isShiny };
            SaveGameInfo();
        }
    }
}
