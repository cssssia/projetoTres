// using System.Collections.Generic;
// using System.Linq;
// using Unity.Netcode;
// using Unity.VisualScripting;
// using UnityEngine;

// [System.Serializable]
// [IncludeInSettings(true)]
// public class CardList : INetworkSerializable
// {
//     [SerializeField] public int[] cardList;

//     public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
//     {
        
//         int length = 0;
//         int[] Array = cardList.ToArray();
//         if (!serializer.IsReader)
//         {
//             length = Array.Length;
//         }

//         serializer.SerializeValue(ref length);

//         for (int n = 0; n < length; ++n)
//         {
//             serializer.SerializeValue(ref Array[n]);
//         }
//     }
// }