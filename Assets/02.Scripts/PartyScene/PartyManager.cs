using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using SimpleJSON;
using System;
using UnityEngine.SceneManagement;

public class PartyManager : MonoBehaviour
{
    [SerializeField] List<StartPokemonNode> pokemonNodes;
    [SerializeField] List<Image> PlayerParty;
    [SerializeField] List<Text> confirmTexts;

    [SerializeField] Text PokemonIdx_Text;
    [SerializeField] Text PokemonName_Text;
    [SerializeField] SpriteRenderer Pokemon_Sprite;
    [SerializeField] Text PokemonValue_Text;


    [HideInInspector] public Dictionary<string, Sprite> sprites = new Dictionary<string, Sprite>();
    [HideInInspector] private List<Sprite> animationFrames = new List<Sprite>();
    [HideInInspector] float frameRate = 0.1f; // 프레임 간격(초)


    StartPokemonNode selectPokemon;
    int currentSelection = 0;
    int previousPokemonIdx = -1; // 이전 포켓몬 인덱스 저장 변수
    int setPokemonIdx = 0;
    private int currentPartyIndex = 0;


    // Start is called before the first frame update
    void Start()
    {
        PokemonValue_Text.text = "0/10";
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.X))
        {
            SceneManager.LoadScene("BattleScene");
        }
        HandleScreenCusor();
    }

    void HandlePokemonCusor()
    {

    }

    void HandleScreenCusor()
    {
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            ++currentSelection;
            // if (currentSelection < 9)
            // {
            // }
        }
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            if (0 < currentSelection)
            {
                --currentSelection;
            }
        }
        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            currentSelection = currentSelection + 9;
            // if (currentSelection < 2)
            // {
            // }
        }

        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            if (8 < currentSelection)
            {
                currentSelection = currentSelection - 9;
            }
        }
        currentSelection = Mathf.Clamp(currentSelection, 0, pokemonNodes.Count - 1);
        UpdateScreenSelection();
        if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return))
        {
            SetPlayerParty(selectPokemon);
            Debug.Log($"{setPokemonIdx}");
        }
    }
    public void UpdateScreenSelection()
    {
        for (int i = 0; i < pokemonNodes.Count; i++)
        {
            if (i == currentSelection)
            {
                selectPokemon = pokemonNodes[i];
                pokemonNodes[i].Select_Cusor.gameObject.SetActive(true);
                setPokemonIdx = pokemonNodes[i].PokemonIndex;
                PokemonIdx_Text.text = pokemonNodes[i].PokemonIndex.ToString("0000");
                PokemonName_Text.text = pokemonNodes[i].PokemonIndex.ToString();


                if (pokemonNodes[i].IsCatch == false)
                {
                    Pokemon_Sprite.color = Color.black;
                }
                else if (pokemonNodes[i].IsCatch == true)
                {
                    Pokemon_Sprite.color = Color.white;
                }

                // 이전 선택과 현재 선택이 다를 경우에만 SetUpPokemonSprite 호출
                if (previousPokemonIdx != setPokemonIdx)
                {
                    SetUpPokemonSprite();
                    previousPokemonIdx = setPokemonIdx; // 이전 선택값 갱신
                }
            }
            else
            {
                pokemonNodes[i].Select_Cusor.gameObject.SetActive(false);
            }
        }
    }
    public void SetPlayerParty(StartPokemonNode pokemon)
    {
        if (currentPartyIndex < PlayerParty.Count)
        {
            // 0번부터 순차적으로 포켓몬 이미지를 추가
            PlayerParty[currentPartyIndex].sprite = pokemon.PokemonDot.sprite;
            Debug.Log($"PlayerParty[{currentPartyIndex}]에 포켓몬이 등록되었습니다.");

            // 다음 슬롯으로 인덱스 이동
            currentPartyIndex++;
        }
        else
        {
            // 파티가 가득 찬 경우
            Debug.LogWarning("PlayerParty가 가득 찼습니다. 더 이상 추가할 수 없습니다.");
        }
        // PlayerParty.Add(pokemon.PokemonDot);
    }
    #region SetPokemonSprite
    void SetUpPokemonSprite()
    {
        if (Pokemon_Sprite != null)
        {
            Pokemon_Sprite.sprite = null;
        }

        StopAllCoroutines(); // 이전 애니메이션 중단
        sprites.Clear();
        animationFrames.Clear(); // 이전 프레임 리스트 초기화

        TextAsset json;
        Texture2D sprite;

        json = Resources.Load<TextAsset>($"Image/Pokemon/PokemonSprite_Front/{setPokemonIdx}");
        sprite = Resources.Load<Texture2D>($"Image/Pokemon/PokemonSprite_Front/{setPokemonIdx}");

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
            Pokemon_Sprite.sprite = animationFrames[currentFrame];
            currentFrame = (currentFrame + 1) % animationFrames.Count; // 루프를 위해 모듈로 연산
            yield return new WaitForSeconds(frameRate);
        }
    }
    #endregion
}
