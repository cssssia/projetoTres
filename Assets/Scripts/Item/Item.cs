using Unity.Netcode;

[System.Serializable]
public class Item
{
    public ItemType Type;
    public NetworkObjectReference cardNetworkObjectReference;
    public Player playerID = Player.DEFAULT;
    public int itemID;
    public bool isOnGame;

    public Item (ItemType p_type, NetworkObjectReference p_cardNetworkObjectReference, Player p_playerId, int p_itemID)
    {
        Type = p_type;
        cardNetworkObjectReference = p_cardNetworkObjectReference;
        playerID = p_playerId;
        itemID = p_itemID;
        isOnGame = false;
    }
}
