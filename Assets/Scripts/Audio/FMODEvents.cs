using UnityEngine;
using FMODUnity;

public class FMODEvents : Singleton<FMODEvents>
{
    // [field: Header("Music")]
    // [field: SerializeField] public EventReference Music { get; private set; }

    [field: Header("Ambience")]
    [field: SerializeField] public EventReference Ambience { get; private set; }

    [field: Header("Play Card SFX")]
    [field: SerializeField] public EventReference PlayCard { get; private set; }

    [field: Header("Hover Card SFX")]
    [field: SerializeField] public EventReference HoverCard { get; private set; }

    [field: Header("Turn Card SFX")]
    [field: SerializeField] public EventReference TurnCard { get; private set; }

}
