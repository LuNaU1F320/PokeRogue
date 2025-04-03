using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillDB
{
    static Dictionary<int, SkillBase> skills;

    public static void Init()
    {
        skills = new Dictionary<int, SkillBase>();
        var skillList = Resources.LoadAll<SkillBase>("Skills");

        foreach (var skill in skillList)
        {
            if (skills.ContainsKey(skill.SkillIndex))
            {
                Debug.Log($"겹치는 스킬 존재{skill.SkillName}");
                continue;
            }
            skills[skill.SkillIndex] = skill;
        }
    }

    public static SkillBase GetSkillByIdx(int skillIdx)
    {
        Init();

        if (skills.ContainsKey(skillIdx) == false)
        {
            Debug.Log($"{skillIdx} 스킬 찾을 수 없음");
            return null;
        }
        // foreach (var kvp in skills)
        // {
        //     if (kvp.Value == null)
        //     {
        //         Debug.LogError($"[SkillDB] key {kvp.Key} → 값이 null임!");
        //     }
        // }
        // if (skills.TryGetValue(skillIdx, out var skillBase))
        // {
        //     if (skillBase == null)
        //     {
        //         Debug.LogError($"[SkillDB] index {skillIdx}는 등록돼 있으나 값이 null이에요!");
        //     }
        //     else
        //     {
        //         Debug.Log($"[SkillDB] index {skillIdx} = {skillBase.SkillName}");
        //     }

        //     return skillBase;
        // }
        // else
        // {
        //     Debug.LogError($"[SkillDB] index {skillIdx}가 등록되지 않았어요!");
        //     return null;
        // }

        return skills[skillIdx];
    }
}
