public class CommandService : GameSystem {


    private Queue<Action> commands = new();


    public void Do(Action action) {
        commands.Enqueue(action);
    }

    protected override void Update() {
        while (commands.Count > 0) {
            commands.Dequeue().Invoke();
        }
    }


}