using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCtrl : MonoBehaviour
{
    [SerializeField] Sprite sprite;
    [SerializeField] string name;

    public string TrainerName
    {
        get => name;
    }
    public Sprite TrainerSprite
    {
        get => sprite;
    }
}
