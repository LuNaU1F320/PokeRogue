using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BattleHud : MonoBehaviour
{
    [SerializeField] Text nameTxt;
    [SerializeField] Text levelTxt;
    [SerializeField] HpBar hpbar;
    [SerializeField] Text hpbar_Text;
    [SerializeField] Image PokemonTypeImg;
    [SerializeField] Image PokemonDualTypeImg1;
    [SerializeField] Image PokemonDualTypeImg2;
    Pokemon _pokemon;

    public void SetHud(Pokemon SetPokemon, bool isPlayerUnit)
    {
        PokemonTypeImg.gameObject.SetActive(false);
        PokemonDualTypeImg1.gameObject.SetActive(false);
        PokemonDualTypeImg2.gameObject.SetActive(false);
        _pokemon = SetPokemon;
        nameTxt.text = SetPokemon.PokemonBase.PokemonName;
        levelTxt.text = "" + SetPokemon.PokemonLevel;
        hpbar.SetHp((float)SetPokemon.PokemonHp / SetPokemon.MaxHp);
        if (hpbar_Text != null)
        {
            hpbar_Text.text = $"{SetPokemon.PokemonHp}/{SetPokemon.MaxHp}";
        }
        SetPokemonType(isPlayerUnit);
    }
    void SetPokemonType(bool isPlayerUnit)
    {
        if (isPlayerUnit)
        {
            if (_pokemon.Type2 == PokemonType.None)
            {
                Sprite[] pokemonTypeSprites = Resources.LoadAll<Sprite>("Image/pbinfo_player_type");
                if (PokemonTypeImg != null)
                {
                    PokemonTypeImg.gameObject.SetActive(true);
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
            }
            else
            {
                if (PokemonDualTypeImg1 != null && PokemonDualTypeImg2 != null)
                {
                    Sprite[] pokemonType2Sprites1 = Resources.LoadAll<Sprite>("Image/pbinfo_player_type1");
                    Sprite[] pokemonType2Sprites2 = Resources.LoadAll<Sprite>("Image/pbinfo_player_type2");
                    PokemonDualTypeImg1.gameObject.SetActive(true);
                    PokemonDualTypeImg2.gameObject.SetActive(true);
                    string pokType1 = _pokemon.Type1.ToString();
                    string pokType2 = _pokemon.Type2.ToString();
                    Sprite type2TargetSprite1 = System.Array.Find(pokemonType2Sprites1, sprite => sprite.name == pokType1);
                    Sprite type2TargetSprite2 = System.Array.Find(pokemonType2Sprites2, sprite => sprite.name == pokType2);
                    if (type2TargetSprite1 != null && type2TargetSprite2 != null)
                    {
                        PokemonDualTypeImg1.sprite = type2TargetSprite1;
                        PokemonDualTypeImg2.sprite = type2TargetSprite2;
                    }
                    else
                    {
                        Debug.LogError("듀얼타입 스프라이트 로딩 실패");
                    }
                }
            }
        }
        else
        {
            if (_pokemon.Type2 == PokemonType.None)
            {
                Sprite[] pokemonTypeSprites = Resources.LoadAll<Sprite>("Image/pbinfo_enemy_type");
                if (PokemonTypeImg != null)
                {
                    PokemonTypeImg.gameObject.SetActive(true);
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
            }
            else
            {
                if (PokemonDualTypeImg1 != null && PokemonDualTypeImg2 != null)
                {
                    Sprite[] pokemonType2Sprites1 = Resources.LoadAll<Sprite>("Image/pbinfo_enemy_type1");
                    Sprite[] pokemonType2Sprites2 = Resources.LoadAll<Sprite>("Image/pbinfo_enemy_type2");
                    PokemonDualTypeImg1.gameObject.SetActive(true);
                    PokemonDualTypeImg2.gameObject.SetActive(true);
                    string pokType1 = _pokemon.Type1.ToString();
                    string pokType2 = _pokemon.Type2.ToString();
                    Sprite type2TargetSprite1 = System.Array.Find(pokemonType2Sprites1, sprite => sprite.name == pokType1);
                    Sprite type2TargetSprite2 = System.Array.Find(pokemonType2Sprites2, sprite => sprite.name == pokType2);
                    if (type2TargetSprite1 != null && type2TargetSprite2 != null)
                    {
                        PokemonDualTypeImg1.sprite = type2TargetSprite1;
                        PokemonDualTypeImg2.sprite = type2TargetSprite2;
                    }
                    else
                    {
                        Debug.LogError("듀얼타입 스프라이트 로딩 실패");
                    }
                }
            }
        }
    }

    public IEnumerator UpdateHp()
    {
        yield return hpbar.SetHpSmooth((float)_pokemon.PokemonHp / _pokemon.MaxHp);
    }

    public IEnumerator AnimateTextHp(int startNumber, int endNumber, float animationDuration = 1f /*, Text TextObject = null, string numType = ""*/)
    {
        float elapsedTime = 0f;

        while (elapsedTime < animationDuration)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / animationDuration;
            int currentNumber = Mathf.RoundToInt(Mathf.Lerp(startNumber, endNumber, progress));
            hpbar_Text.text = currentNumber.ToString() + "/" + _pokemon.MaxHp.ToString();
            yield return null;
        }
        hpbar_Text.text = endNumber.ToString() + "/" + _pokemon.MaxHp.ToString();
    }
}
