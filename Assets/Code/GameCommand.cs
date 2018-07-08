internal class GameCommand
{
    public CommandType Type { get; set; }
    public string Name { get; set; }
    public object Payload { get; set; }

}

internal enum CommandType
{
    PlayerCreate,
    Forward
}