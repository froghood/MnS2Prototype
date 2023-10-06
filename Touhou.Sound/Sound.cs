using NAudio.Wave;

namespace Touhou.Sound;

public class Sound {

    public const int SampleRate = 44100;
    public const int BitsPerSample = 32;
    public const int Channels = 2;

    public float[] Data { get; }
    public WaveFormat WaveFormat { get; }


    public Sound(string path, int resampleQuality = 1) {
        using var reader = new AudioFileReader(path);

        //Log.Info($"{reader.FileName}: {reader.WaveFormat}");

        WaveFormat = new WaveFormat(SampleRate, BitsPerSample, Channels);


        var wholeFile = new List<float>((int)(reader.Length / sizeof(float)));
        var readBuffer = new float[WaveFormat.SampleRate * WaveFormat.Channels];


        using (var resampler = new MediaFoundationResampler(reader, WaveFormat) {
            ResamplerQuality = resampleQuality,
        }) {

            var resampledAudio = resampler.ToSampleProvider();

            int samplesRead;

            while ((samplesRead = resampledAudio.Read(readBuffer, 0, readBuffer.Length)) > 0) {
                wholeFile.AddRange(readBuffer.Take(samplesRead));
            }
        }







        Data = wholeFile.ToArray();
    }
}