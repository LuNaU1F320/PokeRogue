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
        SkillDB.GetSkillByName(saveData.skillName);
        PP = saveData.pp;
    }
    public SkillSaveData GetSaveData()
    {
        var saveData = new SkillSaveData()
        {
            skillName = SkillBase.SkillName,
            pp = PP
        };
        return saveData;
    }
}

[Serializable]
public class SkillSaveData
{
    public string skillName;
    public int pp;
}
