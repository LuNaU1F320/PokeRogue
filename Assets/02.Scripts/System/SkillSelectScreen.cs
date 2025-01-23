using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillSelectScreen : MonoBehaviour
{
    [SerializeField] List<SummaryNode> summaryNodes;

    int currentSelection = 0;
    // public bool SelectionMade { get; private set; } = false; // 선택 완료 여부
    // public int SelectedSkillIndex { get; private set; } = -1; // 선택된 스킬 인덱스

    public void SetSkill(List<SkillBase> currentSkills, SkillBase newSkill)
    {
        for (int i = 0; i < currentSkills.Count; ++i)
        {
            summaryNodes[i].SetSkillData(currentSkills[i]);
        }
        summaryNodes[currentSkills.Count].SetSkillData(newSkill);
    }
    public void HandleSkillSelection(Action<int> onSelected)
    {
        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            currentSelection++;
        }
        else if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            currentSelection--;
        }
        currentSelection = Mathf.Clamp(currentSelection, 0, PokemonBase.MaxNumOfSkills);
        UpdateSkillSelection(currentSelection);

        if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return))
        {
            {

                onSelected?.Invoke(currentSelection);

                /*
                    SelectionMade = true;
                    SelectedSkillIndex = currentSelection;
                    gameObject.SetActive(false); // 선택 화면 비활성화
                */

            }
        }
    }
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