namespace gameQnA.Dto
{
    public class GameRequest
    {
        public string GameType { get; set; }
        public List<PlayerRequest> Players { get; set; }
    }
}