using gameQnA.Dto;
using gameQnA.Enums;
using OneOf;

namespace gameQnA.Services
{
    public interface IGameInMemoryService
    {
        List<Game> GetGames();

        Task<Game> CreateGame(GameRequest request);

        Task<OneOf<Turn, NotFoundException>> CreateNextTurn(Guid gameId);

        Task<OneOf<Turn, NotFoundException>> AnswerTurn(AnsweredTurn answeredTurn);

        Task<OneOf<Game, NotFoundException>> GetGame(Guid gameId);
    }

    /// <summary>
    /// Main Service for the game
    /// </summary>
    public class GameInMemoryService : IGameInMemoryService
    {
        private IQuestionInMemoryService questionService;
        private IOpenAIService IAService;

        private static readonly Random getrandom = new Random();
        private List<Game> games;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="questionInMemoryService"></param>
        /// <param name="openAIService"></param>
        public GameInMemoryService(IQuestionInMemoryService questionInMemoryService, IOpenAIService openAIService)
        {
            questionService = questionInMemoryService;
            IAService = openAIService;
            games = new List<Game>();
        }

        public List<Game> GetGames()
        {
            return games;
        }

        /// <summary>
        /// Create a new game
        /// </summary>
        /// <param name="gameRequest"></param>
        /// <returns></returns>
        public async Task<Game> CreateGame(GameRequest gameRequest)
        {
            var players = GetOrderedPlayers(gameRequest.Players);
            var squares = GenerateSquares(gameRequest.GameType);

            var game = new Game
            {
                Id = Guid.NewGuid(),
                Status = GameStatus.Initiated,
                Squares = squares,
                Players = players
            };

            games.Add(game);
            return game;
        }

        /// <summary>
        /// Creates the next turn for a given game
        /// </summary>
        /// <param name="gameId"></param>
        /// <returns></returns>
        public async Task<OneOf<Turn, NotFoundException>> CreateNextTurn(Guid gameId)
        {
            Game? game = games.FirstOrDefault(g => g.Id == gameId);
            if (game is null)
            {
                return new NotFoundException("Game not found");
            }

            //Check if there is an already started turn
            Turn? turn = game.Turns.FirstOrDefault(t => t.Status == TurnStatus.started);
            if (turn is not null)
            {
                return turn;
            }

            var lastTurn = game.Turns.LastOrDefault();
            Player nextPlayer = game.Players.First();
            int turnId = 1;
            int rollDiceValue = GetDiceRoll();

            if (lastTurn is Turn)
            {
                Player? lastPlayer = game.Players.FirstOrDefault(p => p.id == lastTurn.PlayerId);
                nextPlayer = game.Players.Where(p => p.TurnOrder > lastPlayer?.TurnOrder)
                                         .OrderBy(p => p.TurnOrder)
                                         .FirstOrDefault() ?? game.Players.First();
                turnId = lastTurn.Id + 1;
            }

            turn = new Turn
            {
                Id = turnId,
                PlayerId = nextPlayer.id,
                DiceRollValue = rollDiceValue,
                InitialSquare = nextPlayer.currentSquare,
                LandedSquare = nextPlayer.currentSquare + rollDiceValue,

                Question = RandomizeQuestion(NewAIQuestion(game, rollDiceValue, nextPlayer)),
                Status = TurnStatus.started
            };

            nextPlayer.currentSquare = turn.LandedSquare;

            game.Turns.Add(turn);

            return turn;
        }

        /// <summary>
        /// Check the answer for a given turn
        /// </summary>
        /// <param name="answeredTurn"></param>
        /// <returns></returns>
        public async Task<OneOf<Turn, NotFoundException>> AnswerTurn(AnsweredTurn answeredTurn)
        {
            Game game = games.Where(g => g.Id == answeredTurn.gameId).FirstOrDefault();
            if (game is not Game)
            {
                return new NotFoundException("Game not Found");
            }
            Turn turn = game.Turns.Where(t => t.Id == answeredTurn.turnId && t.Status == TurnStatus.started).FirstOrDefault();
            if (turn is not Turn)
            {
                return new NotFoundException("Turn not Found or is finished");
            }

            //Validate answer
            Answer answer = turn.Question.Answers.Where(a => a.Selector == answeredTurn.selector).FirstOrDefault();
            if (answer is not Answer)
            {
                return new NotFoundException("Invalid Option for question, try again!");
            }
            Guid answerId = answer.Id;

            Question question = questionService.GetQuestions().Where(q => q.Id == turn.Question.Id).FirstOrDefault();

            if (question?.RighAnswerId == answerId)
            {
                turn.RightAnswer = true;
                turn.FinalSquare = turn.LandedSquare + question.AdditionalSquares;
            }
            else
            {
                turn.RightAnswer = false;
                turn.FinalSquare = turn.LandedSquare;
            }
            turn.AnswerId = answerId;
            turn.Status = TurnStatus.finished;
            turn.context = question.context;

            Player player = game.Players.FirstOrDefault(p => p.id == turn.PlayerId);
            player.currentSquare = turn.FinalSquare;

            return turn;
        }

        /// <summary>
        /// Return the current status for a given game
        /// </summary>
        /// <param name="gameId"></param>
        /// <returns></returns>
        public async Task<OneOf<Game, NotFoundException>> GetGame(Guid gameId)
        {
            if (games.FirstOrDefault(g => g.Id == gameId) is Game game)
            {
                return game;
            }

            return new NotFoundException("Game not found");
        }

        #region private methods

        private List<Player> GetOrderedPlayers(List<PlayerRequest> requestedPlayers)
        {
            requestedPlayers.Sort((a, b) => Guid.NewGuid().CompareTo(Guid.NewGuid()));

            List<Player> players = new List<Player>();
            players = requestedPlayers.Select((p, index) =>
                new Player
                {
                    id = Guid.NewGuid(),
                    name = p.Name,
                    age = p.Age,
                    category = GetPlayerCategory(p.Age),
                    currentSquare = 0,
                    TurnOrder = index + 1
                }
            ).OrderBy(p => p.TurnOrder).ToList();
            return players;
        }

        private List<Square> GenerateSquares(string gameType)
        {
            var numberOfSquares = GetNumberOfSquares(gameType);
            var squares = new List<Square>();

            // The first and last squares are neutral
            squares.Add(new Square { Id = 0, Type = QuestionCategory.neutral });

            // Generate random categories for the middle squares
            QuestionCategory previousCategory = QuestionCategory.neutral;
            for (int i = 1; i < numberOfSquares; i++)
            {
                QuestionCategory randomCategory;
                do
                {
                    randomCategory = GetRandomCategory();
                } while (randomCategory == previousCategory);

                squares.Add(new Square { Id = i, Type = randomCategory });
                previousCategory = randomCategory;
            }

            // Add the last neutral square
            squares.Add(new Square { Id = numberOfSquares, Type = QuestionCategory.neutral });

            return squares;
        }

        private QuestionCategory GetRandomCategory()
        {
            var values = Enum.GetValues(typeof(QuestionCategory));
            var random = new Random();
            QuestionCategory randomCategory;

            randomCategory = (QuestionCategory)values.GetValue(GetRandomNumber(1, values.Length));//excluding 0 (Neutral Category)

            return randomCategory;
        }

        private PlayerCategory GetPlayerCategory(int age) => age switch
        {
            <= 5 => PlayerCategory.Preschool,
            > 5 and <= 11 => PlayerCategory.ElementarySchool,
            > 11 and <= 14 => PlayerCategory.MiddleSchool,
            > 14 and <= 19 => PlayerCategory.HighSchool,
            _ => PlayerCategory.College
            /*
            <= 6 => PlayerCategory.preschool,
            > 6 and <= 12 => PlayerCategory.juniorSchool,
            > 13 and <= 18 => PlayerCategory.middleSchool,
            _ => PlayerCategory.adult
            */
        };

        private int GetNumberOfSquares(string gameType) => gameType switch
        {
            "short" => 30,
            "medium" => 60,
            "long" => 100,
            _ => 30
        };

        private static int GetRandomNumber(int min, int max)
        {
            lock (getrandom) // synchronize
            {
                return getrandom.Next(min, max);
            }
        }

        private UnasweredQuestion RandomizeQuestion(Question question)
        {
            UnasweredQuestion unasweredQuestion = new UnasweredQuestion
            {
                Id = question.Id,
                Body = question.Body
            };
            question.Answers.Sort((a, b) => Guid.NewGuid().CompareTo(Guid.NewGuid()));
            AssignSelectors(question.Answers);
            foreach (var answer in question.Answers)
            {
                unasweredQuestion.Answers.Add(answer);
            }
            return unasweredQuestion;
        }

        private Question NewAIQuestion(Game game, int rolDice, Player player)
        {
            int square = player.currentSquare + rolDice;
            string topic = Enum.GetName(game.Squares[square].Type) ?? string.Empty;
            string personType = Enum.GetName(player.category) ?? string.Empty;

            QuestionRequest request = new QuestionRequest(topic, personType);
            var task = IAService.GetQuestion(request);
            task.Wait();
            var response = task.Result;
            Question question = questionService.AddQuestion(request, response);
            return question;
        }

        private static void AssignSelectors(List<Answer> answers)
        {
            // Assign selectors A, B, C, etc. based on the order
            char selector = 'A';
            foreach (var answer in answers)
            {
                answer.Selector = selector;
                selector++;
            }
        }

        /// <summary>
        /// Simulate a dice roll
        /// </summary>
        /// <returns>a number between 1 and 6 </returns>
        private static int GetDiceRoll()
        { return GetRandomNumber(1, 7); }

        #endregion private methods
    }
}