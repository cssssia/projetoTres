using System.Collections.Generic;
using UnityEngine;
using FMODUnity;
using FMOD.Studio;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Volume")]
    [Range(0, 1)] public float masterVolume = 1;
    [Range(0, 1)] public float musicVolume = 1;
    [Range(0, 1)] public float ambienceVolume = 1;
    [Range(0, 1)] public float sfxVolume = 1;

    private Bus m_masterBus;
    private Bus m_musicBus;
    private Bus m_ambienceBus;
    private Bus m_sfxBus;

    private List<EventInstance> m_eventInstancesList;
    private List<StudioEventEmitter> m_eventEmittersList;

    private EventInstance m_musicEventInstance;
    private EventInstance m_ambienceEventInstance;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(this);

        DontDestroyOnLoad(this);

        m_eventInstancesList = new List<EventInstance>();
        m_eventEmittersList = new List<StudioEventEmitter>();

        m_masterBus = RuntimeManager.GetBus("bus:/");
        // m_musicBus = RuntimeManager.GetBus("bus:/Music");
        // m_ambienceBus = RuntimeManager.GetBus("bus:/Ambience");
        // m_sfxBus = RuntimeManager.GetBus("bus:/SFX");
    }

    private void Start()
    {
        InitializeAmbience(FMODEvents.Instance.Ambience);
        //InitializeMusic(FMODEvents.Instance.music);
    }

    private void Update()
    {
        m_masterBus.setVolume(masterVolume);
        // m_musicBus.setVolume(musicVolume);
        // m_ambienceBus.setVolume(ambienceVolume);
        // m_sfxBus.setVolume(sfxVolume);
    }

    private void InitializeMusic(EventReference p_musicEventReference)
    {
        m_musicEventInstance = CreateEventInstance(p_musicEventReference);
        m_musicEventInstance.start();
    }

    public void SetMusicParameter(string p_parameterName, float p_parameterValue)
    {
        m_musicEventInstance.setParameterByName(p_parameterName, p_parameterValue);
    }

    private void InitializeAmbience(EventReference p_ambienceEventReference)
    {
        m_ambienceEventInstance = CreateEventInstance(p_ambienceEventReference);
        m_ambienceEventInstance.start();
    }

    public void SetAmbienceParameter(string p_parameterName, float p_parameterValue)
    {
        m_ambienceEventInstance.setParameterByName(p_parameterName, p_parameterValue);
    }

    public void PlayOneShot(EventReference p_sound, Vector3 p_worldPos)
    {
        RuntimeManager.PlayOneShot(p_sound, p_worldPos);
    }

    public EventInstance CreateEventInstance(EventReference p_eventReference)
    {
        EventInstance l_eventInstance = RuntimeManager.CreateInstance(p_eventReference);
        m_eventInstancesList.Add(l_eventInstance);

        return l_eventInstance;
    }

    public StudioEventEmitter InitializeEventEmitter(EventReference p_eventReference, GameObject p_emitterGameObject)
    {
        StudioEventEmitter l_emitter = p_emitterGameObject.GetComponent<StudioEventEmitter>();
        l_emitter.EventReference = p_eventReference;
        m_eventEmittersList.Add(l_emitter);

        return l_emitter;
    }

    private void ClenUp()
    {
        foreach (EventInstance l_eventInstance in m_eventInstancesList)
        {
            l_eventInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
            l_eventInstance.release();
        }

        foreach (StudioEventEmitter l_eventEmitter in m_eventEmittersList)
        {
            l_eventEmitter.Stop();
        }
    }

    private void OnDestroy()
    {
        ClenUp();
    }
}
