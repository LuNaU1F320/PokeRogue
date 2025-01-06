using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using SimpleJSON;
using System;

public class BattleUnit : MonoBehaviour
{
    // [SerializeField] PokemonBase basePokemon;
    // [SerializeField] int level;
    [SerializeField] bool isPlayerUnit;

    //
    [HideInInspector] public Dictionary<string, Sprite> sprites = new Dictionary<string, Sprite>();
    [HideInInspector] private SpriteRenderer spriteRenderer;
    [HideInInspector] private List<Sprite> animationFrames = new List<Sprite>();
    [HideInInspector] float frameRate = 0.05f; // 프레임 간격(초)

    //

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
        animationFrames.Clear(); // 이전 프레임 리스트 초기화

        // BattlePokemon = new Pokemon(basePokemon, level);
        BattlePokemon = pokemon;

        if (isPlayerUnit)
        {
            TextAsset json = Resources.Load<TextAsset>($"Image/Pokemon/PokemonSprite_Back/{pokemon.PokemonBase.PokemonIndex}");
            Texture2D sprite = Resources.Load<Texture2D>($"Image/Pokemon/PokemonSprite_Back/{pokemon.PokemonBase.PokemonIndex}");
            LoadSprites(json, sprite);
            CreateAnimationFrames();
            if (animationFrames.Count > 0)
            {
                StartCoroutine(PlayAnimation());
            }
            // StartCoroutine(PlayAnimation());
            // Debug.Log("P_Mon : " + json.name + ".json" + sprite.name + ".png");
        }
        else
        {
            TextAsset json = Resources.Load<TextAsset>($"Image/Pokemon/PokemonSprite_Front/{pokemon.PokemonBase.PokemonIndex}");
            Texture2D sprite = Resources.Load<Texture2D>($"Image/Pokemon/PokemonSprite_Front/{pokemon.PokemonBase.PokemonIndex}");
            LoadSprites(json, sprite);
            CreateAnimationFrames();
            StartCoroutine(PlayAnimation());
            // Debug.Log("E_Mon : " + json.name + ".json" + sprite.name + ".png");
        }
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
            float y = pokIdxsprite.height - frame["y"].AsFloat - frame["h"].AsFloat; // y 좌표 수정
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
    /*
        public void LoadSprites(TextAsset pokIdxjson, Texture2D pokIdxsprite)
        {
            JSONNode node = JSON.Parse(pokIdxjson.text);
            var frames = node["textures"][0]["frames"].AsArray;

            for (int i = 0; i < frames.Count; i++)
            {
                var frameNode = frames[i];
                string filename = frameNode["filename"];
                var frame = frameNode["frame"];
                var sourceSize = frameNode["sourceSize"];
                var spriteSourceSize = frameNode["spriteSourceSize"];

                // sourceSize로 전체 스프라이트 크기를 정의
                float sourceX = frame["x"].AsFloat;
                float sourceY = pokIdxsprite.height - (frame["y"].AsFloat + sourceSize["h"].AsFloat); // sourceSize의 높이로 y를 계산
                float sourceW = sourceSize["w"].AsFloat;
                float sourceH = sourceSize["h"].AsFloat;

                // spriteSourceSize를 사용하여 실제 사용할 스프라이트의 위치와 크기 정의
                float x = sourceX + spriteSourceSize["x"].AsFloat;
                float y = sourceY + spriteSourceSize["y"].AsFloat;
                float w = spriteSourceSize["w"].AsFloat;
                float h = spriteSourceSize["h"].AsFloat;

                // y 좌표가 텍스처 경계를 벗어나지 않도록 조정
                // y = Mathf.Clamp(y, 0, pokIdxsprite.height - h);

                Rect rect = new Rect(x, y, w, h);
                Vector2 pivot = new Vector2(0.5f, 0.5f); // 중심 피벗

                try
                {
                    Sprite sprite = Sprite.Create(pokIdxsprite, rect, pivot);
                    sprites[filename] = sprite; // 예제에서 key를 filename으로 변경
                }
                catch (ArgumentException e)
                {
                    Debug.LogError($"Failed to create sprite for {filename}. Error: {e.Message}");
                }
            }
        }
    */
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
        if (animationFrames.Count == 0) yield break;

        int currentFrame = 0;
        while (true)
        {
            spriteRenderer.sprite = animationFrames[currentFrame];
            currentFrame = (currentFrame + 1) % animationFrames.Count; // 루프를 위해 모듈로 연산
            yield return new WaitForSeconds(frameRate);
        }
    }

}
