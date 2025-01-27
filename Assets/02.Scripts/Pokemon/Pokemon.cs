using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public class Pokemon
{
    [SerializeField] PokemonBase _base;
    [SerializeField] int level;
    int pokemonGen;
    [SerializeField] bool isShiny;

    public PokemonBase P_Base
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
    public int PokemonGen
    {
        get
        {
            return pokemonGen;
        }
    }
    public int PokemonExp { get; set; }
    public int PokemonHp { get; set; }
    public int startHp { get; set; }
    public List<Skill> Skills { get; set; }
    public Skill CurrentSkill { get; set; }
    public Dictionary<Stat, int> Stats { get; private set; }
    public Dictionary<Stat, int> Rankup { get; private set; }
    public PokemonCondition Status { get; private set; }
    public int StatusTime { get; set; }
    public PokemonCondition VolatileStatus { get; private set; }
    public int VolatileStatusTime { get; set; }
    public Queue<string> StatusCngMsg { get; private set; }
    public bool IsHpChanged { get; set; }
    public event System.Action OnStatusChanged;

    public Pokemon(PokemonBase pBase, int pLevel)
    {
        _base = pBase;
        level = pLevel;

        Init();
    }
    public void Init()
    {
        Skills = new List<Skill>();
        foreach (var skill in P_Base.LearnableSkills)
        {
            if (skill.Level <= PokemonLevel)
            {
                Skills.Add(new Skill(skill.SkillBase));
            }
            if (Skills.Count >= PokemonBase.MaxNumOfSkills)
            {
                break;
            }
        }
        PokemonExp = P_Base.GetExpForLevel(PokemonLevel);
        CalculateStats();
        PokemonHp = MaxHp;

        SetGeneration();

        StatusCngMsg = new Queue<string>();
        ResetRankup();
        Status = null;
        VolatileStatus = null;
    }
    public Pokemon(PokemonSaveData saveData)
    {
        _base = PokemonDB.GetPokemonByIndex(saveData.index);
        PokemonHp = saveData.hp;
        level = saveData.level;
        PokemonExp = saveData.exp;
        if (saveData.statusID != null)
        {
            Status = ConditionsDB.Conditions[saveData.statusID.Value];
        }
        else
        {
            Status = null;
        }

        Skills = saveData.skills.Select(s => new Skill(s)).ToList();

        CalculateStats();
        StatusCngMsg = new Queue<string>();
        ResetRankup();
        VolatileStatus = null;
    }
    public PokemonSaveData GetSaveData()
    {
        var saveData = new PokemonSaveData()
        {
            index = P_Base.PokemonIndex,
            hp = PokemonHp,
            level = PokemonLevel,
            exp = PokemonExp,
            statusID = Status?.Id,
            skills = Skills.Select(s => s.GetSaveData()).ToList()
        };
        return saveData;
    }
    private void SetGeneration()
    {
        int index = _base.PokemonIndex;
        if (index <= 151) // 1세대: 1 ~ 151
        {
            pokemonGen = 1;
        }
        else if (index <= 251) // 2세대: 152 ~ 251
        {
            pokemonGen = 2;
        }
        else if (index <= 386) // 3세대: 252 ~ 386
        {
            pokemonGen = 3;
        }
        else if (index <= 493) // 4세대: 387 ~ 493
        {
            pokemonGen = 4;
        }
        else if (index <= 649) // 5세대: 494 ~ 649
        {
            pokemonGen = 5;
        }
        else if (index <= 721) // 6세대: 650 ~ 721
        {
            pokemonGen = 6;
        }
        else if (index <= 809) // 7세대: 722 ~ 809
        {
            pokemonGen = 7;
        }
        else if (index <= 898) // 8세대: 810 ~ 898
        {
            pokemonGen = 8;
        }
        else // 9세대 이후 (예시로 899 이상)
        {
            pokemonGen = 9;
        }
    }
    void CalculateStats()
    {
        Stats = new Dictionary<Stat, int>
        {
            { Stat.Attack, Mathf.FloorToInt((P_Base.Attack * PokemonLevel) / 100f) + 5 },
            { Stat.Defence, Mathf.FloorToInt((P_Base.Defence * PokemonLevel) / 100f) + 5 },
            { Stat.SpAttack, Mathf.FloorToInt((P_Base.SpAttack * PokemonLevel) / 100f) + 5 },
            { Stat.SpDefence, Mathf.FloorToInt((P_Base.SpDefence * PokemonLevel) / 100f) + 5 },
            { Stat.Speed, Mathf.FloorToInt((P_Base.Speed * PokemonLevel) / 100f) + 5 }
        };

        MaxHp = Mathf.FloorToInt((P_Base.MaxHp * PokemonLevel) / 100f) + 10 + PokemonLevel;
    }
    public void ResetRankup()
    {
        Rankup = new Dictionary<Stat, int>()
        {
            {Stat.Attack , 0},
            {Stat.Defence , 0},
            {Stat.SpAttack , 0},
            {Stat.SpDefence , 0},
            {Stat.Speed , 0},
            {Stat.Accuracy , 0},
            {Stat.Evasion , 0}
        };
    }
    int GetStat(Stat stat)
    {
        int statVal = Stats[stat];

        //랭크업
        int rank = Rankup[stat];
        var rankupValues = new float[] { 1f, 1.5f, 2f, 2.5f, 3f, 3.5f, 4f };

        if (rank >= 0)
        {
            statVal = Mathf.FloorToInt(statVal * rankupValues[rank]);
        }
        else
        {
            statVal = Mathf.FloorToInt(statVal * rankupValues[-rank]);
        }

        return statVal;
    }
    public void ApplyRankups(List<Rankup> rankUps)
    {
        foreach (var rankUp in rankUps)
        {
            var stat = rankUp.stat;
            // var rank = rankUp.rank;

            var newRank = rankUp.rank; // 현재 랭크를 가져옴
            var oldRank = Rankup[stat]; // 기존 랭크를 가져옴

            // 새로운 랭크로 업데이트
            Rankup[stat] = Mathf.Clamp(newRank, -6, 6);

            // 메시지 생성 (랭크 변화량 계산)
            var rankChange = newRank - oldRank;

            // 최대/최소 랭크 확인
            bool isMaxRank = Rankup[stat] == 6;
            bool isMinRank = Rankup[stat] == -6;

            string statName = Enum.GetName(typeof(Stat), stat);
            if (stat == Stat.Attack)
            {
                statName = "공격";
            }
            else if (stat == Stat.Defence)
            {
                statName = "방어";
            }
            else if (stat == Stat.SpAttack)
            {
                statName = "특수공격";
            }
            else if (stat == Stat.SpDefence)
            {
                statName = "특수방어";
            }
            else if (stat == Stat.Speed)
            {
                statName = "스피드";
            }

            if (isMaxRank)
            {
                if (rankChange > 0)
                {
                    StatusCngMsg.Enqueue($"{_base.PokemonName}의 {statName}이 크게 올라갔다!");
                }
                StatusCngMsg.Enqueue($"{_base.PokemonName}의 {statName}은 더 올라가지 않는다!");
            }
            else if (isMinRank)
            {
                if (rankChange < 0)
                {
                    StatusCngMsg.Enqueue($"{_base.PokemonName}의 {statName}이 크게 떨어졌다!");
                }
                StatusCngMsg.Enqueue($"{_base.PokemonName}의 {statName}은 더 떨어지지 않는다!");
            }
            else
            {
                // 일반적인 랭크 변화 메시지 처리
                if (rankChange >= 2)
                {
                    StatusCngMsg.Enqueue($"{_base.PokemonName}의 {statName}이 크게 올라갔다!");
                }
                else if (rankChange > 0)
                {
                    StatusCngMsg.Enqueue($"{_base.PokemonName}의 {statName}이 올라갔다!");
                }
                else if (rankChange <= -2)
                {
                    StatusCngMsg.Enqueue($"{_base.PokemonName}의 {statName}이 크게 떨어졌다!");
                }
                else if (rankChange < 0)
                {
                    StatusCngMsg.Enqueue($"{_base.PokemonName}의 {statName}이 떨어졌다!");
                }
            }
        }
    }
    public bool CheckForLevelUp()
    {
        if (PokemonExp > P_Base.GetExpForLevel(level + 1))
        {
            level++;
            return true;
        }
        return false;
    }
    public LearnableSkill GetLearnableSkill()
    {
        return P_Base.LearnableSkills.Where(x => x.Level == level).FirstOrDefault();
    }
    public void LearnSkill(LearnableSkill skillToLearn)
    {
        if (Skills.Count > PokemonBase.MaxNumOfSkills)
        {
            return;
        }
        Skills.Add(new Skill(skillToLearn.SkillBase));
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
        float typeDmgMag = TypeChart.GetEffectiveness(skill.SkillBase.SkillType, this.P_Base.Type1) * TypeChart.GetEffectiveness(skill.SkillBase.SkillType, this.P_Base.Type2);       //타입별 데미지 배율

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
            if (attacker.Status == ConditionsDB.Conditions[ConditionID.BRN])
            {
                attack = attacker.Attack / 2;
            }
            else
            {
                attack = attacker.Attack;
            }
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

        float modifiers = UnityEngine.Random.Range(0.85f, 1.0f) * typeDmgMag * critical;
        float a = (2 * attacker.PokemonLevel + 10) / 250.0f;
        float d = a * skill.SkillBase.SkillPower * ((float)attack / defence) + 2;
        int damage = Mathf.FloorToInt(d * modifiers);

        int startHp = PokemonHp;

        UpdateHp(damage);
        return (startHp, PokemonHp, damageDetails);
    }
    public Skill GetRandomSkill()
    {
        var skillWithPP = Skills.Where(x => x.PP > 0).ToList();

        int r = UnityEngine.Random.Range(0, skillWithPP.Count);
        return Skills[r];
    }
    string GetCorrectParticle(string name, bool subject)    //은는이가
    {
        char lastChar = name[name.Length - 1];
        int unicode = (int)lastChar;
        bool endsWithConsonant = (unicode - 44032) % 28 != 0; // 44032는 '가'의 유니코드, 28는 받침의 수


        if (subject)
        {
            return endsWithConsonant ? "이" : "가";
        }
        else
        {
            return endsWithConsonant ? "은" : "는";
        }
    }

    public void SetStatus(ConditionID conditionID)
    {
        if (Status != null)
        {
            return;
        }

        Status = ConditionsDB.Conditions[conditionID];
        Status?.OnStart?.Invoke(this);
        StatusCngMsg.Enqueue($"{_base.PokemonName}{GetCorrectParticle(_base.PokemonName, false)} {Status.StartMessage}");
        OnStatusChanged?.Invoke();
    }
    public void CureStatus()
    {
        Status = null;
        OnStatusChanged?.Invoke();
    }
    public void SetVolatileStatus(ConditionID conditionID)
    {
        if (VolatileStatus != null)
        {
            return;
        }

        VolatileStatus = ConditionsDB.Conditions[conditionID];
        VolatileStatus?.OnStart?.Invoke(this);
        StatusCngMsg.Enqueue($"{_base.PokemonName}{GetCorrectParticle(_base.PokemonName, false)} {Status.StartMessage}");
    }
    public void CureVolatileStatus()
    {
        VolatileStatus = null;
    }

    // public void OnBattleOver()
    // {
    //     VolatileStatus = null;
    //     ResetRankup();
    // }

    public void UpdateHp(int damage)
    {
        startHp = PokemonHp;
        PokemonHp = Mathf.Clamp(PokemonHp - damage, 0, MaxHp);
        IsHpChanged = true;
    }

    public bool OnBeforeSkill()
    {
        bool canPerformSkill = true;
        if (Status?.OnBeforeSkill != null)
        //Status != null && Status.OnBeforeMove != null
        {
            if (Status.OnBeforeSkill(this) == false)
            {
                canPerformSkill = false;
            }
        }
        if (VolatileStatus?.OnBeforeSkill != null)
        {
            if (VolatileStatus.OnBeforeSkill(this) == false)
            {
                canPerformSkill = false;
            }
        }
        return canPerformSkill;
    }

    public void OnAfterTurn()
    {
        Status?.OnAfterTurn?.Invoke(this);
        VolatileStatus?.OnAfterTurn?.Invoke(this);
    }
    public PokemonType Type1
    {
        get { return P_Base.Type1; }
    }
    public PokemonType Type2
    {
        get { return P_Base.Type2; }
    }
}
public class DamageDetails
{
    public bool Fainted { get; set; }
    public float Critical { get; set; }
    public float TypeEffectiveness { get; set; }
}
[System.Serializable]
public class PokemonSaveData
{
    public int index;
    public int hp;
    public int level;
    public int exp;
    public ConditionID? statusID;
    public List<SkillSaveData> skills;
}
