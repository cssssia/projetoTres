using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ItemType { NONE, SCISSORS }

[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/ItemsScriptableObject", order = 1)]

public class ItemCardScriptableObject : ScriptableObject, ICardScriptableObject
{
    [SerializeField] private GameObject prefab;
    public GameObject Prefab { get { return prefab; } }
    public Vector3 InitialPosition;
    public Vector3 InitialRotation;

    public ItemConfig[] items;

    public ItemType[] initialItems;

    public ItemConfig GetItemConfig(ItemType p_type)
    {
        for (int i = 0; i < items.Length; i++)
        {
            if (items[i].item == p_type) return items[i];
        }

        return null;
    }

    [System.Serializable]
    public class ItemConfig
    {
        public ItemType item;
        public Material material;
        public string objectName;
    }
}
