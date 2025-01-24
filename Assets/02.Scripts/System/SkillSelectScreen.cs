
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SkillSelectScreen : MonoBehaviour
{
    [SerializeField] List<SummaryNode> summaryNodes;
    // [SerializeField] BattleUnit playerPokemon;
    [SerializeField] public Text PokemonIdx;
    // [SerializeField] Image PokemonSprite;
    [SerializeField] SpriteRenderer PokemonSprite;

    int currentSelection = 0;

    public void SetPokemonData(Pokemon pokemon)
    {
        PokemonIdx.text = pokemon.P_Base.PokemonIndex.ToString();
        PokemonSprite.sprite = Resources.Load<Sprite>($"Image/Pokemon/PokemonDot/{pokemon.PokemonGen}/{pokemon.P_Base.PokemonIndex}");
        // PokemonSprite = playerPokemon.spriteRenderer;
    }
    public void SetSkill(List<SkillBase> currentSkills, SkillBase newSkill)
    {
        for (int i = 0; i < currentSkills.Count; ++i)
        {
            summaryNodes[i].SetSkillData(currentSkills[i]);
        }
        summaryNodes[currentSkills.Count].SetSkillData(newSkill);
    }
    // public void HandleSkillSelection(int onSelected)
    // {
    //     if (Input.GetKeyDown(KeyCode.DownArrow))
    //     {
    //         currentSelection++;
    //     }
    //     else if (Input.GetKeyDown(KeyCode.UpArrow))
    //     {
    //         currentSelection--;
    //     }
    //     currentSelection = Mathf.Clamp(currentSelection, 0, PokemonBase.MaxNumOfSkills);
    //     UpdateSkillSelection(currentSelection);

    //     if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return))
    //     {
    //         {

    //             // onSelected?.Invoke(currentSelection);

    //             gameObject.SetActive(false); // 선택 화면 비활성화


    //         }
    //     }
    // }
    public void UpdateSkillSelection(int selection)
    {
        for (int i = 0; i < PokemonBase.MaxNumOfSkills + 1; i++)
        {
            if (i == selection)
            {
                summaryNodes[i].Select_Img.gameObject.SetActive(true);
                summaryNodes[i].SkillDescription.gameObject.SetActive(true);
            }
            else
            {
                summaryNodes[i].Select_Img.gameObject.SetActive(false);
                summaryNodes[i].SkillDescription.gameObject.SetActive(false);
            }
        }
    }
}