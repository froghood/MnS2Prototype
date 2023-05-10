namespace Touhou.Scenes.Match.Objects;

public class PlayerAttackBuilder {
    private Dictionary<string, AttackOld> attacksByName;
    private Dictionary<PlayerAction, List<AttackOld>> attacksByInput;
    private PlayerAction actionInput;
    private string name;

    public bool Focusable { get; private set; }
    public bool Holdable { get; private set; }
    public Action<Time, bool> OnPress { get; private set; }
    public Action<Time, Time, bool> OnHold { get; private set; }
    public Action<Time, Time, bool> OnRelease { get; private set; }

    public PlayerAttackBuilder(Dictionary<string, AttackOld> attacksByName, Dictionary<PlayerAction, List<AttackOld>> attacksByInput, PlayerAction actionInput, string name) {
        this.attacksByName = attacksByName;
        this.attacksByInput = attacksByInput;
        this.actionInput = actionInput;
        this.name = name;
    }

    public PlayerAttackBuilder IsFocusable() { Focusable = true; return this; }
    public PlayerAttackBuilder IsHoldable() { Holdable = true; return this; }
    public PlayerAttackBuilder AddPress(Action<Time, bool> press) { OnPress = press; return this; }
    public PlayerAttackBuilder AddHold(Action<Time, Time, bool> hold) { OnHold = hold; return this; }
    public PlayerAttackBuilder AddRelease(Action<Time, Time, bool> release) { OnRelease = release; return this; }

    public void Build() {
        var attack = new AttackOld() {
            Focusable = Focusable,
            Holdable = Holdable,
            OnPress = OnPress,
            OnHold = OnHold,
            OnRelease = OnRelease
        };

        attacksByName.Add(name, attack);
        if (attacksByInput.TryGetValue(actionInput, out var attackList)) {
            attackList.Add(attack);
        } else {
            attacksByInput.Add(actionInput, new List<AttackOld>() { attack });
        }
    }



}
