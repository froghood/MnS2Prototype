public enum PacketType : byte {
    LatencyCorrection,
    Connection,
    ConnectionResponse,
    TimeRequest,
    TimeResponse,
    SyncFinished,
    MatchStarted,
    VelocityChanged,
    AttackPressed,
    AttackReleased,
    BombPressed,
    SpentPower,
    Grazed,
    Hit,
    Death,
    DeathConfirmation,
    Rematch,
    UpdateProjectile,
    DestroyProjectile,
}