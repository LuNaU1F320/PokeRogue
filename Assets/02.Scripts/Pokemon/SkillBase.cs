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
    [SerializeField] int skillAccuary;
    [SerializeField] int skillPP;
    [SerializeField] SkillEffects effects;
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
    public SkillTarget Target
    {
        get { return target; }
    }
    public int SkillPower
    {
        get { return skillPower; }
    }
    public int SkillAccuary
    {
        get { return skillAccuary; }
    }
    public int SkillPP
    {
        get { return skillPP; }
    }
}
[System.Serializable]
public class SkillEffects
{
    [SerializeField] List<Rankup> rankup;

    public List<Rankup> Rankup
    {
        get { return rankup; }
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