using Unity.VisualScripting;
using UnityEngine;

[System.Serializable]
[IncludeInSettings(true)]
public class Card
{
    [SerializeField] public string name;
    [SerializeField] public int value;
}