using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;
using System;

public class BattleHud : MonoBehaviour
{
    [SerializeField] Text nameTxt;
    [SerializeField] Text levelTxt;
    [SerializeField] HpBar hpbar;
    [SerializeField] Image ExpBar;

    [SerializeField] public Text hpbar_Text;
    [SerializeField] Image PokemonTypeImg;
    [SerializeField] Image PokemonDualTypeImg1;
    [SerializeField] Image PokemonDualTypeImg2;
    [SerializeField] Image Status_Img;
    Pokemon _pokemon;

    public void SetHud(Pokemon SetPokemon, bool isPlayerUnit)
    {
        PokemonTypeImg.gameObject.SetActive(false);
        PokemonDualTypeImg1.gameObject.SetActive(false);
        PokemonDualTypeImg2.gameObject.SetActive(false);
        Status_Img.gameObject.SetActive(false);
        _pokemon = SetPokemon;
        nameTxt.text = SetPokemon.P_Base.PokemonName;
        hpbar.SetHp((float)SetPokemon.PokemonHp / SetPokemon.MaxHp);
        SetLevel();
        SetExp();
        if (hpbar_Text != null)
        {
            hpbar_Text.text = $"{SetPokemon.PokemonHp}/{SetPokemon.MaxHp}";
        }
        SetPokemonType(isPlayerUnit);
        SetStatusIMG();
        _pokemon.OnStatusChanged += SetStatusIMG;
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
    void SetStatusIMG()
    {
        if (_pokemon.Status == null)
        {
            Status_Img.gameObject.SetActive(false);
            return;
        }
        else
        {
            Status_Img.gameObject.SetActive(true);
            Status_Img.sprite = Resources.Load<Sprite>($"Image/UI/statuses/{_pokemon.Status.Id.ToString().ToUpper()}");
        }
    }
    public void SetExp()
    {
        if (ExpBar == null)
        {
            return;
        }
        float normalizeExp = GetNormalizedExp();
        ExpBar.fillAmount = normalizeExp;
    }
    // public IEnumerator SetExpSmooth(bool reset = false)
    // {
    //     if (ExpBar == null)
    //     {
    //         yield break;
    //     }
    //     if (reset == true)
    //     {
    //         ExpBar.fillAmount = 0f;
    //     }
    //     float currentExp = ExpBar.fillAmount; // 현재 경험치 바 상태
    //     float targetExp = GetNormalizedExp(); // 목표 경험치 바 상태
    //     float duration = 0.5f / Mathf.Max(GlobalValue.ExpBarSpeed, 0.01f);
    //     float elapsedTime = 0f;

    //     while (elapsedTime < duration)
    //     {
    //         elapsedTime += Time.deltaTime;
    //         ExpBar.fillAmount = Mathf.Lerp(currentExp, targetExp, elapsedTime / duration);
    //         yield return null;
    //     }

    //     ExpBar.fillAmount = targetExp; // 최종 값 보정
    //     yield return new WaitForSeconds(duration);
    // }
    public async UniTask SetExpSmooth(bool reset = false)
    {
        if (ExpBar == null)
        {
            return;
        }
        if (reset)
        {
            ExpBar.fillAmount = 0f;
        }

        float currentExp = ExpBar.fillAmount;
        float targetExp = GetNormalizedExp();
        float duration = 0.5f / Mathf.Max(GlobalValue.ExpBarSpeed, 0.01f);
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            ExpBar.fillAmount = Mathf.Lerp(currentExp, targetExp, elapsedTime / duration);
            await UniTask.Yield();
        }

        ExpBar.fillAmount = targetExp;
        await UniTask.Delay(TimeSpan.FromSeconds(duration));
    }

    public void SetLevel()
    {
        levelTxt.text = "" + _pokemon.PokemonLevel;
    }
    public float GetNormalizedExp()
    {
        int curLevelExp = _pokemon.P_Base.GetExpForLevel(_pokemon.PokemonLevel);
        int nextLevelExp = _pokemon.P_Base.GetExpForLevel(_pokemon.PokemonLevel + 1);

        float normalizeExp = (float)(_pokemon.PokemonExp - curLevelExp) / (nextLevelExp - curLevelExp);
        return Mathf.Clamp01(normalizeExp);
    }
    public IEnumerator UpdateHp()
    {
        if (_pokemon.IsHpChanged)
        {

            StartCoroutine(hpbar.SetHpSmooth((float)_pokemon.PokemonHp / _pokemon.MaxHp));
            if (hpbar_Text != null)
            {
                StartCoroutine(AnimateTextHp(_pokemon.startHp, _pokemon.PokemonHp));
            }
            yield return null;
            _pokemon.IsHpChanged = false;
        }
    }
    // public async UniTask UpdateHp()
    // {
    //     if (_pokemon.IsHpChanged)
    //     {
    //         var barTask = hpbar.SetHpSmooth((float)_pokemon.PokemonHp / _pokemon.MaxHp);
    //         var textTask = hpbar_Text != null
    //             ? AnimateTextHpAsync(_pokemon.startHp, _pokemon.PokemonHp)
    //             : UniTask.CompletedTask;

    //         await UniTask.WhenAll(barTask, textTask);

    //         _pokemon.IsHpChanged = false;
    //     }
    // }
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
    // public async UniTask AnimateTextHpAsync(int startHp, int endHp)
    // {
    //     int currentHp = startHp;
    //     while (currentHp > endHp)
    //     {
    //         currentHp--;
    //         hpbar_Text.text = $"{currentHp} / {_pokemon.MaxHp}";
    //         await UniTask.Delay(TimeSpan.FromSeconds(0.02f));
    //     }

    //     hpbar_Text.text = $"{endHp} / {_pokemon.MaxHp}";
    // }

}
