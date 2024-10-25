using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/PlayerSpawnPositionScriptableObject", order = 2)]
public class PlayerSpawnPositionScriptableObject : ScriptableObject
{
    [SerializeField] public List<Vector3> spawnPositionList;
}