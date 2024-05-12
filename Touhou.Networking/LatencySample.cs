namespace Touhou.Networking;

public class LatencySample {

    public float Latency { get; }
    public float Weight => weight;

    private float weight;


    public LatencySample(float latency) {
        this.Latency = latency;
    }

    public void Weigh(float weight) {
        this.weight += weight;
    }
}