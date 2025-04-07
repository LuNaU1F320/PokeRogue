using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SimpleJSON;
using System;
using Cysharp.Threading.Tasks;

public class EvolutionManager : MonoBehaviour
{
    public static EvolutionManager Inst;
    [SerializeField] GameObject EvolutionPanel;
    [SerializeField] SpriteRenderer PokemonSprite;
    [SerializeField] BattleDialogBox dialogBox;
    int setPokemonIdx = 0;

    // public event Action OnStartEvolution;
    // public event Action OnCompleteEvolution;

    [HideInInspector] public Dictionary<string, Sprite> sprites = new Dictionary<string, Sprite>();
    [HideInInspector] private List<Sprite> animationFrames = new List<Sprite>();
    [HideInInspector] float frameRate = 0.07f; // 프레임 간격(초)
    private void Awake()
    {
        Inst = this;
    }
    public IEnumerator Evolve(Pokemon pokemon, Evolution evolution)
    {
        // OnStartEvolution?.Invoke();

        EvolutionPanel.SetActive(true);

        setPokemonIdx = pokemon.P_Base.PokemonIndex;
        SetUpPokemonSprite();

        yield return new WaitForSeconds(3.0f);

        var oldPokemon = pokemon.P_Base;
        pokemon.Evolve(evolution);

        setPokemonIdx = pokemon.P_Base.PokemonIndex;
        SetUpPokemonSprite();

        yield return dialogBox.TypeDialog($"{oldPokemon.PokemonName}{GameManager.Inst.GetCorrectParticle(oldPokemon.PokemonName, "topic")} {pokemon.P_Base.PokemonName}{GameManager.Inst.GetCorrectParticle(pokemon.P_Base.PokemonName, "objectTo")} 진화했다!");

        EvolutionPanel.SetActive(false);
    }
    // // 비동기 방식의 포켓몬 진화 처리
    // public async UniTask EvolveAsync(Pokemon pokemon, Evolution evolution)
    // {
    //     // Evolution 시작 UI
    //     EvolutionPanel.SetActive(true);

    //     setPokemonIdx = pokemon.P_Base.PokemonIndex;
    //     SetUpPokemonSprite();

    //     await UniTask.Delay(TimeSpan.FromSeconds(3.0f));

    //     var oldPokemon = pokemon.P_Base;
    //     pokemon.Evolve(evolution);

    //     setPokemonIdx = pokemon.P_Base.PokemonIndex;
    //     SetUpPokemonSprite();

    //     await dialogBox.TypeDialog($"{oldPokemon.PokemonName}{GameManager.Inst.GetCorrectParticle(oldPokemon.PokemonName, "topic")} {pokemon.P_Base.PokemonName}{GameManager.Inst.GetCorrectParticle(pokemon.P_Base.PokemonName, "objectTo")} 진화했다!");

    //     EvolutionPanel.SetActive(false);
    // }
    void SetUpPokemonSprite()
    {
        if (PokemonSprite != null)
        {
            PokemonSprite.sprite = null; // 스프라이트 제거
        }

        if (PokemonSprite == null)
        {
            PokemonSprite = gameObject.AddComponent<SpriteRenderer>();
        }

        StopAllCoroutines(); // 이전 애니메이션 중단
        sprites.Clear();
        animationFrames.Clear(); // 이전 프레임 리스트 초기화

        TextAsset json = Resources.Load<TextAsset>($"Image/Pokemon/PokemonSprite_Front/{setPokemonIdx}");
        Texture2D sprite = Resources.Load<Texture2D>($"Image/Pokemon/PokemonSprite_Front/{setPokemonIdx}");

        if (json == null || sprite == null)
        {
            return;
        }

        LoadSprites(json, sprite);

        CreateAnimationFrames();

        if (animationFrames.Count > 0)
        {
            StartCoroutine(PlayAnimation());
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
            PokemonSprite.sprite = animationFrames[currentFrame];
            currentFrame = (currentFrame + 1) % animationFrames.Count; // 루프를 위해 모듈로 연산
            yield return new WaitForSeconds(frameRate);
        }
    }
}
