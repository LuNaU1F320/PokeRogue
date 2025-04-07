using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;
using Cysharp.Threading.Tasks;

public class PokemonParty : MonoBehaviour
{
    [SerializeField] List<Pokemon> party;

    public event Action OnUpdated;

    public List<Pokemon> Party
    {
        get
        {
            return party;

        }
        set
        {
            party = value;
            OnUpdated?.Invoke();
        }
    }
    public Pokemon GetHealthyPokemon()
    {
        return party.Where(x => x.PokemonHp > 0).FirstOrDefault();
    }
    public void AddPokemon(Pokemon newPokemon)
    {
        if (party.Count < 6)
        {
            party.Add(newPokemon);
            OnUpdated?.Invoke();
        }
        else
        {
            //놓아줄 포켓몬 선택
        }
    }
    public void RemovePokemon(Pokemon removePokemon)
    {
        party.Remove(removePokemon);
    }

    public IEnumerator CheckForEvolutions()
    {
        foreach (var pokemon in party)
        {
            var evolution = pokemon.CheckForEvolution();
            if (evolution != null)
            {
                BattleSystem.Inst.state = BattleState.Evolution;

                yield return EvolutionManager.Inst.Evolve(pokemon, evolution);

                yield return new WaitForSeconds(0.5f);

                BattleSystem.Inst.state = BattleState.Busy;
            }
        }
        OnUpdated?.Invoke();
    }
    // 파티 내 모든 포켓몬의 진화 조건을 확인하고 진화 연출을 실행함
    // public async UniTask CheckForEvolutions()
    // {
    //     foreach (var pokemon in party)
    //     {
    //         var evolution = pokemon.CheckForEvolution();
    //         if (evolution != null)
    //         {
    //             BattleSystem.Inst.state = BattleState.Evolution;

    //             await EvolutionManager.Inst.EvolveAsync(pokemon, evolution);
    //             await UniTask.Delay(500); // 진화 마무리 시간 대기

    //             BattleSystem.Inst.state = BattleState.Busy;
    //         }
    //     }

    //     OnUpdated?.Invoke();
    // }
}
