using UnityEngine;
using FMODUnity;
using FMOD;
using FMOD.Studio;
using System.Runtime.InteropServices;
using System;
using UnityEditor;

public class FMODMetronome
{
    readonly float bpm = 120f;

    private int sampleRate;

    readonly float[] samples = null;
    Sound sound;
    Channel channel;

    ChannelGroup masterChannelGroup;

    AudioManager audioManager;

    public FMODMetronome(float bpm = 120f, float offsetInMS = 0f)
    {
        this.bpm = bpm;
        samples = CreateMetronomeSoundSamples(bpm);

        sound = CreateFMODSound(samples, offsetInMS / 1000f);
        sound.setMode(MODE.LOOP_NORMAL | MODE.OPENMEMORY | MODE.OPENRAW | MODE.CREATESAMPLE);

        FMOD.System system = RuntimeManager.CoreSystem;
        system.getMasterChannelGroup(out masterChannelGroup);

        // metronomeInstance = RuntimeManager.CreateInstance("event:/MetronomeSample");
    }

    public void StartMetronome(double time = 0)
    {
        audioManager = AudioManager.Instance;

        FMOD.System system = RuntimeManager.CoreSystem;
        var result = system.playSound(sound, masterChannelGroup, false, out channel);
        if (result != FMOD.RESULT.OK)
        {
            UnityEngine.Debug.LogError("FMOD playSound failed" + result);
            return;
        }
        channel.setChannelGroup(audioManager.metronomeChannelGroup);

        channel.setPosition((uint)(time * 1000), TIMEUNIT.MS);
    }

    public void Pause()
    {
        channel.setPaused(true);
    }

    public void Resume()
    {
        channel.setPaused(false);
    }

#region
    float[] CreateMetronomeSoundSamples(float bpm)
    {
        int beats = 4;
        int repeatCount = 16; // Repeat the measure 16 times
        float beatIntervalSec = 60f / bpm;
        float beatDuration = beatIntervalSec * beats; // 4 beats for a full measure

        float firstHz = 440f * 3;
        float secondHz = 440f * 2;

        FMOD.System system = RuntimeManager.CoreSystem;
        system.getSoftwareFormat(out sampleRate, out _, out _);

        int totalSamples = (int)(beatDuration * sampleRate * repeatCount);

        float[] samples = new float[totalSamples];

        void ApplySineWave(int start, int end, float frequency)
        {
            for (int i = start; i < end; i++)
            {
                float t = (float)i / sampleRate;
                float p = (i - start) / (float)(end - start);
                var v = Mathf.Sin(2f * Mathf.PI * frequency * t);
                samples[i] = v * Mathf.Pow(1f - p, 2f); // Apply fade-out
            }
        }

        int samplesPerBeat = (int)(beatIntervalSec * sampleRate);
        float soundDuration = 0.01f;
        int soundDurationSamples = (int)(soundDuration * sampleRate);

        for (int repeat = 0; repeat < repeatCount; repeat++)
        {
            int measureStart = (int)(repeat * beatDuration * sampleRate);
            for (int i = 0; i < beats; i++)
            {
                int start = measureStart + i * samplesPerBeat;
                int end = start + soundDurationSamples;
                if (end > totalSamples) end = totalSamples; // Prevent overflow
                ApplySineWave(start, end, i == 0 ? firstHz : secondHz); // A4 note (440 Hz)
            }
        }

        return samples;
    }

    Sound CreateFMODSound(float[] samples, float offset = 0f)
    {
        int samplesCount = samples.Length;
        int offsetInSamples = (int)(offset * sampleRate);

        byte[] rotatedSamples = new byte[samplesCount * 2];
        for (int i = 0; i < samplesCount; i++)
        {
            int index = (i + offsetInSamples + samplesCount) % samplesCount;
            short sample = (short)(samples[index] * short.MaxValue);
            rotatedSamples[i * 2] = (byte)(sample & 0xFF);
            rotatedSamples[i * 2 + 1] = (byte)((sample >> 8) & 0xFF);
        }

        CREATESOUNDEXINFO exinfo = new()
        {
            cbsize = Marshal.SizeOf(typeof(CREATESOUNDEXINFO)),
            length = (uint)rotatedSamples.Length,
            numchannels = 1,
            defaultfrequency = sampleRate,
            format = SOUND_FORMAT.PCM16
        };

        RESULT result = RuntimeManager.CoreSystem.createSound(rotatedSamples,
            MODE.OPENMEMORY | MODE.OPENRAW | MODE.CREATESAMPLE,
            ref exinfo, out var sound);
        if (result != FMOD.RESULT.OK)
        {
            UnityEngine.Debug.LogError("FMOD createSound failed: " + result);
        }

        return sound;
    }
#endregion
}
