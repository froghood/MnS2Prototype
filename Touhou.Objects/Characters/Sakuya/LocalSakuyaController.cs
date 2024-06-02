namespace Touhou.Objects.Characters;

public class LocalSakuyaController : LocalCharacterController<Sakuya> {
    public LocalSakuyaController(Sakuya c) : base(c) {
    }

    public override void Update() {
        base.Update();

        while (c.IsTimestopped && c.TimestopTimer.TotalElapsed >= c.TimestopSpendTime) {

            if (c.Power >= c.TimestopSpendCost) {
                c.TimestopSpendTime += Time.InSeconds(0.25f);
                c.SpendPower(c.TimestopSpendCost);
            } else {
                c.DisableTimestop(0L, false);
            }

        }
    }

}