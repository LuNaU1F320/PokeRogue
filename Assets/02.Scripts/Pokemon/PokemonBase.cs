using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Pokemon", menuName = "Pokemon/Create New Pokemon")]
public class PokemonBase : ScriptableObject
{
    [SerializeField] int pokemonIndex;
    [SerializeField] string pokemonName;

    [TextArea]
    [SerializeField] string pokemonDescription;

    [SerializeField] PokemonType pokemonType1;
    [SerializeField] PokemonType pokemonType2;

    //Base Stats
    [SerializeField] int maxHp;
    [SerializeField] int attack;
    [SerializeField] int defence;
    [SerializeField] int spAttack;
    [SerializeField] int spDefence;
    [SerializeField] int speed;
    [SerializeField] int expYield;
    [SerializeField] GrowthRate growthRate;

    [SerializeField] int catchRate;

    [SerializeField] List<LearnableSkill> learnableSkills;
    [SerializeField] List<Evolution> evolutions;

    public static int MaxNumOfSkills { get; set; } = 4;
    public int GetExpForLevel(int level)
    {
        if (growthRate == GrowthRate.Erratic)
        {
            if (level <= 50)
            {
                return (level * level * level * (100 - level)) / 50;
            }
            else if (level <= 68)
            {
                return (level * level * level * (150 - level)) / 100;
            }
            else if (level <= 98)
            {
                return (level * level * level * ((1911 - 10 * level) / 3)) / 500;
            }
            else
            {
                return (level * level * level * (160 - level)) / 100;
            }
        }
        else if (growthRate == GrowthRate.Fast)
        {
            return 4 * (level * level * level) / 5;
        }
        else if (growthRate == GrowthRate.MediumFast)
        {
            return level * level * level;
        }
        else if (growthRate == GrowthRate.MediumSlow)
        {
            return (6 * (level * level * level) / 5) - (15 * (level * level)) + (100 * level) - 140;
        }
        else if (growthRate == GrowthRate.Slow)
        {
            return 5 * (level * level * level) / 4;
        }
        else if (growthRate == GrowthRate.Fluctuating)
        {
            if (level <= 15)
            {
                return (level * level * level * (24 + (level + 1))) / 50;
            }
            else if (level <= 35)
            {
                return (level * level * level * (14 + level)) / 50;
            }
            else
            {
                return (level * level * level * (32 + (level / 2))) / 50;
            }
        }

        return -1;
    }

    public int PokemonIndex
    {
        get { return pokemonIndex; }
    }
    public string PokemonName
    {
        get { return pokemonName; }
    }
    public string Description
    {
        get { return pokemonDescription; }
    }
    public PokemonType Type1
    {
        get { return pokemonType1; }
    }
    public PokemonType Type2
    {
        get { return pokemonType2; }
    }
    public int MaxHp
    {
        get { return maxHp; }
    }
    public int Attack
    {
        get { return attack; }
    }
    public int Defence
    {
        get { return defence; }
    }
    public int SpAttack
    {
        get { return spAttack; }
    }
    public int SpDefence
    {
        get { return spDefence; }
    }
    public int Speed
    {
        get { return speed; }
    }
    public int CatchRate
    {
        get { return catchRate; }
    }
    public List<LearnableSkill> LearnableSkills
    {
        get { return learnableSkills; }
    }
    public List<Evolution> Evolutions
    {
        get { return evolutions; }
    }
    public int ExpYield => expYield;
    public GrowthRate GrowthRate => growthRate;
}
[System.Serializable]
public class LearnableSkill
{
    [SerializeField] SkillBase skillBase;
    [SerializeField] int level;
    public LearnableSkill(SkillBase skillBase, int level)
    {
        this.skillBase = skillBase;
        this.level = level;
    }

    public SkillBase SkillBase
    {
        get { return skillBase; }
    }
    public int Level
    {
        get { return level; }
    }
}
[System.Serializable]
public class Evolution
{
    [SerializeField] PokemonBase evolesInto;
    [SerializeField] int requiredLevel;

    public PokemonBase EvolvesInto => evolesInto;
    public int RequiredLevel => requiredLevel;
}
public enum PokemonType
{
    None,
    Normal,
    Fire,
    Water,
    Electric,
    Grass,
    Ice,
    Fighting,
    Poison,
    Ground,
    Rock,
    Flying,
    Psychic,
    Dark,
    Steel,
    Bug,
    Ghost,
    Dragon,
    Fairy
}
public enum Stat
{
    Attack,
    Defence,
    SpAttack,
    SpDefence,
    Speed,
    Accuracy,
    Evasion
}
public enum GrowthRate
{
    Erratic,
    Fast,
    MediumFast,
    MediumSlow,
    Slow,
    Fluctuating
}
public class TypeChart
{
    // 2차원 배열: 공격 타입(행) -> 방어 타입(열)의 상성
    static float[][] chart =
    {
        //                           Nor Fire Wat Ele Gra Ice Fig Poi Gro Roc Fly Psy Dar Ste Bug Gho Dra Fai                           
        /*Normal*/      new float [] {1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 0.5f, 1f, 1f, 1f, 0.5f, 1f, 0f, 1f, 1f},
        /*Fire*/        new float [] {1f, 0.5f, 0.5f, 1f, 2f, 2f, 1f, 1f, 1f, 0.5f, 1f, 1f, 1f, 2f, 2f, 1f, 0.5f, 1f},
        /*Water*/       new float [] {1f, 2f, 0.5f, 1f, 0.5f, 1f, 1f, 1f, 2f, 2f, 1f, 1f, 1f, 1f, 1f, 1f, 0.5f, 1f},
        /*Electric*/    new float [] {1f, 1f, 2f, 0.5f, 0.5f, 1f, 1f, 1f, 0f, 1f, 2f, 1f, 1f, 1f, 1f, 1f, 0.5f, 1f},
        /*Grass*/       new float [] {1f, 0.5f, 2f, 1f, 0.5f, 1f, 1f, 0.5f, 2f, 2f, 0.5f, 1f, 1f, 0.5f, 0.5f, 1f, 0.5f, 1f},
        /*Ice*/         new float [] {1f, 0.5f, 0.5f, 1f, 2f, 0.5f, 1f, 1f, 2f, 1f, 2f, 1f, 1f, 0.5f, 1f, 1f, 2f, 1f},
        /*Fighting*/    new float [] {2f, 1f, 1f, 1f, 1f, 2f, 1f, 0.5f, 1f, 2f, 0.5f, 0.5f, 2f, 2f, 0.5f, 0f, 1f, 0.5f},
        /*Poison*/      new float [] {1f, 1f, 1f, 1f, 2f, 1f, 1f, 0.5f, 0.5f, 0.5f, 1f, 1f, 1f, 0f, 1f, 0.5f, 1f, 2f},
        /*Ground*/      new float [] {1f, 2f, 1f, 2f, 0.5f, 1f, 1f, 2f, 1f, 2f, 0f, 1f, 1f, 1f, 1f, 1f, 1f, 1f},
        /*Rock*/        new float [] {1f, 2f, 1f, 1f, 1f, 2f, 0.5f, 1f, 0.5f, 1f, 2f, 1f, 1f, 0.5f, 1f, 0.5f, 1f, 1f},
        /*Flying*/      new float [] {1f, 1f, 1f, 0.5f, 2f, 1f, 2f, 1f, 1f, 0.5f, 1f, 1f, 1f, 0.5f, 2f, 1f, 1f, 1f},
        /*Psychic*/     new float [] {1f, 1f, 1f, 1f, 1f, 1f, 2f, 2f, 1f, 1f, 1f, 0.5f, 0f, 0.5f, 1f, 1f, 1f, 1f},
        /*Dark*/        new float [] {1f, 1f, 1f, 1f, 1f, 1f, 0.5f, 1f, 1f, 1f, 1f, 2f, 0.5f, 1f, 1f, 2f, 1f, 0.5f},
        /*Steel*/       new float [] {1f, 0.5f, 0.5f, 0.5f, 1f, 2f, 1f, 1f, 1f, 2f, 1f, 1f, 1f, 0.5f, 1f, 1f, 0.5f, 2f},
        /*Bug*/         new float [] {1f, 0.5f, 1f, 1f, 2f, 1f, 0.5f, 0.5f, 1f, 1f, 0.5f, 2f, 1f, 0.5f, 1f, 0.5f, 1f, 0.5f},
        /*Ghost*/       new float [] {0f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 2f, 1f, 1f, 2f, 1f, 1f},
        /*Dragon*/      new float [] {1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 2f, 0f},
        /*Fairy*/       new float [] {1f, 0.5f, 1f, 1f, 1f, 1f, 2f, 0.5f, 1f, 1f, 1f, 1f, 2f, 1f, 1f, 1f, 2f, 1f}
    };

    public static float GetEffectiveness(PokemonType attackType, PokemonType defenseType)
    {
        if (attackType == PokemonType.None || defenseType == PokemonType.None)
        {
            return 1;
        }
        int row = (int)attackType - 1;
        int col = (int)defenseType - 1;

        return chart[row][col];
    }
}
