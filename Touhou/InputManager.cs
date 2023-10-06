
using System.Collections.ObjectModel;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using OpenTK.Input.Hid;
using OpenTK.Windowing.Common.Input;

using OpenTK.Mathematics;

using Vortice.XInput;
using Steamworks;

namespace Touhou;
public class InputManager {


    public Dictionary<PlayerActions, Keys> KeyboardConfig { get; private set; }
    public Dictionary<PlayerActions, int> GamepadButtonConfig { get; private set; }



    public event Action<PlayerActions> ActionPressed;
    public event Action<PlayerActions> ActionReleased;

    private Time bufferTime = Time.InMilliseconds(300);



    private readonly PlayerActions[] playerActions;
    private PlayerActions actionState = PlayerActions.None;
    private PlayerActions previousActionState = PlayerActions.None;
    private List<PlayerActions> actionPressOrder;
    private Dictionary<PlayerActions, (Time Time, PlayerActions StateSnapshot)> actionPressBuffer = new();
    private Dictionary<PlayerActions, Time> actionReleaseBuffer = new();




    public InputManager() {

        var jsonText = File.ReadAllText("./KeyConfig.json");
        var json = JObject.Parse(jsonText);

        KeyboardConfig = json["Keyboard"].ToObject<Dictionary<PlayerActions, Keys>>();
        GamepadButtonConfig = json["Gamepad"].ToObject<Dictionary<PlayerActions, int>>();

        playerActions = Enum.GetValues<PlayerActions>().Skip(1).ToArray();
        actionPressOrder = Enum.GetValues<PlayerActions>().Skip(1).ToList();


        // XInput.SetReporting(true);

        // Log.Info($"XInput version: {XInput.Version}");

    }


    public bool IsActionPressed(NativeWindow window, PlayerActions action) {
        return actionState.HasFlag(action);
    }


    public bool IsActionPressBuffered(PlayerActions action, out Time time, out PlayerActions state) {
        if (actionPressBuffer.TryGetValue(action, out var data)) {
            time = data.Time;
            state = data.StateSnapshot;
            return true;
        }
        time = Time.InSeconds(0f);
        state = PlayerActions.None;
        return false;
    }

    public bool IsActionReleaseBuffered(PlayerActions action) => actionReleaseBuffer.ContainsKey(action);
    public List<PlayerActions> GetActionOrder() => actionPressOrder;



    public void Process(NativeWindow window) {

        window.ProcessEvents(0);

        previousActionState = actionState;
        actionState = PlayerActions.None;

        // bool connected = false;
        // for (int i = 0; i < 4; i++) {
        //     connected = connected || XInput.GetState(i, out var _);
        // }

        // if (!connected) {
        //     //Log.Warn("No controllers connected!!!");
        // } else {
        //     //Log.Info("CONTROLLER CONNECTED");
        // }


        if (window.IsFocused) {

            foreach (var state in window.JoystickStates.Where(e => e != null)) {

                // buttons
                foreach (var action in playerActions) {
                    if (GamepadButtonConfig.TryGetValue(action, out var button)) {
                        if (state.IsButtonDown(button)) actionState |= action;
                    }
                }

                // hat
                actionState |= state.GetHat(0) switch {
                    Hat.Right => PlayerActions.Right,
                    Hat.RightUp => PlayerActions.Right | PlayerActions.Up,
                    Hat.Up => PlayerActions.Up,
                    Hat.LeftUp => PlayerActions.Up | PlayerActions.Left,
                    Hat.Left => PlayerActions.Left,
                    Hat.LeftDown => PlayerActions.Left | PlayerActions.Down,
                    Hat.Down => PlayerActions.Down,
                    Hat.RightDown => PlayerActions.Down | PlayerActions.Right,
                    _ => PlayerActions.None
                };

                // stick
                var position = new Vector2(state.GetAxis(0), -state.GetAxis(1));
                var angle = MathF.Atan2(position.Y, position.X);
                var distance = MathF.Sqrt(position.X * position.X + position.Y * position.Y);
                int direction = (int)MathF.Floor((angle / MathF.Tau + 1.0625f) % 1f * 8f);

                if (distance >= 0.3f) {
                    actionState |= (direction) switch {
                        0 => PlayerActions.Right,
                        1 => PlayerActions.Right | PlayerActions.Up,
                        2 => PlayerActions.Up,
                        3 => PlayerActions.Up | PlayerActions.Left,
                        4 => PlayerActions.Left,
                        5 => PlayerActions.Left | PlayerActions.Down,
                        6 => PlayerActions.Down,
                        7 => PlayerActions.Down | PlayerActions.Right,
                        _ => PlayerActions.None
                    };
                }
            }

            // controller
            // for (int i = 0; i < 4; i++) {
            //     if (!XInput.GetState(0, out var state)) continue;

            //     var gamepad = state.Gamepad;

            //     // buttons
            //     foreach (var action in playerActions) {
            //         if (GamepadButtonConfig.TryGetValue(action, out var button)) {

            //             if (gamepad.Buttons.HasFlag(button)) actionState |= action;

            //         }
            //     }

            //     // stick
            //     var position = new Vector2(Math.Max((short)-32767, gamepad.LeftThumbX), Math.Max((short)-32767, gamepad.LeftThumbY)) / 32767f;
            //     var angle = MathF.Atan2(position.Y, position.X);
            //     var distance = MathF.Sqrt(position.X * position.X + position.Y * position.Y);
            //     int direction = (int)MathF.Floor((angle / MathF.Tau + 1.0625f) % 1f * 8f);

            //     if (distance >= 0.3f) {
            //         actionState |= (direction) switch {
            //             0 => PlayerActions.Right,
            //             1 => PlayerActions.Right | PlayerActions.Up,
            //             2 => PlayerActions.Up,
            //             3 => PlayerActions.Up | PlayerActions.Left,
            //             4 => PlayerActions.Left,
            //             5 => PlayerActions.Left | PlayerActions.Down,
            //             6 => PlayerActions.Down,
            //             7 => PlayerActions.Down | PlayerActions.Right,
            //             _ => PlayerActions.None
            //         };
            //     }
            // }


            // keyboard
            foreach (var action in playerActions) {
                if (window.IsKeyDown(KeyboardConfig[action])) actionState |= action;
            }
        }



        foreach (var action in playerActions) {
            if (actionState.HasFlag(action) && !previousActionState.HasFlag(action)) BufferActionPress(action);
            if (!actionState.HasFlag(action) && !previousActionState.HasFlag(action)) BufferActionRelease(action);
        }




        // remove old buffers
        foreach (var (action, press) in actionPressBuffer) {
            if (Game.Time - press.Time >= bufferTime) actionPressBuffer.Remove(action);
        }

        foreach (var (action, time) in actionReleaseBuffer) {
            if (Game.Time - time >= bufferTime) actionReleaseBuffer.Remove(action);
        }

    }



    private void BufferActionPress(PlayerActions action) {

        actionPressBuffer[action] = (Game.Time, actionState);
        actionReleaseBuffer.Remove(action);

        ActionPressed.Invoke(action);

        actionPressOrder.RemoveAll(e => e == action);
        actionPressOrder.Insert(0, action);
    }

    private void BufferActionRelease(PlayerActions action) {

        actionReleaseBuffer[action] = Game.Time;
        ActionReleased.Invoke(action);
    }

    public void ConsumePressBuffer(PlayerActions action) {
        actionPressBuffer.Remove(action);
    }

}