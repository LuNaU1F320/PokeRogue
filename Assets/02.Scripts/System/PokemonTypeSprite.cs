using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PokemonTypeSprite : MonoBehaviour
{
    [SerializeField] Sprite Normal;
    [SerializeField] Sprite Fire;
    [SerializeField] Sprite Water;
    [SerializeField] Sprite Electric;
    [SerializeField] Sprite Grass;
    [SerializeField] Sprite Ice;
    [SerializeField] Sprite Fighting;
    [SerializeField] Sprite Poison;
    [SerializeField] Sprite Ground;
    [SerializeField] Sprite Rock;
    [SerializeField] Sprite Flying;
    [SerializeField] Sprite Psychinc;
    [SerializeField] Sprite Dark;
    [SerializeField] Sprite Steel;
    [SerializeField] Sprite Bug;
    [SerializeField] Sprite Ghost;
    [SerializeField] Sprite Dragon;
    [SerializeField] Sprite Fairy;
    Image Type;

    public void TypeSpriteSet(Pokemon Pokemon)
    {
        if (Pokemon.Type2 == PokemonType.None)
        {
            if (Pokemon.Type1 == PokemonType.Fire)
            {
                GetComponent<Image>().sprite = Fire;
            }
        }
    }

}
