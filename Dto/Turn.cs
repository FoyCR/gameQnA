using gameQnA.Enums;

namespace gameQnA.Dto
{
    public class Turn
    {
        public int Id { get; set; }
        public Guid PlayerId { get; set; }
        public int DiceRollValue { get; set; }
        public int InitialSquare { get; set; }
        public int LandedSquare { get; set; }
        public int FinalSquare { get; set; }
        public TurnStatus Status { get; set; }
        public UnasweredQuestion Question { get; set; }
        public Guid AnswerId { get; set; }
        public bool RightAnswer { get; set; }

        public string context { get; set; }

        public Turn()
        {
            Status = TurnStatus.started;
        }
    }
}