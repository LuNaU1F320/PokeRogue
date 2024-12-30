using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pokemon
{
    public PokemonBase PokemonBase { get; set; }
    public int PokemonLevel { get; set; }
    public int PokemonHp { get; set; }

    public List<Skill> Skills { get; set; }
    public Pokemon(PokemonBase pbase, int plevel)
    {
        PokemonBase = pbase;
        PokemonLevel = plevel;
        PokemonHp = MaxHp;

        Skills = new List<Skill>();
        foreach (var skill in PokemonBase.LearnableSkills)
        {
            if (skill.Level <= PokemonLevel)
            {
                Skills.Add(new Skill(skill.SkillBase));
            }
            if (Skills.Count >= 4)
            {
                break;
            }
        }

    }
    public int Attack
    {
        get { return Mathf.FloorToInt((PokemonBase.Attack * PokemonLevel) / 100f) + 5; }
    }
    public int Defence
    {
        get { return Mathf.FloorToInt((PokemonBase.Defence * PokemonLevel) / 100f) + 5; }
    }
    public int SpAttack
    {
        get { return Mathf.FloorToInt((PokemonBase.SpAttack * PokemonLevel) / 100f) + 5; }
    }
    public int SpDefence
    {
        get { return Mathf.FloorToInt((PokemonBase.SpDefence * PokemonLevel) / 100f) + 5; }
    }
    public int Speed
    {
        get { return Mathf.FloorToInt((PokemonBase.Speed * PokemonLevel) / 100f) + 5; }
    }
    public int MaxHp
    {
        get { return Mathf.FloorToInt((PokemonBase.MaxHp * PokemonLevel) / 100f) + 10; }
    }
    public bool TakeDamage(Skill skill, Pokemon attacker)
    {
        float modifiers = Random.Range(0.85f, 1.0f);
        float a = (2 * attacker.PokemonLevel + 10) / 250.0f;
        float d = a * skill.SkillBase.SkillPower * ((float)attacker.Attack / Defence) + 2;
        int damage = Mathf.FloorToInt(d * modifiers);

        PokemonHp -= damage;
        if (PokemonHp <= 0)
        {
            PokemonHp = 0;
            return true;
        }
        return false;
    }
    public Skill GetRandomSkill()
    {
        int r = Random.Range(0, Skills.Count);
        return Skills[r];
    }
    public PokemonType Type1
    {
        get { return PokemonBase.Type1; }
    }
    public PokemonType Type2
    {
        get { return PokemonBase.Type2; }
    }
}
