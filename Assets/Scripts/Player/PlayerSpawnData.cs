using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CFG_PlayerSpawnData", menuName = "ScriptableObjects/Player Spawn Data")]
public class PlayerSpawnData : ScriptableObject
{
    public Vector3[] spawnPosition;
    public Vector3[] spawnRotation;

}
