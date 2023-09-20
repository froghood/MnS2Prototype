using System.Diagnostics;

namespace Touhou;

public class Clock {


    public Time Elapsed { get => (long)internalTimer.Elapsed.TotalMicroseconds; }

    private Stopwatch internalTimer = new();




    public void Start() => internalTimer.Start();
    public void Stop() => internalTimer.Stop();
    public void Restart() => internalTimer.Restart();
    public void Reset() => internalTimer.Reset();


}