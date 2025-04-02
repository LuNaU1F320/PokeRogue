using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

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
    private void Awake()
    {
        // DontDestroyOnLoad(this.gameObject);
        // if (Party != null)
        // {
        //     foreach (var pokemon in party)
        //     {
        //         pokemon.Init();
        //     }
        // }
    }
    void Start()
    {
        // if (Party != null)
        // {
        //     foreach (var pokemon in party)
        //     {
        //         pokemon.Init();
        //     }
        // }
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

    // public static PokemonParty GetPlayerParty()
    // {
    //     return PlayerCtrl.Instance?.Party;
    // }
}
