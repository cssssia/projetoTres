using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ItemType { NONE, SCISSORS }

[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/ItemsScriptableObject", order = 1)]

public class ItemCardScriptableObject : ScriptableObject, ICardScriptableObject
{
    [SerializeField] private GameObject prefab;
    public GameObject Prefab { get { return prefab; } }

    public Item[] items;

    [System.Serializable]
    public class Item
    {
        public ItemType item;
        public Material material;
    }
}
