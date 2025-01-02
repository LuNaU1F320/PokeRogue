using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BattleUnit : MonoBehaviour
{
    [SerializeField] PokemonBase basePokemon;
    [SerializeField] int level;
    [SerializeField] bool isPlayerUnit;

    public Pokemon BattlePokemon { get; set; }
    public void SetUp()
    {
        BattlePokemon = new Pokemon(basePokemon, level);
        
        if (isPlayerUnit )
        {
            GetComponent<Image>().sprite = BattlePokemon.PokemonBase.BackSprite;
            GetComponent<Image>().SetNativeSize();
        }
        else
        {
            GetComponent<Image>().sprite = BattlePokemon.PokemonBase.FrontSprite;
            GetComponent<Image>().SetNativeSize();
        }
    }

}
