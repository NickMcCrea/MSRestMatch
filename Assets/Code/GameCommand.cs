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
    Forward,
    Reverse,
    Right,
    Left,
    TurretRight,
    TurretLeft,
    Stop,
    StopTurret,
    Fire,
    GetState,
    Despawn
}