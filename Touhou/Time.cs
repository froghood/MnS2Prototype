using SFML.System;

namespace Touhou;
public struct Time {

    private long microseconds;

    public Time(long microseconds) => this.microseconds = microseconds;

    public static Time InSeconds(float seconds) => new Time((long)Math.Round(seconds * 1000000f));
    public static Time InMilliseconds(long milliseconds) => new Time(milliseconds * 1000);

    public float AsSeconds() => microseconds / 1000000f;
    public long AsMilliseconds() => (long)Math.Round(microseconds / 1000d);

    public static Time operator +(Time left, Time right) => left.microseconds + right.microseconds;
    public static Time operator -(Time left, Time right) => left.microseconds - right.microseconds;
    public static Time operator *(Time left, Time right) => left.microseconds * right.microseconds;

    public static implicit operator long(Time duration) => duration.microseconds;
    public static implicit operator Time(long amount) => new Time(amount);
    public static implicit operator Time(SFML.System.Time time) => new Time(time.AsMicroseconds());

    public override string ToString() => microseconds.ToString();

}