using NAudio.Wave;

namespace Touhou.Sound;

public class SoundSampleProvider : ISampleProvider {

    private readonly Sound sound;
    private long position;

    public SoundSampleProvider(Sound sound) {
        this.sound = sound;
    }

    public WaveFormat WaveFormat => sound.WaveFormat;

    public int Read(float[] buffer, int offset, int count) {

        var availableSamples = sound.Data.Length - position;
        var samplesToCopy = Math.Min(availableSamples, count);

        Array.Copy(sound.Data, position, buffer, offset, samplesToCopy);

        position += samplesToCopy;

        return (int)samplesToCopy;
    }
}