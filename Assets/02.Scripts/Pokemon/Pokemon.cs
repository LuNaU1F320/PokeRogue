using System;
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
    public PokemonCondition Status { get; private set; }
    public Queue<string> StatusCngMsg { get; private set; } = new Queue<string>();
    public bool IsHpChanged { get; set; }
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

        ResetRankup();
    }
    void CalculateStats()
    {
        Stats = new Dictionary<Stat, int>
        {
            { Stat.공격, Mathf.FloorToInt((PokemonBase.Attack * PokemonLevel) / 100f) + 5 },
            { Stat.방어, Mathf.FloorToInt((PokemonBase.Defence * PokemonLevel) / 100f) + 5 },
            { Stat.특수공격, Mathf.FloorToInt((PokemonBase.SpAttack * PokemonLevel) / 100f) + 5 },
            { Stat.특수방어, Mathf.FloorToInt((PokemonBase.SpDefence * PokemonLevel) / 100f) + 5 },
            { Stat.스피드, Mathf.FloorToInt((PokemonBase.Speed * PokemonLevel) / 100f) + 5 }
        };

        MaxHp = Mathf.FloorToInt((PokemonBase.MaxHp * PokemonLevel) / 100f) + 10;
    }

    public void ResetRankup()
    {
        Rankup = new Dictionary<Stat, int>()
        {
            {Stat.공격 , 0},
            {Stat.방어 , 0},
            {Stat.특수공격 , 0},
            {Stat.특수방어 , 0},
            {Stat.스피드 , 0}
        };
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
            // var rank = rankUp.rank;

            var newRank = rankUp.rank; // 현재 랭크를 가져옴
            var oldRank = Rankup[stat]; // 기존 랭크를 가져옴

            // 새로운 랭크로 업데이트
            Rankup[stat] = Math.Clamp(newRank, -6, 6);

            // Rankup[stat] = Math.Clamp(Rankup[stat] + rank, -6, 6);

            // 메시지 생성 (랭크 변화량 계산)
            var rankChange = newRank - oldRank;

            // 최대/최소 랭크 확인
            bool isMaxRank = Rankup[stat] == 6;
            bool isMinRank = Rankup[stat] == -6;

            // 메시지 처리
            if (isMaxRank)
            {
                if (rankChange > 0)
                {
                    StatusCngMsg.Enqueue($"{_base.PokemonName}의 {stat}이 크게 올라갔다!");
                }
                StatusCngMsg.Enqueue($"{_base.PokemonName}의 {stat}은 더 올라가지 않는다!");
            }
            else if (isMinRank)
            {
                if (rankChange < 0)
                {
                    StatusCngMsg.Enqueue($"{_base.PokemonName}의 {stat}이 크게 떨어졌다!");
                }
                StatusCngMsg.Enqueue($"{_base.PokemonName}의 {stat}은 더 떨어지지 않는다!");
            }
            else
            {
                // 일반적인 랭크 변화 메시지 처리
                if (rankChange >= 2)
                {
                    StatusCngMsg.Enqueue($"{_base.PokemonName}의 {stat}이 크게 올라갔다!");
                }
                else if (rankChange > 0)
                {
                    StatusCngMsg.Enqueue($"{_base.PokemonName}의 {stat}이 올라갔다!");
                }
                else if (rankChange <= -2)
                {
                    StatusCngMsg.Enqueue($"{_base.PokemonName}의 {stat}이 크게 떨어졌다!");
                }
                else if (rankChange < 0)
                {
                    StatusCngMsg.Enqueue($"{_base.PokemonName}의 {stat}이 떨어졌다!");
                }
            }
        }
    }

    public int Attack
    {
        get { return GetStat(Stat.공격); }
    }
    public int Defence
    {
        get { return GetStat(Stat.방어); }
    }
    public int SpAttack
    {
        get { return GetStat(Stat.특수공격); }
    }
    public int SpDefence
    {
        get { return GetStat(Stat.특수방어); }
    }
    public int Speed
    {
        get { return GetStat(Stat.스피드); }
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

        int startHp = PokemonHp;
        // PokemonHp -= damage;
        // if (PokemonHp <= 0)
        // {
        //     PokemonHp = 0;
        //     damageDetails.Fainted = true;
        //     return (startHp, 0, damageDetails);
        // }
        UpdateHp(damage);
        return (startHp, PokemonHp, damageDetails);
    }

    public void SetStatus(ConditionID conditionID)
    {
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
        Status = ConditionsDB.Conditions[conditionID];
        StatusCngMsg.Enqueue($"{_base.PokemonName}{GetCorrectParticle(_base.PokemonName, false)} {Status.StartMessage}");
    }

    public Skill GetRandomSkill()
    {
        int r = UnityEngine.Random.Range(0, Skills.Count);
        return Skills[r];
    }

    public void OnBattleOver()
    {
        ResetRankup();
    }

    public class DamageDetails
    {
        public bool Fainted { get; set; }
        public float Critical { get; set; }
        public float TypeEffectiveness { get; set; }
    }

    public (int startHp, int endHp) UpdateHp(int damage)
    {
        int startHp = PokemonHp;
        PokemonHp = Mathf.Clamp(PokemonHp - damage, 0, MaxHp);
        IsHpChanged = true;

        return (startHp, PokemonHp);
    }

    public void OnAfterTurn()
    {
        Status?.OnAfterTurn?.Invoke(this);
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
