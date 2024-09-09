using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Netcode;

public struct PlayerData : IEquatable<PlayerData>, INetworkSerializable
{
    public ulong clientId;
    public FixedString64Bytes playerName;
    //public List<Card> playerController;
    //public FixedList128Bytes playerController;
    //public ForceNetworkSerializeByMemcpy<FixedList128Bytes<int>> m_Name;
    public bool Equals(PlayerData other)
    {
        return
            clientId == other.clientId &&
            playerName == other.playerName;
            //playerController == other.playerController;
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref clientId);
        serializer.SerializeValue(ref playerName);
        //serializer.SerializeValue(ref playerController);
    }
}