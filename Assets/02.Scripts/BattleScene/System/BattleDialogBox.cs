using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BattleDialogBox : MonoBehaviour
{
    [SerializeField] Text dialogText;
    [SerializeField] Color highlightedColor;
    [SerializeField] int lettersPerSecond;

    [SerializeField] GameObject skillSelector;
    [SerializeField] GameObject actionSelector;
    [SerializeField] GameObject skillDetail;

    [SerializeField] List<Text> skillTexts;
    [SerializeField] List<Text> actionTexts;
    [SerializeField] List<Text> confirmTexts;

    [SerializeField] Image typeImg;
    [SerializeField] Image categoryKeyImg;
    [SerializeField] Text ppText;
    [SerializeField] Text skillDetailText;

    public void SetDialog(string dialog)
    {
        dialogText.text = dialog;
    }
    public IEnumerator TypeDialog(string dialog)
    {
        dialogText.text = string.Empty;

        foreach (var letter in dialog.ToCharArray())
        {
            dialogText.text += letter;
            yield return new WaitForSeconds(1f / lettersPerSecond);
        }
        yield return new WaitForSeconds(1.0f);

        dialogText.text = string.Empty;
    }
    public void EnableDialogText(bool enabled)
    {
        dialogText.enabled = enabled;
    }
    public void EnableSkillSelector(bool enabled)
    {
        skillSelector.SetActive(enabled);
        skillDetail.SetActive(enabled);
    }
    public void EnableActionSelector(bool enabled)
    {
        actionSelector.SetActive(enabled);
    }
    public void UpdateActionSelection(int selectedAction)
    {
        for (int i = 0; i < actionTexts.Count; i++)
        {
            if (i == selectedAction)
            {
                actionTexts[i].color = highlightedColor;
            }
            else
            {
                actionTexts[i].color = Color.white;
            }
        }
    }
    public void UpdateSkillSelection(int selectedSkill, Skill skill)
    {
        // 선택된 스킬 강조 표시
        for (int i = 0; i < skillTexts.Count; i++)
        {
            if (i == selectedSkill)
            {
                skillTexts[i].color = highlightedColor;
            }
            else
            {
                skillTexts[i].color = Color.white;
            }
        }

        // PP 텍스트 설정
        ppText.text = $"{skill.PP}/{skill.SkillBase.SkillPP}";

        if (skill.PP == 0)
        {
            ppText.color = Color.red;
        }
        else
        {
            ppText.color = Color.white;
        }

        Sprite[] skillTypeSprites = Resources.LoadAll<Sprite>("Image/SkillType");
        string skillTypeName = skill.SkillBase.SkillType.ToString();
        Sprite typeTargetSprite = System.Array.Find(skillTypeSprites, sprite => sprite.name == skillTypeName);

        if (typeTargetSprite != null)
        {
            typeImg.sprite = typeTargetSprite;
        }
        else
        {
            Debug.LogError($"스프라이트를 찾을 수 없습니다: {skillTypeName}");
        }

        // 공격타입 설정
        Sprite[] categorySprites = Resources.LoadAll<Sprite>("Image/CategoryKey");
        string categoryName = skill.SkillBase.CategoryKey.ToString();
        Sprite catTargetSprite = System.Array.Find(categorySprites, sprite => sprite.name == categoryName);

        if (catTargetSprite != null)
        {
            categoryKeyImg.sprite = catTargetSprite;
        }
        else
        {
            Debug.LogError($"스프라이트를 찾을 수 없습니다: {categoryName}");
        }
        skillDetailText.text = $"{(skill.SkillBase.SkillPower == 0 ? "-" : skill.SkillBase.SkillPower.ToString())}\n{(skill.SkillBase.SkillAccuracy == 0 ? "-" : skill.SkillBase.SkillAccuracy.ToString())}";
    }

    public void SetSkillNames(List<Skill> skills)
    {
        for (int i = 0; i < skillTexts.Count; i++)
        {
            if (i < skills.Count)
            {
                skillTexts[i].text = skills[i].SkillBase.SkillName;
            }
            else
            {
                skillTexts[i].text = "-";
            }
        }
    }
    public void ConfirmBoxSelection(int selectedAction)
    {
        for (int i = 0; i < confirmTexts.Count; i++)
        {
            if (i == selectedAction)
            {
                confirmTexts[i].color = highlightedColor;
            }
            else
            {
                confirmTexts[i].color = Color.white;
            }
        }
    }
}
