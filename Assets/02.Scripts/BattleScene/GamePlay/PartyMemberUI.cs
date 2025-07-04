using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PartyMemberUI : MonoBehaviour
{
    [SerializeField] Text nameTxt;
    [SerializeField] Text levelTxt;
    [SerializeField] HpBar hpbar;
    [SerializeField] Text hpbar_Text;
    [SerializeField] SpriteRenderer pokemonDotSprite;
    [SerializeField] Image selectedImg;
    Pokemon _pokemon;

    public void SetData(Pokemon SetPokemon)
    {
        _pokemon = SetPokemon;
        nameTxt.text = SetPokemon.P_Base.PokemonName;
        levelTxt.text = "" + SetPokemon.PokemonLevel;
        hpbar.SetHp((float)SetPokemon.PokemonHp / SetPokemon.MaxHp);
        if (hpbar_Text != null)
        {
            hpbar_Text.text = $"{SetPokemon.PokemonHp}/{SetPokemon.MaxHp}";
        }
        pokemonDotSprite.sprite = Resources.Load<Sprite>($"Image/Pokemon/PokemonDot/{SetPokemon.PokemonGen}/{SetPokemon.P_Base.PokemonIndex}");

        // var spritePath = $"Image/Pokemon/PokemonDot/{SetPokemon.PokemonGen}/{SetPokemon.P_Base.PokemonIndex}";
        // Debug.Log($"📦 Sprite 로드 경로: {spritePath}");

        // var sprite = Resources.Load<Sprite>(spritePath);
        // if (sprite == null)
        // {
        //     Debug.LogError($"❌ 스프라이트 로드 실패! 값들 - Gen: {SetPokemon.PokemonGen}, Index: {SetPokemon.P_Base.PokemonIndex}");
        // }
    }
    public void SetSelected(bool selected, int memberIndex)
    {
        string spritePath = (memberIndex == 0)
        ? "Image/UI/party_slot_main"
        : "Image/UI/party_slot";
        Sprite[] sprites = Resources.LoadAll<Sprite>(spritePath);
        if (selected)
        {
            selectedImg.sprite = sprites[1];
            if (sprites == null)
            {
                Debug.Log("파일찾지못함");
            }
        }
        else
        {
            selectedImg.sprite = sprites[0];
            if (sprites == null)
            {
                Debug.Log("파일찾지못함");
            }
        }
    }
}
