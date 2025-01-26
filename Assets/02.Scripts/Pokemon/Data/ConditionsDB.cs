using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ConditionID
{
    None,
    PSN,
    BRN,
    PAR,
    FRZ,
    SLP,
    Confusion
}
public class ConditionsDB
{
    public static void Init()
    {
        foreach (var kvp in Conditions)
        {
            var conditionId = kvp.Key;
            var condition = kvp.Value;

            condition.Id = conditionId;
        }
    }
    public static Dictionary<ConditionID, PokemonCondition> Conditions { get; set; } = new Dictionary<ConditionID, PokemonCondition>
    {
        {
            ConditionID.PSN,
            new PokemonCondition()
            {
                ConditionName = "독",
                StartMessage = "몸에 독이 퍼졌다!",
                OnAfterTurn = (Pokemon pokemon) =>
                {
                    pokemon.UpdateHp(pokemon.MaxHp / 8);

                    pokemon.StatusCngMsg.Enqueue($"{pokemon.P_Base.PokemonName}은 독에 의한 데미지를 입었다!");
                }
            }
        },
        {
            ConditionID.BRN,
             new PokemonCondition()
            {
                ConditionName = "화상",
                StartMessage = "화상을 입었다!",
                OnAfterTurn = (Pokemon pokemon) =>
                {
                    pokemon.UpdateHp(pokemon.MaxHp / 16);
                    pokemon.StatusCngMsg.Enqueue($"{pokemon.P_Base.PokemonName}은 화상 데미지를 입었다!");
                }
            }
        },
        {
            ConditionID.PAR,
             new PokemonCondition()
            {
                ConditionName = "마비",
                StartMessage = "마비되어 기술이 나오기 어려워졌다!",
                OnStart = (Pokemon pokemon) =>
                {
                    // pokemon.SetStat(Stat.Speed, 2);
                },
                OnBeforeSkill = (Pokemon pokemon) =>
                {
                    if( Random.Range(1,5) == 1)
                    {
                        pokemon.StatusCngMsg.Enqueue($"{pokemon.P_Base.PokemonName}은 몸이 저려서 움직일 수 없다!");
                        return false;
                    }
                    return true;
                }
            }
        },
        {
            ConditionID.FRZ,
             new PokemonCondition()
            {
                ConditionName = "얼음",
                StartMessage = "얼어붙었다!",
                OnBeforeSkill = (Pokemon pokemon) =>
                {
                    if( Random.Range(1,5) == 1)
                    {
                        pokemon.CureStatus();
                        pokemon.StatusCngMsg.Enqueue($"{pokemon.P_Base.PokemonName}은(는) 얼음에서 풀렸다!");
                        return true;
                    }
                    else
                    {
                        pokemon.StatusCngMsg.Enqueue($"{pokemon.P_Base.PokemonName}은 얼어버려서 움직일 수 없다!");
                        return false;
                    }
                }
            }
        },
        {
            ConditionID.SLP,
             new PokemonCondition()
            {
                ConditionName = "잠듦",
                StartMessage = "잠들어 버렸다!",
                OnStart = (Pokemon pokemon) =>
                {
                    pokemon.StatusTime = Random.Range(1, 4);
                },
                OnBeforeSkill = (Pokemon pokemon) =>
                {
                    if(pokemon.StatusTime <= 0)
                    {
                    pokemon.CureStatus();
                    pokemon.StatusCngMsg.Enqueue($"{pokemon.P_Base.PokemonName}은 잠에서 깨어났다!");
                    return true;
                    }
                    pokemon.StatusTime --;
                    pokemon.StatusCngMsg.Enqueue($"{pokemon.P_Base.PokemonName}은 쿨쿨 잠들어 있다!");
                    return false;
                }
            }
        },
        //일시 상태이상
        {
            ConditionID.Confusion,
             new PokemonCondition()
            {
                ConditionName = "혼란",
                StartMessage = "혼란에 빠졌다!",
                OnStart = (Pokemon pokemon) =>
                {
                    pokemon.VolatileStatusTime = Random.Range(1, 5);
                },
                OnBeforeSkill = (Pokemon pokemon) =>
                {
                    if(pokemon.VolatileStatusTime <= 0)
                    {
                    pokemon.CureVolatileStatus();
                    pokemon.StatusCngMsg.Enqueue($"{pokemon.P_Base.PokemonName}은 혼란에서 풀렸다!");
                    return true;
                    }
                    pokemon.VolatileStatusTime --;
                    if( Random.Range(1,3) == 1)
                    {
                        return true;
                    }

                    pokemon.StatusCngMsg.Enqueue($"{pokemon.P_Base.PokemonName}은 혼란에 빠져 있다!");
                    pokemon.UpdateHp(pokemon.MaxHp / 8);
                    pokemon.StatusCngMsg.Enqueue($"영문도 모른 채 자신을 공격했다!");
                    return false;
                }
            }
        },
    };
    public static float GetStatusBonus(PokemonCondition condition)
    {
        if (condition == null)
        {
            return 1f;
        }
        else if (condition.Id == ConditionID.SLP || condition.Id == ConditionID.FRZ)
        {
            return 2f;
        }
        else if (condition.Id == ConditionID.PAR || condition.Id == ConditionID.PSN || condition.Id == ConditionID.BRN)
        {
            return 1.5f;
        }

        return 1f;
    }

}
