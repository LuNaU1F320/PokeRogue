using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BattleHud : MonoBehaviour
{
    [SerializeField] Text nameTxt;
    [SerializeField] Text levelTxt;
    [SerializeField] HpBar hpbar;
    [SerializeField] Image PokemonTypeImg;
    Pokemon _pokemon;

    public void SetHud(Pokemon SetPokemon)
    {
        _pokemon = SetPokemon;
        nameTxt.text = SetPokemon.PokemonBase.PokemonName;
        levelTxt.text = "" + SetPokemon.PokemonLevel;
        hpbar.SetHp((float)SetPokemon.PokemonHp / SetPokemon.MaxHp);
        SetPokemonType();
    }
    void SetPokemonType()
    {
        Sprite[] pokemonTypeSprites = Resources.LoadAll<Sprite>("Image/PokemonType");
        if (_pokemon.Type2 == PokemonType.None)
        {
            string onlypokType = _pokemon.Type1.ToString();
            Sprite typeTargetSprite = System.Array.Find(pokemonTypeSprites, sprite => sprite.name == onlypokType);
            if (typeTargetSprite != null)
            {
                PokemonTypeImg.sprite = typeTargetSprite;
            }
            else
            {
                Debug.LogError($"스프라이트를 찾을 수 없습니다: {onlypokType}");
                foreach (var sprite in pokemonTypeSprites)
                {
                    Debug.Log($"스프라이트 이름: {sprite.name}");
                }
            }
        }
        else
        {
            // string pokType1 = _pokemon.Type1.ToString();
            // string pokType2 = _pokemon.Type1.ToString();
            // Sprite typeTargetSprite = System.Array.Find(pokemonTypeSprites, sprite => sprite.name == pokType1);
            // Sprite type2TargetSprite = System.Array.Find(pokemonTypeSprites, sprite => sprite.name == pokType2);
        }
    }
    public IEnumerator UpdateHp()
    {
        yield return hpbar.SetHpSmooth((float)_pokemon.PokemonHp / _pokemon.MaxHp);
    }
}
