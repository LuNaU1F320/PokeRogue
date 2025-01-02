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