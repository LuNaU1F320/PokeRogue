using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlobalValue : MonoBehaviour
{

    public static int UserGold;
    public static int CurStage;

    public struct MyPokemonData
    {
        public int PokemonIdx;
        public bool IsShiny;
    }
    public static Dictionary<int, MyPokemonData> MyPokemon = new Dictionary<int, MyPokemonData>();


    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
}
