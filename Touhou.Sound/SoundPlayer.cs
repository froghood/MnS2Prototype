
using NAudio;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;

namespace Touhou.Sound;

public class SoundPlayer {

    public float Volume { get => player.Volume; set => player.Volume = value; }

    private Dictionary<string, Sound> library = new();

    private IWavePlayer player;
    private MixingSampleProvider mixer;


    public SoundPlayer(float volume, int latency, int bufferCount) {

        player = new WaveOutEvent() {
            Volume = volume,
            DesiredLatency = latency,
            NumberOfBuffers = bufferCount,
        };


        mixer = new MixingSampleProvider(WaveFormat.CreateIeeeFloatWaveFormat(Sound.SampleRate, Sound.Channels)) {
            ReadFully = true,
        };

        player.Init(mixer);
        player.Play();
    }

    public void Load(string path) {
        var name = Path.GetFileNameWithoutExtension(path);
        library.Add(name, new Sound(path));
    }

    public void Play(string name) {
        if (!library.TryGetValue(name, out var sound)) return;
        mixer.AddMixerInput(new SoundSampleProvider(sound));
    }

}