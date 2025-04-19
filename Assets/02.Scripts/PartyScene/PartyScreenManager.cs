using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using SimpleJSON;
using System;
using UnityEngine.SceneManagement;

public class PartyScreenManager : MonoBehaviour
{
    PokemonBase _base;

    [SerializeField] GameObject pokemonNodePrefab;
    [SerializeField] Transform contentPanel;
    [HideInInspector] private List<GameObject> Nodes = new List<GameObject>();
    [SerializeField] GameObject StartCheck_Img;

    private List<Pokemon> selectedPokemons = new List<Pokemon>();
    PokemonParty playerParty;
    [SerializeField] List<Image> PartyPokemon_Img;
    [SerializeField] Sprite DefaultParty_Sprite;

    [SerializeField] Text PokemonIdx_Text;
    [SerializeField] Text PokemonName_Text;
    [SerializeField] Text PokemonCost_Text;
    [SerializeField] SpriteRenderer Pokemon_Sprite;
    [SerializeField] Text PokemonValue_Text;
    int PokemonValue = 0;

    List<StartPokemonNode> pokemonNodes = new List<StartPokemonNode>();
    StartPokemonNode selectPokemonNode;

    //Sprite
    [HideInInspector] public Dictionary<string, Sprite> sprites = new Dictionary<string, Sprite>();
    [HideInInspector] private List<Sprite> animationFrames = new List<Sprite>();
    [HideInInspector] float frameRate = 0.1f; // 프레임 간격(초)
    //Sprite

    int currentSelection = 0;
    int previousPokemonIdx = -1; // 이전 포켓몬 인덱스 저장 변수
    int setPokemonIdx = 0;
    private int currentPartyIndex = 0;
    int lastIndex; // 마지막 노드 인덱스
    int previousSelection = -1; // 마지막 노드에 들어가기 전 위치 저장 변수
    GameObject lastNodeImageObject;

    [SerializeField] Image PokemonTypeImg1;
    [SerializeField] Image PokemonTypeImg2;

    [SerializeField] ConfigPanel configPanel;

    void Start()
    {
        playerParty = FindObjectOfType<PokemonParty>();
        GeneratePokemonNodes();
        PokemonValue = 0;
        PokemonValue_Text.text = "0/10";

        configPanel.StartSetting();
        Sound_Manager.Instance.PlayBGM("BGM/menu");
    }
    void Update()
    {
        if (!configPanel.gameObject.activeSelf)
        {
            HandleScreenCusor();
        }
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (configPanel.gameObject.activeSelf)
            {
                if (configPanel.state == ConfigState.Config_Right)
                {
                    if (Input.GetKeyDown(KeyCode.Escape))
                    {
                        Sound_Manager.Instance.PlayGUISound("UI/menu_open");
                        configPanel.gameObject.SetActive(false);
                    }
                }
            }
            else
            {
                Sound_Manager.Instance.PlayGUISound("UI/menu_open");
                configPanel.gameObject.SetActive(true);
            }
        }
    }
    void GeneratePokemonNodes()
    {
        foreach (int pokemonIndex in GlobalValue.StartPokemonList)
        {
            bool isCatch = GlobalValue.MyPokemon.ContainsKey(pokemonIndex);

            GameObject newNode = Instantiate(pokemonNodePrefab, contentPanel);
            StartPokemonNode nodeComponent = newNode.GetComponent<StartPokemonNode>();

            if (nodeComponent != null)
            {
                nodeComponent.Init(pokemonIndex, isCatch);
                Nodes.Add(newNode);
            }
        }

        if (StartCheck_Img != null)
        {
            Nodes.Add(StartCheck_Img);
        }

        foreach (GameObject node in Nodes)
        {
            StartPokemonNode pokemonNode = node.GetComponent<StartPokemonNode>();
            if (pokemonNode != null)
            {
                pokemonNodes.Add(pokemonNode);
            }
        }

        lastIndex = Nodes.Count - 1;
        lastNodeImageObject = Nodes[lastIndex];

        if (lastNodeImageObject != null)
        {
            lastNodeImageObject.SetActive(false);
        }

        if (pokemonNodes.Count > 0)
        {
            currentSelection = 0;
            UpdateScreenSelection();
        }
    }

    void HandleScreenCusor()
    {
        int rowSize = 9; // 가로에 9개씩 배치됨
        int rowEnd = ((currentSelection / rowSize) + 1) * rowSize - 1; // 현재 선택한 행의 마지막 인덱스

        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            if (currentSelection == rowEnd && currentSelection < lastIndex)
            {
                // 이전 위치 저장 후 마지막 노드로 이동
                previousSelection = currentSelection;
                currentSelection = lastIndex;
            }
            else if (currentSelection < lastIndex)
            {
                currentSelection++;
            }
            Sound_Manager.Instance.PlayGUISound("UI/select");
        }
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            if (currentSelection == lastIndex && previousSelection != -1)
            {
                // 마지막 노드에서 이전 위치로 복귀
                currentSelection = previousSelection;
                previousSelection = -1; // 복귀 후 다시 초기화
            }
            else if (currentSelection > 0)
            {
                currentSelection--;
            }
            Sound_Manager.Instance.PlayGUISound("UI/select");
        }
        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            currentSelection += rowSize;
            Sound_Manager.Instance.PlayGUISound("UI/select");
        }

        // 위쪽 이동
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            if (currentSelection >= rowSize)
            {
                currentSelection -= rowSize;
            }
            Sound_Manager.Instance.PlayGUISound("UI/select");
        }
        currentSelection = Mathf.Clamp(currentSelection, 0, lastIndex);
        UpdateScreenSelection();
        if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return))
        {
            if (currentSelection == lastIndex)
            {
                if (PokemonValue == 0)
                {
                    Debug.Log("선택안됨");
                    Sound_Manager.Instance.PlayGUISound("UI/error");
                    return;
                }
                Sound_Manager.Instance.PlayGUISound("UI/select");
                SceneManager.LoadScene("BattleScene");
            }
            else
            {
                if (selectPokemonNode != null)
                {
                    if (selectPokemonNode.IsCatch == false)
                    {
                        Debug.Log("안댐");
                        Sound_Manager.Instance.PlayGUISound("UI/error");
                        return;
                    }
                    SetPlayerParty(selectPokemonNode);
                }
            }
        }

        if (Input.GetKeyDown(KeyCode.Backspace))
        {
            Sound_Manager.Instance.PlayGUISound("UI/select");
            RemovePlayerParty();
        }
    }
    public void UpdateScreenSelection()
    {
        for (int i = 0; i < Nodes.Count; i++)
        {
            if (i == currentSelection)
            {
                StartPokemonNode nodeComponent = Nodes[i].GetComponent<StartPokemonNode>();
                if (nodeComponent != null)
                {
                    selectPokemonNode = nodeComponent;
                    nodeComponent.Select_Cursor.gameObject.SetActive(true);
                    setPokemonIdx = nodeComponent.PokemonIndex;
                    Pokemon_Sprite.color = nodeComponent.IsCatch ? Color.white : Color.black;

                    // 이전 선택과 현재 선택이 다를 경우에만 SetUpPokemonSprite 호출
                    if (previousPokemonIdx != setPokemonIdx)
                    {
                        PokemonIdx_Text.text = nodeComponent.PokemonIndex.ToString("0000");
                        _base = PokemonDB.GetPokemonByIndex(setPokemonIdx);
                        if (_base != null)
                        {
                            if (nodeComponent.IsCatch)
                            {
                                PokemonName_Text.text = _base.PokemonName;
                                PokemonCost_Text.text = _base.Cost.ToString();
                                SetPokemonType(true);
                            }
                            else
                            {
                                PokemonName_Text.text = "???";
                                PokemonCost_Text.text = "???";
                                SetPokemonType(false);
                            }
                        }
                        SetUpPokemonSprite();

                        previousPokemonIdx = setPokemonIdx; // 이전 선택값 갱신
                    }
                    if (lastNodeImageObject != null)
                    {
                        lastNodeImageObject.SetActive(false);
                    }
                }
                else
                {
                    selectPokemonNode = null;
                    if (i == lastIndex && lastNodeImageObject != null)
                    {
                        lastNodeImageObject.SetActive(true);
                    }
                }
            }
            else
            {
                StartPokemonNode otherNodeComponent = Nodes[i].GetComponent<StartPokemonNode>();
                if (otherNodeComponent != null)
                {
                    otherNodeComponent.Select_Cursor.gameObject.SetActive(false);
                }
            }
        }
    }
    public void SetPlayerParty(StartPokemonNode pokemon)
    {
        Sound_Manager.Instance.PlayGUISound("UI/select");
        if (currentPartyIndex < PartyPokemon_Img.Count)
        {
            Pokemon newPokemon = new Pokemon(_base, 5);
            if (PokemonValue + newPokemon.P_Base.Cost > 10)
            {
                Debug.LogWarning("포켓몬 가치를 초과하여 추가할 수 없습니다.");
                return;
            }
            PokemonValue += newPokemon.P_Base.Cost;

            PartyPokemon_Img[currentPartyIndex].sprite = pokemon.PokemonDot.sprite;
            selectedPokemons.Add(newPokemon);
            playerParty.AddPokemon(newPokemon);

            PokemonValue_Text.text = $"{PokemonValue}/10";
            currentPartyIndex++;
        }
        else
        {
            // 파티가 가득 찬 경우
            Debug.LogWarning("PlayerParty가 가득 찼습니다. 더 이상 추가할 수 없습니다.");
        }
    }
    public void RemovePlayerParty()
    {
        if (selectedPokemons.Count > 0)
        {
            // 리스트에서 마지막 포켓몬을 제거
            Pokemon lastSelectedPokemon = selectedPokemons[selectedPokemons.Count - 1];

            playerParty.RemovePokemon(lastSelectedPokemon);
            selectedPokemons.RemoveAt(selectedPokemons.Count - 1);

            PokemonValue -= lastSelectedPokemon.P_Base.Cost;
            PokemonValue_Text.text = $"{PokemonValue}/10";

            // currentPartyIndex가 0 이하로 떨어지지 않도록 제한
            if (currentPartyIndex > 0)
            {
                currentPartyIndex--;
            }

            PartyPokemon_Img[currentPartyIndex].sprite = DefaultParty_Sprite;
        }
    }
    #region SetPokemonSprite
    void SetUpPokemonSprite()
    {
        if (Pokemon_Sprite.sprite != null)
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
    void SetPokemonType(bool isCatch = false)
    {
        if (isCatch)
        {
            if (_base.Type2 == PokemonType.None)
            {
                Sprite[] pokemonTypeSprites = Resources.LoadAll<Sprite>("Image/SkillType");
                if (PokemonTypeImg1 != null)
                {
                    PokemonTypeImg1.gameObject.SetActive(true);
                    PokemonTypeImg2.gameObject.SetActive(false);
                    string onlypokType = _base.Type1.ToString();
                    Sprite typeTargetSprite = System.Array.Find(pokemonTypeSprites, sprite => sprite.name == onlypokType);
                    if (typeTargetSprite != null)
                    {
                        PokemonTypeImg1.sprite = typeTargetSprite;
                    }
                    else
                    {
                        Debug.LogError($"스프라이트를 찾을 수 없습니다");
                        foreach (var sprite in pokemonTypeSprites)
                        {
                            Debug.Log($"스프라이트 이름: {sprite.name}");
                        }
                    }
                }
            }
            else
            {
                if (PokemonTypeImg1 != null && PokemonTypeImg2 != null)
                {
                    Sprite[] pokemonTypeSprites = Resources.LoadAll<Sprite>("Image/SkillType");
                    PokemonTypeImg1.gameObject.SetActive(true);
                    PokemonTypeImg2.gameObject.SetActive(true);
                    string pokType1 = _base.Type1.ToString();
                    string pokType2 = _base.Type2.ToString();
                    Sprite type2TargetSprite1 = System.Array.Find(pokemonTypeSprites, sprite => sprite.name == pokType1);
                    Sprite type2TargetSprite2 = System.Array.Find(pokemonTypeSprites, sprite => sprite.name == pokType2);
                    if (type2TargetSprite1 != null && type2TargetSprite2 != null)
                    {
                        PokemonTypeImg1.sprite = type2TargetSprite1;
                        PokemonTypeImg2.sprite = type2TargetSprite2;
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
            Sprite[] pokemonTypeSprites = Resources.LoadAll<Sprite>("Image/SkillType");
            if (PokemonTypeImg1 != null)
            {
                PokemonTypeImg1.gameObject.SetActive(true);
                PokemonTypeImg2.gameObject.SetActive(false);
                Sprite typeTargetSprite = System.Array.Find(pokemonTypeSprites, sprite => sprite.name == "None");
                if (typeTargetSprite != null)
                {
                    PokemonTypeImg1.sprite = typeTargetSprite;
                }
                else
                {
                    Debug.LogError($"스프라이트를 찾을 수 없습니다");
                    foreach (var sprite in pokemonTypeSprites)
                    {
                        Debug.Log($"스프라이트 이름: {sprite.name}");
                    }
                }
            }
        }
    }
    #endregion
}
