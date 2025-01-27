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
}
