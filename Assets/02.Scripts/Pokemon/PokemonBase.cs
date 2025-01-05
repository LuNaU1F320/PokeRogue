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

    [SerializeField] Sprite frontSprite;
    [SerializeField] Sprite backSprite;

    [SerializeField] PokemonType pokemonType1;
    [SerializeField] PokemonType pokemonType2;

    //Base Stats
    [SerializeField] int maxHp;
    [SerializeField] int attack;
    [SerializeField] int defence;
    [SerializeField] int spAttack;
    [SerializeField] int spDefence;
    [SerializeField] int speed;
    [SerializeField] List<LearnableSkill> learnableSkills;

    public string PokemonName
    {
        get { return pokemonName; }
    }
    public string Description
    {
        get { return pokemonDescription; }
    }
    public Sprite FrontSprite
    {
        get { return frontSprite; }
    }
    public Sprite BackSprite
    {
        get { return backSprite; }
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
    public List<LearnableSkill> LearnableSkills
    {
        get { return learnableSkills; }
    }
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
