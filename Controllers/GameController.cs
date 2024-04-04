// GameController.cs

using gameQnA.Dto;
using gameQnA.Services;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("[controller]")]
public class GameController : ControllerBase
{
    private List<Game> gameList;
    private List<Question> availableQuestions;

    private IGameInMemoryService gameService;

    /// <summary>
    /// Game Controller
    /// </summary>
    /// <param name="gameInMemoryService"></param>
    /// <param name="questionInMemoryService"></param>
    public GameController(IGameInMemoryService gameInMemoryService)
    {
        gameService = gameInMemoryService;
    }

    /// <summary>
    /// Create a new game
    /// </summary>
    /// <param name="gameRequest">Info require for a game: type, players</param>
    /// <returns>Game created info with player order</returns>
    [HttpPost("Game")]
    public async Task<IActionResult> CreateGame([FromBody] GameRequest gameRequest)
    {
        Game game = await gameService.CreateGame(gameRequest);

        return CreatedAtAction(nameof(GetGame), new { gameId = game.Id }, game);
    }

    /// <summary>
    /// Return the next turn for a game or the current turn if one is already started
    /// </summary>
    /// <param name="gameId">Game id GUID</param>
    /// <returns>Turno Info</returns>
    [HttpGet("Game/{gameId}/Turn")]
    public async Task<IActionResult> NextTurn(Guid gameId)
    {
        var result = await gameService.CreateNextTurn(gameId);

        return result.Match<IActionResult>(
            turn => Ok(turn),
            notFoundException => NotFound(notFoundException.message)
            );
    }

    /// <summary>
    /// Response for a Turn with a answer for the question
    /// </summary>
    /// <param name="gameId">Game Id GUID</param>
    /// <param name="turnId">Turn Id int</param>
    /// <param name="answerId">Answer id GUID</param>
    /// <returns>Verification if the answer is right and new position of the player</returns>
    [HttpPost("Game/{gameId}/Turn/{turnId}")]
    public async Task<IActionResult> AnswerTurn(Guid gameId, int turnId, [FromBody] char selector)
    {
        AnsweredTurn answeredTurn = new AnsweredTurn(gameId, turnId, selector);

        var result = await gameService.AnswerTurn(answeredTurn);

        return result.Match<IActionResult>(
            turn => Ok(turn),
            notFoundException => NotFound(notFoundException.message)
            );
    }

    /// <summary>
    /// Return game status
    /// </summary>
    /// <param name="gameId">Game Id GUID</param>
    /// <returns>Current game Info</returns>
    [HttpGet("Game/{gameId}")]
    public async Task<IActionResult> GetGame(Guid gameId)
    {
        var result = await gameService.GetGame(gameId);

        return result.Match<IActionResult>(
          game => Ok(game),
          notFoundException => NotFound(notFoundException.message)
          );
    }
}