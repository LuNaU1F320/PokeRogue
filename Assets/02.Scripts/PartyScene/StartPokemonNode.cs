using UnityEngine;
using UnityEngine.UI;

public class StartPokemonNode : MonoBehaviour
{
    [SerializeField] public int PokemonIndex;
    [SerializeField] public Image PokemonDot;
    [SerializeField] public Image Select_Cursor;
    [SerializeField] public bool IsCatch;

    private int pokemonGen;

    public void Init(int index, bool isCatch)
    {
        PokemonIndex = index;
        IsCatch = isCatch;
        SetPokemonData();
    }

    private void SetPokemonData()
    {
        SetGeneration();
        LoadPokemonSprite();
        PokemonDot.color = IsCatch ? Color.white : Color.black;
    }

    private void SetGeneration()
    {
        int[] genBoundaries = { 151, 251, 386, 493, 649, 721, 809, 898 };
        for (int i = 0; i < genBoundaries.Length; i++)
        {
            if (PokemonIndex <= genBoundaries[i])
            {
                pokemonGen = i + 1;
                return;
            }
        }
        pokemonGen = 9; // 9세대 이상
    }

    private void LoadPokemonSprite()
    {
        PokemonDot.sprite = Resources.Load<Sprite>($"Image/Pokemon/PokemonDot/{pokemonGen}/{PokemonIndex}");
        if (!IsCatch)
        {
            PokemonDot.color = Color.black;
        }
    }
}
