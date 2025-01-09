using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Pokemon
{
    [SerializeField] PokemonBase _base;
    [SerializeField] int level;

    public PokemonBase PokemonBase
    {
        get
        {
            return _base;
        }
    }
    public int PokemonLevel
    {
        get
        {
            return level;
        }
    }
    public int PokemonHp { get; set; }

    public List<Skill> Skills { get; set; }
    public Dictionary<Stat, int> Stats { get; private set; }
    public Dictionary<Stat, int> Rankup { get; private set; }
    public void Init()
    {
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
        CalculateStats();
        PokemonHp = MaxHp;
        Rankup = new Dictionary<Stat, int>()
        {
            {Stat.Attack , 0},
            {Stat.Defence , 0},
            {Stat.SpAttack , 0},
            {Stat.SpDefence , 0},
            {Stat.Speed , 0}
        };
    }
    void CalculateStats()
    {
        Stats = new Dictionary<Stat, int>
        {
            { Stat.Attack, Mathf.FloorToInt((PokemonBase.Attack * PokemonLevel) / 100f) + 5 },
            { Stat.Defence, Mathf.FloorToInt((PokemonBase.Defence * PokemonLevel) / 100f) + 5 },
            { Stat.SpAttack, Mathf.FloorToInt((PokemonBase.SpAttack * PokemonLevel) / 100f) + 5 },
            { Stat.SpDefence, Mathf.FloorToInt((PokemonBase.SpDefence * PokemonLevel) / 100f) + 5 },
            { Stat.Speed, Mathf.FloorToInt((PokemonBase.Speed * PokemonLevel) / 100f) + 5 }
        };

        MaxHp = Mathf.FloorToInt((PokemonBase.MaxHp * PokemonLevel) / 100f) + 10;
    }
    int GetStat(Stat stat)
    {
        int statVal = Stats[stat];

        //랭크업
        int rank = Rankup[stat];
        var rankValues = new float[] { 1f, 1.5f, 2f, 2.5f, 3f, 3.5f, 4f };

        if (rank >= 0)
        {
            statVal = Mathf.FloorToInt(statVal * rankValues[rank]);
        }
        else
        {
            statVal = Mathf.FloorToInt(statVal * rankValues[-rank]);
        }

        return statVal;
    }
    public void ApplyRankups(List<Rankup> rankUps)
    {
        foreach (var rankUp in rankUps)
        {
            var stat = rankUp.stat;
            var rank = rankUp.rank;

            Rankup[stat] = Math.Clamp(Rankup[stat] + rank, -6, 6);
        }
    }

    public int Attack
    {
        get { return GetStat(Stat.Attack); }
    }
    public int Defence
    {
        get { return GetStat(Stat.Defence); }
    }
    public int SpAttack
    {
        get { return GetStat(Stat.SpAttack); }
    }
    public int SpDefence
    {
        get { return GetStat(Stat.SpDefence); }
    }
    public int Speed
    {
        get { return GetStat(Stat.Speed); }
    }
    public int MaxHp
    {
        get; private set;
    }
    public (int startHp, int endHp, DamageDetails damageDetails) TakeDamage(Skill skill, Pokemon attacker)
    {
        if (skill.SkillBase.CategoryKey == CategoryKey.Status)
        {
            // 변화기 처리
            return (PokemonHp, PokemonHp, new DamageDetails { TypeEffectiveness = 1f, Critical = 1f, Fainted = false });
        }
        float critical = 1f;        //급소
        if (UnityEngine.Random.value * 100f < 6.25f)
        {
            critical = 2f;
        }
        float typeDmgMag = TypeChart.GetEffectiveness(skill.SkillBase.SkillType, this.PokemonBase.Type1) * TypeChart.GetEffectiveness(skill.SkillBase.SkillType, this.PokemonBase.Type2);       //타입별 데미지 배율

        var damageDetails = new DamageDetails()
        {
            TypeEffectiveness = typeDmgMag,
            Critical = critical,
            Fainted = false
        };
        //물리특수 처리
        float attack;
        if (skill.SkillBase.CategoryKey == CategoryKey.Special)
        {
            attack = attacker.SpAttack;
        }
        else if (skill.SkillBase.CategoryKey == CategoryKey.Physical)
        {
            attack = attacker.Attack;
        }
        else
        {
            attack = 0;
        }
        float defence;
        if (skill.SkillBase.CategoryKey == CategoryKey.Special)
        {
            defence = SpDefence;
        }
        else if (skill.SkillBase.CategoryKey == CategoryKey.Physical)
        {
            defence = Defence;
        }
        else
        {
            defence = Defence;
        }

        //float attack = (skill.SkillBase.IsSpecial) ? attacker.SpAttack : attacker.Attack;
        float modifiers = UnityEngine.Random.Range(0.85f, 1.0f) * typeDmgMag * critical;
        float a = (2 * attacker.PokemonLevel + 10) / 250.0f;
        float d = a * skill.SkillBase.SkillPower * ((float)attack / defence) + 2;
        int damage = Mathf.FloorToInt(d * modifiers);
        // Debug.Log(damage);
        int startHp = PokemonHp;
        PokemonHp -= damage;
        if (PokemonHp <= 0)
        {
            PokemonHp = 0;
            damageDetails.Fainted = true;
            return (startHp, 0, damageDetails);
        }
        return (startHp, PokemonHp, damageDetails);
    }
    public Skill GetRandomSkill()
    {
        int r = UnityEngine.Random.Range(0, Skills.Count);
        return Skills[r];
    }
    public class DamageDetails
    {
        public bool Fainted { get; set; }
        public float Critical { get; set; }
        public float TypeEffectiveness { get; set; }
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
