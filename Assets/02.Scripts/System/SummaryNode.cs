using UnityEngine;
using UnityEngine.UI;

public class SummaryNode : MonoBehaviour
{
    [SerializeField] public Image Select_Img;
    [SerializeField] Image SkillType_Img;
    [SerializeField] Text SkillName_Text;
    [SerializeField] Text SkillPP_Text;
    [SerializeField] Text SkillDesc_Text;
    [SerializeField] Text SkillPower_Text;
    [SerializeField] Text SkillAcc_Text;
    [SerializeField] Image SkillCategory_Img;

    public void SetSkillData(SkillBase skill)
    {
        Sprite[] skillTypeSprites = Resources.LoadAll<Sprite>("Image/SkillType");
        string skillTypeName = skill.SkillType.ToString();
        Sprite typeTargetSprite = System.Array.Find(skillTypeSprites, sprite => sprite.name == skillTypeName);

        if (typeTargetSprite != null)
        {
            SkillType_Img.sprite = typeTargetSprite;
        }
        else
        {
            Debug.LogError($"스프라이트를 찾을 수 없습니다: {skillTypeName}");
        }
        SkillName_Text.text = skill.SkillName;
        SkillPP_Text.text = "PP " + skill.SkillPP.ToString();
        SkillDesc_Text.text = skill.SkillDescription;
        SkillPower_Text.text = $"{(skill.SkillPower == 0 ? "-" : skill.SkillPower.ToString())}";
        SkillAcc_Text.text = $"{(skill.SkillAccuracy == 0 ? "-" : skill.SkillAccuracy.ToString())}";
        Sprite[] categorySprites = Resources.LoadAll<Sprite>("Image/CategoryKey");
        string categoryName = skill.CategoryKey.ToString();
        Sprite catTargetSprite = System.Array.Find(categorySprites, sprite => sprite.name == categoryName);

        if (catTargetSprite != null)
        {
            SkillCategory_Img.sprite = catTargetSprite;
        }
        else
        {
            Debug.LogError($"스프라이트를 찾을 수 없습니다: {categoryName}");
        }
    }
}
