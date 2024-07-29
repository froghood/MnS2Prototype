using Touhou.Networking;

namespace Touhou.Objects.Characters;

public class AttackOld {

    public static AttackOld Empty => new AttackOld();

    public bool IsHoldable { get; private set; }
    public bool IsFocusable { get; private set; }

    public string IconSpriteName { get; private set; }
    public string FocusedIconSpriteName { get; private set; }

    private Action<Time, bool> localPress;
    private Action<Time, Time, bool> localHold;
    private Action<Time, Time, bool> localRelease;
    private Action<Packet> remotePress;
    private Action<Packet> remoteHold;
    private Action<Packet> remoteRelease;


    public AttackOld(string icon, Action<Time, bool> localPress, Action<Packet> remotePress) {
        IsHoldable = false;
        IsFocusable = false;
        IconSpriteName = icon;
        this.localPress = localPress;
        this.remotePress = remotePress;
    }

    public AttackOld(string icon, string focusedIcon, Action<Time, bool> localPress, Action<Packet> remotePress) {
        IsHoldable = false;
        IsFocusable = true;
        IconSpriteName = icon;
        FocusedIconSpriteName = focusedIcon;
        this.localPress = localPress;
        this.remotePress = remotePress;
    }

    public AttackOld(
        string icon,
        Action<Time, bool> localPress,
        Action<Time, Time, bool> localHold,
        Action<Time, Time, bool> localRelease,
        Action<Packet> remotePress,
        Action<Packet> remoteHold,
        Action<Packet> remoteRelease
    ) {
        IsHoldable = true;
        IsFocusable = false;
        IconSpriteName = icon;
        this.localPress = localPress;
        this.localHold = localHold;
        this.localRelease = localRelease;
        this.remotePress = remotePress;
        this.remoteHold = remoteHold;
        this.remoteRelease = remoteRelease;
    }

    public AttackOld(
        string icon,
        string focusedIcon,
        Action<Time, bool> localPress,
        Action<Time, Time, bool> localHold,
        Action<Time, Time, bool> localRelease,
        Action<Packet> remotePress,
        Action<Packet> remoteHold,
        Action<Packet> remoteRelease
    ) {
        IsHoldable = true;
        IsFocusable = true;
        IconSpriteName = icon;
        FocusedIconSpriteName = focusedIcon;
        this.localPress = localPress;
        this.localHold = localHold;
        this.localRelease = localRelease;
        this.remotePress = remotePress;
        this.remoteHold = remoteHold;
        this.remoteRelease = remoteRelease;
    }

    private AttackOld() {
        IconSpriteName = string.Empty;
        FocusedIconSpriteName = string.Empty;
    }

    public void LocalPress(Time cooldownOverflow, bool focused) => localPress?.Invoke(cooldownOverflow, focused);
    public void LocalHold(Time cooldownOverflow, Time holdTime, bool focused) => localHold?.Invoke(cooldownOverflow, holdTime, focused);
    public void LocalRelease(Time cooldownOverflow, Time holdTime, bool focused) => localRelease?.Invoke(cooldownOverflow, holdTime, focused);

    public void RemotePress(Packet packet) => remotePress?.Invoke(packet);
    public void RemoteHold(Packet packet) => remoteHold?.Invoke(packet);
    public void RemoteRelease(Packet packet) => remoteRelease?.Invoke(packet);
}
