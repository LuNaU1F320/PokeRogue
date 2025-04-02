using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillDB
{
    static Dictionary<int, SkillBase> skills;

    public static void Init()
    {
        if (skills != null && skills.Count > 0)
        {
            Debug.Log("[SkillDB] 이미 초기화됨. 생략");
            return;
        }
        skills = new Dictionary<int, SkillBase>();
        var skillList = Resources.LoadAll<SkillBase>("");

        foreach (var skill in skillList)
        {
            // Debug.Log($"[SkillDB] 등록됨: '{skill.SkillName}' (파일 이름: {skill.name})");
            if (skills.ContainsKey(skill.SkillIndex))
            {
                Debug.Log($"겹치는 스킬 존재{skill.SkillName}");
                continue;
            }
            skills[skill.SkillIndex] = skill;
        }
        // foreach (var skill in skills.Values)
        // {
        //     Debug.Log($"[SkillDB] 등록된 스킬: {skill.SkillName} (index: {skill.SkillIndex})");
        // }


    }
    // public static SkillBase GetSkillByName(string skillName)
    // {
    //     if (skills.ContainsKey(skillName) == false)
    //     {
    //         Debug.Log($"{skillName} 스킬 찾을 수 없음");
    //         return null;
    //     }
    //     return skills[skillName];
    // }
    public static SkillBase GetSkillByIdx(int skillIdx)
    {
        if (skills.ContainsKey(skillIdx) == false)
        {
            Debug.Log($"{skillIdx} 스킬 찾을 수 없음");
            return null;
        }
        return skills[skillIdx];
    }
}
