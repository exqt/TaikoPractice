using UnityEngine;
using System;
using System.Collections.Generic;
using FMODUnity;
using FMOD;
using Debug = UnityEngine.Debug;
using FMOD.Studio;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    ChannelGroup masterChannelGroup;
    public FMOD.System coreSystem;
    public FMOD.Studio.System studioSystem;

    public EventReference donRef;
    public EventReference kaRef;

    EventInstance donInstance;
    EventInstance kaInstance;

    public ChannelGroup metronomeChannelGroup;
    public Bus hitSoundBus;

    public bool IsMetronomeOn { get; private set; } = true;
    public bool IsHitSoundOn { get; private set; } = true;

    public InputActionAsset inputActionAsset;
    InputAction toggleHitSoundAction, toggleMetronomeAction;

    public UnityEvent volumeChanged = new();

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            var systemOptionGroup = SystemOptionGroup.Instance;
            Init(systemOptionGroup);
            ApplySetting(systemOptionGroup);

            var inputMap = inputActionAsset.FindActionMap("Global");
            toggleHitSoundAction = inputMap.FindAction("ToggleHitSound");
            toggleMetronomeAction = inputMap.FindAction("ToggleMetronome");
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    public void ApplySetting(SystemOptionGroup systemOptionGroup)
    {
        if (systemOptionGroup == null)
        {
            Debug.LogError("SystemOptionGroup is null");
            return;
        }

        var outputType = systemOptionGroup.audioBackend;

        FMOD.OUTPUTTYPE outputTypeEnum;
        if (outputType == AudioBackend.WASAPI)
        {
            outputTypeEnum = FMOD.OUTPUTTYPE.WASAPI;
        }
        else if (outputType == AudioBackend.ASIO)
        {
            outputTypeEnum = FMOD.OUTPUTTYPE.ASIO;
        }
        else
        {
            Debug.LogError("Unsupported output type: " + outputType);
            return;
        }

        var result = coreSystem.setOutput(outputTypeEnum);
        if (result != FMOD.RESULT.OK)
        {
            Debug.LogError($"Failed to set output {outputType}: " + result);
        }

        SetHitSoundVolume(systemOptionGroup.hitSoundVolume);
        SetMetronomeVolume(systemOptionGroup.metronomeVolume);
    }

    public void Init(SystemOptionGroup systemOptionGroup)
    {
        studioSystem = RuntimeManager.StudioSystem;
        coreSystem = RuntimeManager.CoreSystem;

        /////////
        donInstance = RuntimeManager.CreateInstance(donRef);
        kaInstance = RuntimeManager.CreateInstance(kaRef);

        {
            var res = coreSystem.getMasterChannelGroup(out masterChannelGroup);
            if (res != FMOD.RESULT.OK)
            {
                Debug.LogError("FMOD getMasterChannelGroup failed " + res);
                return;
            }
        }

        {
            var res = coreSystem.createChannelGroup("Metronome", out metronomeChannelGroup);
            if (res != FMOD.RESULT.OK)
            {
                Debug.LogError("FMOD createChannelGroup failed: " + res);
                return;
            }

            res = masterChannelGroup.addGroup(metronomeChannelGroup);
            if (res != FMOD.RESULT.OK)
            {
                Debug.LogError("FMOD addChannelGroup failed: " + res);
                return;
            }
        }

        {
            hitSoundBus = RuntimeManager.GetBus("bus:/HitSound");
        }
    }

    public void ToggleHitSoundBus(bool isOn)
    {
        hitSoundBus.setMute(!isOn);
        volumeChanged.Invoke();
    }

    public void ToggleMasterChannelGroup(bool isOn)
    {
        masterChannelGroup.setMute(!isOn);
        volumeChanged.Invoke();
    }

    public void ToggleMetronomeChannelGroup(bool isOn)
    {
        metronomeChannelGroup.setMute(!isOn);
        volumeChanged.Invoke();
    }

    bool isMasterOn = true;
    void Update()
    {
        if (toggleHitSoundAction.WasPressedThisFrame())
        {
            IsHitSoundOn = !IsHitSoundOn;
            ToggleHitSoundBus(IsHitSoundOn);
        }
        else if (toggleMetronomeAction.WasPressedThisFrame())
        {
            IsMetronomeOn = !IsMetronomeOn;
            ToggleMetronomeChannelGroup(IsMetronomeOn);
        }
    }

    public void Clean()
    {
        coreSystem.clearHandle();
        var closeRes = coreSystem.close();
        if (closeRes != FMOD.RESULT.OK)
        {
            Debug.LogError("FMOD system close failed: " + closeRes);
            return;
        }

        var releaseRes = coreSystem.release();
        if (releaseRes != FMOD.RESULT.OK)
        {
            Debug.LogError("FMOD system release failed: " + releaseRes);
            return;
        }
    }

    readonly Dictionary<string, Sound> soundDict = new();

    public void PlaySound(string soundName)
    {
        if (soundName == "dong") donInstance.start();
        else if (soundName == "ka") kaInstance.start();

        // var s = soundDict[soundName];

        // var playResult = RuntimeManager.CoreSystem.playSound(s, masterChannelGroup, false, out var channel);
        // if (playResult != FMOD.RESULT.OK)
        // {
        //     Debug.LogError("FMOD playSound failed: " + playResult);
        //     return;
        // }
    }

    public void PlaySound(NoteType type)
    {
        if (type == NoteType.Don)
        {
            donInstance.start();
        }
        else if (type == NoteType.Ka)
        {
            kaInstance.start();
        }
    }

    public void SetMetronomeVolume(float volume)
    {
        var result = metronomeChannelGroup.setVolume(volume);
        if (result != FMOD.RESULT.OK)
        {
            Debug.LogError("Failed to set metronome volume: " + result);
        }
    }

    public void SetHitSoundVolume(float volume)
    {
        var result = hitSoundBus.setVolume(volume);
        if (result != FMOD.RESULT.OK)
        {
            Debug.LogError("Failed to set hit sound volume: " + result);
        }
    }

    public void LoadSound(AudioClip clip)
    {
        int samples = clip.samples * clip.channels;
        float[] floatData = new float[samples];
        clip.GetData(floatData, 0); // Read all samples

        // Convert float [-1.0, 1.0] to 16-bit PCM (little endian)
        byte[] pcmData = new byte[samples * 2]; // 2 bytes per sample
        for (int i = 0; i < samples; i++)
        {
            short intSample = (short)Mathf.Clamp(floatData[i] * 32767f, short.MinValue, short.MaxValue);
            pcmData[i * 2] = (byte)(intSample & 0xFF);
            pcmData[i * 2 + 1] = (byte)((intSample >> 8) & 0xFF);
        }

        // Setup FMOD sound format
        CREATESOUNDEXINFO exinfo = new()
        {
            cbsize = System.Runtime.InteropServices.Marshal.SizeOf(typeof(CREATESOUNDEXINFO)),
            length = (uint)pcmData.Length,
            numchannels = clip.channels,
            defaultfrequency = clip.frequency,
            format = SOUND_FORMAT.PCM16
        };

        // Create sound in memory
        Sound fmodSound;
        RESULT result = RuntimeManager.CoreSystem.createSound(
            pcmData,
            MODE.OPENMEMORY | MODE.OPENRAW | MODE.LOOP_OFF | MODE.CREATESAMPLE,
            ref exinfo, out fmodSound
        );

        if (result != FMOD.RESULT.OK)
        {
            Debug.LogError("FMOD createSound failed: " + result);
            return;
        }

        soundDict[clip.name] = fmodSound;
    }
}
