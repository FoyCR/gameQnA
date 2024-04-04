using gameQnA.Enums;

namespace gameQnA.Dto
{
    public class Player
    {
        public Guid id { set; get; }
        public string name { set; get; }
        public int age { set; get; }

        public int TurnOrder { set; get; }

        public int currentSquare { set; get; }
        public PlayerCategory category { set; get; }
    }
}