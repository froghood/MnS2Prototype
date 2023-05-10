namespace Touhou.Scenes.Match.Objects;

public class AttackOld {

    public bool Focusable { get; init; }
    public bool Holdable { get; init; }
    public Action<Time, bool> OnPress { get; init; }
    public Action<Time, Time, bool> OnHold { get; init; }
    public Action<Time, Time, bool> OnRelease { get; init; }

    public bool Disabled { get; set; }
    public Time CooldownDuration { get; set; }
    public Time Cooldown { get; set; }
}