using System.Collections;
using System.Collections.Generic;
using System;
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
    [SerializeField] List<Text> configTexts;

    [SerializeField] Image typeImg;
    [SerializeField] Image categoryKeyImg;
    [SerializeField] Text ppText;
    [SerializeField] Text skillDetailText;
    bool isTyping = false;
    private Queue<string> dialogQueue = new Queue<string>();
    private bool isRunningDialog = false;

    // public IEnumerator TypeDialog(string dialog)
    // {
    //     dialogText.text = "";
    //     isTyping = true;

    //     foreach (var letter in dialog.ToCharArray())
    //     {
    //         dialogText.text += letter;
    //         yield return new WaitForSecondsRealtime(1f / lettersPerSecond);
    //     }

    //     isTyping = false;
    //     dialogText.text += " ▼";

    //     // // 입력 대기
    //     yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.Space));
    // }
    // public IEnumerator TypeDialog(string dialog)
    // {
    //     dialogText.text = "";
    //     isTyping = true;

    //     // 여러 줄 처리: \n로 나눠서 한 줄씩 출력 후 Space 대기
    //     var lines = dialog.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);

    //     for (int i = 0; i < lines.Length; i++)
    //     {
    //         dialogText.text = "";
    //         foreach (var letter in lines[i])
    //         {
    //             dialogText.text += letter;
    //             yield return new WaitForSecondsRealtime(1f / lettersPerSecond);
    //         }

    //         isTyping = false;
    //         dialogText.text += " ▼";
    //         yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.Space));
    //     }
    // }
    public IEnumerator TypeDialog(string dialog)
    {
        dialogQueue.Enqueue(dialog);

        if (!isRunningDialog)
        {
            isRunningDialog = true;
            while (dialogQueue.Count > 0)
            {
                string nextDialog = dialogQueue.Dequeue();
                dialogText.text = "";
                isTyping = true;

                foreach (var letter in nextDialog.ToCharArray())
                {
                    dialogText.text += letter;
                    yield return new WaitForSeconds(1f / lettersPerSecond);
                }

                isTyping = false;
                dialogText.text += " ▼";

                yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.Space));
            }
            isRunningDialog = false;
        }
    }
    // public IEnumerator TypeDialog(string dialog)
    // {
    //     dialogQueue.Enqueue(dialog);

    //     if (!isRunningDialog)
    //     {
    //         isRunningDialog = true;

    //         while (dialogQueue.Count > 0)
    //         {
    //             string nextDialog = dialogQueue.Dequeue();
    //             dialogText.text = "";
    //             isTyping = true;

    //             bool skip = false;

    //             // 타이핑 하나하나
    //             foreach (var letter in nextDialog)
    //             {
    //                 if (Input.GetKeyDown(KeyCode.Space))
    //                 {
    //                     skip = true;
    //                     break;
    //                 }

    //                 dialogText.text += letter;
    //                 yield return new WaitForSecondsRealtime(1f / lettersPerSecond);
    //             }

    //             // skip했으면 나머지 글자 그냥 붙이기
    //             if (skip)
    //             {
    //                 dialogText.text = nextDialog;
    //             }

    //             isTyping = false;
    //             dialogText.text += " ▼";

    //             // ✅ 여기서 반드시 다시 스페이스 입력을 기다려야 함
    //             yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.Space));
    //         }

    //         isRunningDialog = false;
    //     }
    // }
    public IEnumerator TypeDialogMulti(string[] dialogs)
    {
        foreach (string dialog in dialogs)
        {
            dialogText.text = "";
            isTyping = true;

            foreach (var letter in dialog.ToCharArray())
            {
                dialogText.text += letter;
                yield return new WaitForSecondsRealtime(1f / lettersPerSecond);
            }

            isTyping = false;
            dialogText.text += " ▼";

            yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.Space));
        }
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
