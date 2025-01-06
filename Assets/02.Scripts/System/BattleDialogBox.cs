using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BattleDialogBox : MonoBehaviour
{
    [SerializeField] Text dialogText;
    [SerializeField] Color highlightedColor;
    [SerializeField] int lettersPerSecond;

    private Coroutine typingCoroutine; // 현재 실행 중인 코루틴
    private bool isTyping = false;    // 텍스트 출력 상태 플래그


    [SerializeField] GameObject skillSelector;
    [SerializeField] GameObject actionSelector;
    [SerializeField] GameObject skillDetail;

    [SerializeField] List<Text> skillTexts;
    [SerializeField] List<Text> actionTexts;

    [SerializeField] Image typeImg;
    [SerializeField] Image categoryKeyImg;

    // [SerializeField] Text typeText;
    // [SerializeField] Text categoryKeyText;
    [SerializeField] Text ppText;
    [SerializeField] Text skillDetailText;

    public void SetDialog(string dialog)
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine); // 기존 코루틴 중단
        }
        dialogText.text = dialog;
    }
    public IEnumerator TypeDialog(string dialog)
    {
        isTyping = true; // 텍스트 출력 시작
        dialogText.text = "";

        foreach (var letter in dialog.ToCharArray())
        {
            dialogText.text += letter;
            yield return new WaitForSeconds(1f / lettersPerSecond);
        }
        yield return new WaitForSeconds(1.0f);

        isTyping = false; // 텍스트 출력 완료
    }
    public bool IsTyping()
    {
        return isTyping;
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
        ppText.text = $"{skill.SkillPP}/{skill.SkillBase.SkillPP}";

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
            foreach (var sprite in skillTypeSprites)
            {
                Debug.Log($"스프라이트 이름: {sprite.name}");
            }
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
            foreach (var sprite in categorySprites)
            {
                Debug.Log($"스프라이트 이름: {sprite.name}");
            }
        }
        skillDetailText.text = $"{(skill.SkillBase.SkillPower == 0 ? "-" : skill.SkillBase.SkillPower.ToString())}\n{(skill.SkillBase.SkillAccuary == 0 ? "-" : skill.SkillBase.SkillAccuary.ToString())}";
    }
    /*
    // public void UpdateSkillSelection(int selectedSkill, Skill skill)
    // {
    //     for (int i = 0; i < skillTexts.Count; i++)
    //     {
    //         if (i == selectedSkill)
    //         {
    //             skillTexts[i].color = highlightedColor;
    //         }
    //         else
    //         {
    //             skillTexts[i].color = Color.white;
    //         }
    //     }

    //     ppText.text = $"{skill.SkillPP}/{skill.SkillBase.SkillPP}";
    //     // typeText.text = skill.SkillBase.SkillType.ToString();

    //     Sprite[] sprites = Resources.LoadAll<Sprite>("Image/CategoryKey");

    //     // CategoryKey에 해당하는 이름으로 스프라이트 검색
    //     string categoryName = skill.SkillBase.CategoryKey.ToString();
    //     Sprite targetSprite = System.Array.Find(sprites, sprite => sprite.name == categoryName);

    //     if (targetSprite != null)
    //     {
    //         typeImg.sprite = targetSprite;
    //     }
    //     else
    //     {
    //         Debug.LogError($"스프라이트를 찾을 수 없습니다: {categoryName}");
    //     }

    //     // typeImg.sprite = Resources.Load<Sprite>($"Image/{skill.SkillBase.CategoryKey.ToString()}");
    //     skillDetailText.text = $"{skill.SkillBase.SkillPower}\n{skill.SkillBase.SkillAccuary}";
    // }
    */

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
}
