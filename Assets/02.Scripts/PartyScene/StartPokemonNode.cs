using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StartPokemonNode : MonoBehaviour
{
    [SerializeField] public int PokemonIndex;
    [SerializeField] public Image PokemonDot;
    [SerializeField] public Image Select_Cusor;
    [SerializeField] public bool IsCatch;

    int pokemonGen;
    int pokemonValue;

    void Start()
    {
        SetPokemonData();
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void SetPokemonData()
    {
        SetGeneration();
        PokemonDot.sprite = Resources.Load<Sprite>($"Image/Pokemon/PokemonDot/{pokemonGen}/{PokemonIndex}");
        if (!IsCatch)
        {
            PokemonDot.color = Color.black;
        }
    }
    private void SetGeneration()
    {
        int index = PokemonIndex;
        if (index <= 151) // 1세대: 1 ~ 151
        {
            pokemonGen = 1;
        }
        else if (index <= 251) // 2세대: 152 ~ 251
        {
            pokemonGen = 2;
        }
        else if (index <= 386) // 3세대: 252 ~ 386
        {
            pokemonGen = 3;
        }
        else if (index <= 493) // 4세대: 387 ~ 493
        {
            pokemonGen = 4;
        }
        else if (index <= 649) // 5세대: 494 ~ 649
        {
            pokemonGen = 5;
        }
        else if (index <= 721) // 6세대: 650 ~ 721
        {
            pokemonGen = 6;
        }
        else if (index <= 809) // 7세대: 722 ~ 809
        {
            pokemonGen = 7;
        }
        else if (index <= 898) // 8세대: 810 ~ 898
        {
            pokemonGen = 8;
        }
        else // 9세대 이후 (예시로 899 이상)
        {
            pokemonGen = 9;
        }
    }
}
