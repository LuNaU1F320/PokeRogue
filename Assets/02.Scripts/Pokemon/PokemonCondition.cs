using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PokemonCondition
{
    public ConditionID Id { get; set; }
    public string ConditionName { get; set; }
    public string Description { get; set; }
    public string StartMessage { get; set; }
    public Action<Pokemon> OnStart { get; set; }
    public Func<Pokemon, bool> OnBeforeSkill { get; set; }
    public Action<Pokemon> OnAfterTurn { get; set; }
}
