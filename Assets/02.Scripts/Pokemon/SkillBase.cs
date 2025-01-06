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
    /*
    public bool IsSpecial
    {
        get
        {
            if (skillType == PokemonType.Fire || skillType == PokemonType.Water)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }*/
}
public enum CategoryKey
{
    Physical,
    Special,
    Status
}