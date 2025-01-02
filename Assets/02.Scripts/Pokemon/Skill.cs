using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Skill
{
    public SkillBase SkillBase { get; set; }
    public int SkillPP { get; set; }

    public Skill(SkillBase pBase)
    {
        SkillBase = pBase;
        SkillPP = pBase.SkillPP;
    }
}
