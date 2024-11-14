using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/CardsScriptableObject", order = 1)]
public class CardsScriptableObject : ScriptableObject, ICardScriptableObject
{
    [SerializeField] private GameObject prefab;
    public GameObject Prefab { get { return prefab; } }
    [SerializeField] public List<Card> deck;

    [System.Serializable]
    [IncludeInSettings(true)]
    public class Card
    {
        [SerializeField] public string name;
        [SerializeField] public int value;
        [SerializeField] public Material material;
    }
}