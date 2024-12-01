using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FMODUnity;

public class FMODEvents : Singleton<FMODEvents>
{
    [field: Header("Music")]
    [field: SerializeField] public EventReference music { get; private set; }

    [field: Header("Ambience")]
    [field: SerializeField] public EventReference ambience { get; private set; }

    [field: Header("Button Pressed SFX")]
    [field: SerializeField] public EventReference buttonPressed { get; private set; }

    public string test = "test";
}
