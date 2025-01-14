using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Skill", menuName = "Pokemon/Create new Skill")]
public class SkillBase : ScriptableObject
{
    [SerializeField] int skillIndex;
    [SerializeField] string skillName;

    [TextArea]
    [SerializeField] string skillDescription;

    [SerializeField] PokemonType skillType;
    [SerializeField] CategoryKey categoryKey;

    [SerializeField] int skillPower;
    [SerializeField] int skillAccuracy;
    [SerializeField] bool alwaysHits;
    [SerializeField] int skillPP;
    [SerializeField] int priority;
    [SerializeField] SkillEffects effects;
    [SerializeField] List<SecondaryEffects> secondaryEffects;
    [SerializeField] SkillTarget target;

    public string SkillName
    {
        get { return skillName; }
    }
    public string SkillDescription
    {
        get { return skillDescription; }
    }
    public PokemonType SkillType
    {
        get { return skillType; }
    }
    public CategoryKey CategoryKey
    {
        get { return categoryKey; }
    }
    public SkillEffects Effects
    {
        get { return effects; }
    }
    public List<SecondaryEffects> SecondaryEffects
    {
        get { return secondaryEffects; }
    }
    public SkillTarget Target
    {
        get { return target; }
    }
    public int SkillPower
    {
        get { return skillPower; }
    }
    public int SkillAccuracy
    {
        get { return skillAccuracy; }
    }
    public bool AlwaysHits
    {
        get { return alwaysHits; }
    }
    public int SkillPP
    {
        get { return skillPP; }
    }
    public int Priority
    {
        get { return priority; }
    }
}

[System.Serializable]
public class SkillEffects
{
    [SerializeField] List<Rankup> rankup;
    [SerializeField] ConditionID status;
    [SerializeField] ConditionID volatileStatus;

    public List<Rankup> Rankup
    {
        get { return rankup; }
    }

    public ConditionID Status
    {
        get { return status; }
    }
    public ConditionID VolatileStatus
    {
        get { return volatileStatus; }
    }


}
[System.Serializable]
public class SecondaryEffects : SkillEffects
{
    [SerializeField] int chance;
    [SerializeField] SkillTarget target;
    public int Chance
    {
        get { return chance; }
    }
    public SkillTarget Target
    {
        get { return target; }
    }
}

[System.Serializable]
public class Rankup
{
    public Stat stat;
    public int rank;
}
public enum CategoryKey
{
    Physical,
    Special,
    Status
}
public enum SkillTarget
{
    Foe,
    Self
}