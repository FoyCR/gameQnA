using gameQnA.Enums;

namespace gameQnA.Dto
{
    public class Game
    {
        public Guid Id { get; set; }
        public List<Square> Squares { get; set; }
        public List<Player> Players { get; set; }

        public List<Turn> Turns { get; set; }

        public GameStatus Status { get; set; }

        public Game()
        {
            Squares = new List<Square>();
            Players = new List<Player>();
            Turns = new List<Turn>();
        }
    }
}