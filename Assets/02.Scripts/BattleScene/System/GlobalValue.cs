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

        Debug.Log(json.ToString());

        PlayerPrefs.SetString("MyPokemon", json.ToString());
        PlayerPrefs.Save();
    }
    public static void LoadGameInfo()
    {
        UserGold = PlayerPrefs.GetInt("UserGold", 0);
        CurStage = PlayerPrefs.GetInt("CurStage", 1);


        string myPokemonJson = PlayerPrefs.GetString("MyPokemon", "");
        Debug.Log("로드된 JSON 데이터: " + myPokemonJson);  // JSON 출력
        if (!string.IsNullOrEmpty(myPokemonJson))
        {
            JSONNode json = JSON.Parse(myPokemonJson);

            var newPokemonDict = new Dictionary<int, MyPokemonData>(); // 🔹 새 Dictionary 생성

            foreach (KeyValuePair<string, JSONNode> kvp in json) // 🔹 Dictionary처럼 접근
            {
                int pokemonIdx = kvp.Value["PokemonIdx"].AsInt;  // 🔹 kvp.Value를 직접 접근
                bool isShiny = kvp.Value["IsShiny"].AsBool;

                MyPokemon[int.Parse(kvp.Key)] = new MyPokemonData
                {
                    PokemonIdx = pokemonIdx,
                    IsShiny = isShiny
                };
            }

            MyPokemon = newPokemonDict; // 🔹 마지막에 한 번에 교체
        }
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
}
