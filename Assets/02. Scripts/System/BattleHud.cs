using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BattleHud : MonoBehaviour
{
    [SerializeField] Text nameTxt;
    [SerializeField] Text levelTxt;
    [SerializeField] HpBar hpbar;
    [SerializeField] Image Type;
    Pokemon _pokemon;

    public void SetHud(Pokemon SetPokemon)
    {
        _pokemon = SetPokemon;
        nameTxt.text = SetPokemon.PokemonBase.PokemonName;
        levelTxt.text = "" + SetPokemon.PokemonLevel;
        hpbar.SetHp((float)SetPokemon.PokemonHp / SetPokemon.MaxHp);
    }
    public IEnumerator UpdateHp()
    {
        yield return hpbar.SetHpSmooth((float)_pokemon.PokemonHp / _pokemon.MaxHp);
    }
}
