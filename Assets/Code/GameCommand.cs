internal class GameCommand
{
    public CommandType Type { get; set; }
    public string Token { get; set; }
    public object Payload { get; set; }

}

internal enum CommandType
{
    PlayerCreate,
    Forward,
    Reverse,
    Right,
    Left,
    TurretRight,
    TurretLeft,
    Stop,
    StopTurret,
    Fire
}