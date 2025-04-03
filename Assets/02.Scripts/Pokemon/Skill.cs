using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Skill
{
    public SkillBase SkillBase { get; set; }
    public int PP { get; set; }

    public Skill(SkillBase pBase)
    {
        SkillBase = pBase;
        PP = pBase.SkillPP;
    }
    public Skill(SkillSaveData saveData)
    {
        // Debug.Log($"[Skill 생성 시도] index: {saveData.skillIdx}, pp: {saveData.pp}");
        SkillBase = SkillDB.GetSkillByIdx(saveData.skillIdx);
        // SkillDB.GetSkillByIdx(saveData.skillIdx);

        PP = saveData.pp;
    }
    public SkillSaveData GetSaveData()
    {
        var saveData = new SkillSaveData()
        {
            skillIdx = SkillBase.SkillIndex,
            pp = PP
        };
        return saveData;
    }
}

[Serializable]
public class SkillSaveData
{
    // public string skillName;
    public int skillIdx;
    public int pp;
}
