using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillSelectScreen : MonoBehaviour
{
    [SerializeField] List<SummaryNode> summaryNodes;

    int currentSelection = 0;

    public void SetSkill(List<SkillBase> currentSkills, SkillBase newSkill)
    {
        for (int i = 0; i < currentSkills.Count; ++i)
        {
            summaryNodes[i].SetSkillData(currentSkills[i]);
        }
        summaryNodes[currentSkills.Count].SetSkillData(newSkill);
    }
    public void HandleSkillSelection()
    {
        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            currentSelection++;
        }
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            currentSelection--;
        }
        currentSelection = Mathf.Clamp(currentSelection, 0, PokemonBase.MaxNumOfSkills);
    }
    public void UpdateSkillSelection(int selection)
    {
        for (int i = 0; i < PokemonBase.MaxNumOfSkills + 1; i++)
        {
            if (i == selection)
            {

            }
        }
    }
}
