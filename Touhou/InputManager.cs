
using System.Collections.ObjectModel;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SFML.Graphics;
using SFML.System;
using SFML.Window;

namespace Touhou;
public class InputManager {


    public Dictionary<PlayerAction, Keyboard.Key> KeyboardConfig { get; private set; }

    public Dictionary<Keyboard.Key, PlayerAction> ActionsByKey { get; private set; } = new();
    //public Dictionary<PlayerAction, uint> GamepadConfig { get; private set; }

    public event Action<PlayerAction> ActionPressed;
    public event Action<PlayerAction> ActionReleased;

    private Time bufferTime = Time.InMilliseconds(300);

    private Dictionary<PlayerAction, (Time Time, Dictionary<PlayerAction, bool> StateSnapshot)> actionPressBuffer = new();
    private Dictionary<PlayerAction, Time> actionReleaseBuffer = new();

    private List<PlayerAction> actionPressOrder;

    private PlayerAction[] playerActions;

    private Dictionary<PlayerAction, bool> previousActionState = new();
    private Dictionary<PlayerAction, bool> actionState = new() {
        {PlayerAction.Right, false},
        {PlayerAction.Left, false},
        {PlayerAction.Down, false},
        {PlayerAction.Up, false},
        {PlayerAction.Focus, false},
        {PlayerAction.Primary, false},
        {PlayerAction.Secondary, false},
        {PlayerAction.SpellA, false},
        {PlayerAction.SpellB, false},
        {PlayerAction.Bomb, false},
    };

    public InputManager() {

        var jsonText = File.ReadAllText("./KeyConfig.json");
        var json = JObject.Parse(jsonText);

        KeyboardConfig = json["Keyboard"].ToObject<Dictionary<PlayerAction, Keyboard.Key>>();
        foreach (var entry in KeyboardConfig) {
            ActionsByKey.Add(entry.Value, entry.Key);
        }

        playerActions = Enum.GetValues<PlayerAction>().Skip(1).ToArray();
        actionPressOrder = new List<PlayerAction>(playerActions);
    }

    public void AttachEvents(RenderWindow window) {
        // window.KeyPressed += KeyPressed;
        // window.KeyReleased += KeyReleased;
        // window.LostFocus += LostFocus;
    }

    public bool GetActionState(PlayerAction action) {
        return actionState.TryGetValue(action, out bool state) && state;
    }

    public bool IsActionPressBuffered(PlayerAction action) {
        return IsActionPressBuffered(action, out _);
    }

    public bool IsActionPressBuffered(PlayerAction action, out ReadOnlyDictionary<PlayerAction, bool> state) {
        if (actionPressBuffer.TryGetValue(action, out var data)) {
            state = new ReadOnlyDictionary<PlayerAction, bool>(data.StateSnapshot);
            return true;
        }
        state = null;
        return false;
    }

    public bool IsActionReleaseBuffered(PlayerAction action) => actionReleaseBuffer.ContainsKey(action);
    public List<PlayerAction> GetActionOrder() => actionPressOrder;

    public void Update(RenderWindow window) {

        window.DispatchEvents();

        bool windowFocused = window.HasFocus();
        foreach (var action in playerActions) {

            // keeping track of action state each frame to determine if a press or release happened on this frame
            previousActionState[action] = actionState[action];
            actionState[action] = Keyboard.IsKeyPressed(KeyboardConfig[action]) && windowFocused;
        }

        foreach (var action in playerActions) {

            // previously not pressed but now is
            if (!previousActionState[action] && actionState[action]) BufferPressAction(action);

            // previously pressed but now isn't
            if (previousActionState[action] && !actionState[action]) BufferReleaseAction(action);
        }

        // remove old buffers
        foreach (var (action, press) in actionPressBuffer) {
            if (Game.Time - press.Time >= bufferTime) actionPressBuffer.Remove(action);
        }

        foreach (var (action, time) in actionReleaseBuffer) {
            if (Game.Time - time >= bufferTime) actionReleaseBuffer.Remove(action);
        }
    }

    public void Render() {
        var text = new Text();
        text.Font = Game.DefaultFont;
        text.CharacterSize = 14;

        float offset = 0f;
        foreach (var action in actionPressOrder) {
            bool pressed = actionState[action];
            text.DisplayedString = action.ToString();
            text.FillColor = new Color(255, 255, 255, 80);
            if (pressed) text.FillColor = Color.White;
            if (IsActionReleaseBuffered(action)) text.FillColor = new Color(0, 200, 255);
            if (IsActionPressBuffered(action)) text.FillColor = new Color(255, 0, 100);
            text.Origin = new Vector2f(text.GetLocalBounds().Width, 0f);
            text.Position = new Vector2f(Game.Window.Size.X - 2f, offset);

            Game.Window.Draw(text);

            offset += 20f;
        }
    }



    // private void KeyPressed(object sender, KeyEventArgs args) {
    //     if (ActionsByKey.TryGetValue(args.Code, out var action)) {
    //         BufferPressAction(action);
    //     }
    // }

    // private void KeyReleased(object sender, KeyEventArgs args) {
    //     if (ActionsByKey.TryGetValue(args.Code, out var action)) {
    //         BufferReleaseAction(action);
    //     }
    // }

    private void BufferPressAction(PlayerAction action) {
        actionPressBuffer[action] = (Game.Time, new Dictionary<PlayerAction, bool>(actionState));
        actionReleaseBuffer.Remove(action);

        ActionPressed.Invoke(action);

        actionPressOrder.RemoveAll(e => e == action);
        actionPressOrder.Insert(0, action);

        System.Console.WriteLine(action);

    }

    private void BufferReleaseAction(PlayerAction action) {
        actionReleaseBuffer[action] = Game.Time;

        ActionReleased.Invoke(action);
    }

    public void ConsumePressBuffer(PlayerAction action) {
        actionPressBuffer.Remove(action);
    }

    // private void LostFocus(object sender, EventArgs args) {
    //     // loop through and release all pressed actions
    //     foreach (var action in actionStates.Where(e => e.Value).Select(e => e.Key)) {
    //         actionStates[action] = false;
    //         ActionReleased.Invoke(action);
    //     }
    // }


}

public class ActionPress {

    public Time Time { get; }
    public PlayerAction Action { get; }
    private Dictionary<PlayerAction, bool> states;

    public bool Consumed { get; private set; }
    public bool Released { get; private set; }

    public ActionPress(Time time, PlayerAction action, Dictionary<PlayerAction, bool> states) {
        Time = time;
        Action = action;
        this.states = states;
    }

    public void Consume() => Consumed = true;
    public void Release() => Released = true;
    public bool WasActionPressed(PlayerAction action) => states[action];
}