using SFML.Audio;

namespace Touhou.Audio;

public class SoundPlayer {


    private Dictionary<string, SoundBuffer> soundLibrary = new();
    private List<Sound> sounds = new();

    public void Load(string soundsDirectory) {
        string[] soundPaths = Directory.GetFiles(soundsDirectory, "*.wav");

        foreach (var soundPath in soundPaths) {
            string soundName = Path.GetFileNameWithoutExtension(soundPath);

            soundLibrary[soundName] = new SoundBuffer(soundPath);
        }
    }

    public void Play(string soundName) {

        if (!soundLibrary.TryGetValue(soundName, out var buffer)) return;

        sounds.RemoveAll(e => e.Status == SoundStatus.Stopped);


        var sound = new Sound(buffer);
        sound.Volume = Game.Settings.SoundVolume;

        sounds.Add(sound);
        sound.Play();
    }

}