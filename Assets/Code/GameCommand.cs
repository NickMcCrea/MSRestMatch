public class GameCommand
{
    public CommandType Type { get; set; }
    public string Token { get; set; }
    public object Payload { get; set; }

}

public enum CommandType
{
    PlayerCreate,
    PlayerCreateTest,
    ToggleForward,
    ToggleReverse,
    ToggleRight,
    ToggleLeft,
    ToggleTurretRight,
    ToggleTurretLeft,
    FullStop,
    StopMove,
    StopTurn,
    StopTurret,
    Fire,
    GetState,
    Despawn,
    TurnToHeading,
    TurnTurretToHeading
}