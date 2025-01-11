using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ConditionID
{
    None,
    PSN,
    BRN,
    PAR,
    FRZ
}
public class ConditionsDB
{
    public static Dictionary<ConditionID, PokemonCondition> Conditions { get; set; } = new Dictionary<ConditionID, PokemonCondition>
    {
        {
            ConditionID.PSN,
            new PokemonCondition()
            {
                ConditionName = "독",
                StartMessage = "의 몸에 독이 퍼졌다!",
                OnAfterTurn = (Pokemon pokemon) =>
                {
                    pokemon.UpdateHp(pokemon.MaxHp / 8);

                    pokemon.StatusCngMsg.Enqueue($"{pokemon.PokemonBase.PokemonName}은 독에 의한 데미지를 입었다!");
                }
            }
        },
        {
            ConditionID.BRN,
             new PokemonCondition()
            {
                ConditionName = "화상",
                StartMessage = "은 화상을 입었다!",
                OnAfterTurn = (Pokemon pokemon) =>
                {
                    pokemon.UpdateHp(pokemon.MaxHp / 16);
                    pokemon.StatusCngMsg.Enqueue($"{pokemon.PokemonBase.PokemonName}은 화상 데미지를 입었다!");
                }
            }
        }
    };
}
