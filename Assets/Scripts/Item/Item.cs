using Unity.Netcode;

public class Item
{
    public ItemType Type;
    public NetworkObjectReference cardNetworkObjectReference;
    public int playerId = -1;
    public bool isOnGame;

    public Item (ItemType p_type, NetworkObjectReference p_cardNetworkObjectReference, int p_playerId)
    {
        Type = p_type;
        cardNetworkObjectReference = p_cardNetworkObjectReference;
        isOnGame = false;
        playerId = p_playerId;
    }
}
