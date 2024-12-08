using UnityEngine;

[System.Serializable]
public class Item
{
    public int id;
    public ItemType type = ItemType.NONE;
    public Player playerId = Player.DEFAULT;
    public GameObject gameObject;
    public bool isOnGame;

    public void ResetItem()
    {
        playerId = Player.DEFAULT;
    }
}

public enum ItemType { NONE, SCISSORS, STAKE }
