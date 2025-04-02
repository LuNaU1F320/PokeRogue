using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillDB
{
    static Dictionary<string, SkillBase> skills;

    public static void Init()
    {
        skills = new Dictionary<string, SkillBase>();
        var skillList = Resources.LoadAll<SkillBase>("");

        foreach (var skill in skillList)
        {
            // Debug.Log($"[SkillDB] 등록됨: '{skill.SkillName}' (파일 이름: {skill.name})");
            if (skills.ContainsKey(skill.SkillName))
            {
                Debug.Log($"겹치는 스킬 존재{skill.SkillName}");
                continue;
            }
            skills[skill.SkillName] = skill;
        }
    }
    public static SkillBase GetSkillByName(string skillName)
    {
        if (skills.ContainsKey(skillName) == false)
        {
            Debug.Log($"{skillName} 스킬 찾을 수 없음");
            return null;
        }
        return skills[skillName];
    }
    public static SkillBase GetSkillByIdx(string skillIdx)
    {
        if (skills.ContainsKey(skillIdx) == false)
        {
            Debug.Log($"{skillIdx} 스킬 찾을 수 없음");
            return null;
        }
        return skills[skillIdx];
    }

}
