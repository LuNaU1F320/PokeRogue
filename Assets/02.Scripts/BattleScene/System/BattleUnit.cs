using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using SimpleJSON;
using System;

public class BattleUnit : MonoBehaviour
{
    [SerializeField] bool isPlayerUnit;
    public bool IsPlayerUnit
    {
        get
        {
            return isPlayerUnit;
        }
    }
    [SerializeField] BattleHud battlehud;
    public BattleHud BattleHud
    {
        get
        {
            return battlehud;
        }
    }

    [HideInInspector] public Dictionary<string, Sprite> sprites = new Dictionary<string, Sprite>();
    [HideInInspector] public SpriteRenderer spriteRenderer;
    [HideInInspector] private List<Sprite> animationFrames = new List<Sprite>();
    [HideInInspector] float frameRate = 0.07f; // 프레임 간격(초)

    public Pokemon BattlePokemon { get; set; }
    public void SetUp(Pokemon pokemon)
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.sprite = null; // 스프라이트 제거
        }

        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
        }

        StopAllCoroutines(); // 이전 애니메이션 중단
        // StopCoroutine(PlayAnimation());
        sprites.Clear();
        animationFrames.Clear(); // 이전 프레임 리스트 초기화

        BattlePokemon = pokemon;
        TextAsset json;
        Texture2D sprite;
        if (isPlayerUnit)
        {
            json = Resources.Load<TextAsset>($"Image/Pokemon/PokemonSprite_Back/{pokemon.P_Base.PokemonIndex}");
            sprite = Resources.Load<Texture2D>($"Image/Pokemon/PokemonSprite_Back/{pokemon.P_Base.PokemonIndex}");

        }
        else
        {
            json = Resources.Load<TextAsset>($"Image/Pokemon/PokemonSprite_Front/{pokemon.P_Base.PokemonIndex}");
            sprite = Resources.Load<Texture2D>($"Image/Pokemon/PokemonSprite_Front/{pokemon.P_Base.PokemonIndex}");
        }

        battlehud.SetHud(pokemon, isPlayerUnit);

        LoadSprites(json, sprite);
        CreateAnimationFrames();
        if (animationFrames.Count > 0)
        {
            StartCoroutine(PlayAnimation());
        }

        // Debug.Log($"Loaded {sprites.Count} sprites.");
        // Debug.Log($"Loaded {animationFrames.Count} animationFrames.");
    }
    public void LoadSprites(TextAsset pokIdxjson, Texture2D pokIdxsprite)
    {
        JSONNode node = JSON.Parse(pokIdxjson.text);
        var frames = node["textures"][0]["frames"].AsArray;

        for (int i = 0; i < frames.Count; i++)
        {
            var frameNode = frames[i];
            string filename = frameNode["filename"];
            var frame = frameNode["frame"];

            // 프레임 데이터 추출
            float x = frame["x"].AsFloat;
            float y = pokIdxsprite.height - frame["y"].AsFloat - frame["h"].AsFloat;
            float w = frame["w"].AsFloat;
            float h = frame["h"].AsFloat;

            Rect rect = new Rect(x, y, w, h);
            Vector2 pivot = new Vector2(0.5f, 0.5f); // 중심 피벗

            try
            {
                Sprite sprite = Sprite.Create(pokIdxsprite, rect, pivot);
                sprites[filename] = sprite;
            }
            catch (ArgumentException e)
            {
                Debug.LogError($"Failed to create sprite for {filename}. Error: {e.Message}");
            }
        }
    }
    void CreateAnimationFrames()
    {
        // 여기서는 예시로 스프라이트 이름이 0001.png, 0002.png, ... 순으로 있다고 가정합니다.
        for (int i = 1; i <= sprites.Count; i++)
        {
            string key = i.ToString("0000") + ".png";
            if (sprites.ContainsKey(key))
            {
                animationFrames.Add(sprites[key]);
            }
        }
    }
    IEnumerator PlayAnimation()
    {
        // yield return new WaitForSeconds(0.5f);
        if (animationFrames.Count == 0)
        {
            yield break;
        }

        int currentFrame = 0;
        while (true)
        {
            spriteRenderer.sprite = animationFrames[currentFrame];
            currentFrame = (currentFrame + 1) % animationFrames.Count; // 루프를 위해 모듈로 연산
            yield return new WaitForSeconds(frameRate);
        }
    }

}
