namespace gameQnA.Dto
{
    public class AnsweredTurn
    {
        public Guid gameId { get; set; }
        public int turnId { get; set; }
        public char selector { get; set; }

        public AnsweredTurn(Guid gameId, int turnId, char selector)
        {
            this.gameId = gameId;
            this.turnId = turnId;
            this.selector = selector;
        }
    }
}