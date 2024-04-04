using gameQnA.Dto;
using gameQnA.Enums;

namespace gameQnA.Services
{
    public interface IQuestionInMemoryService
    {
        List<Question> GetQuestions();

        Question AddQuestion(QuestionRequest request, QuestionResponse question);
    }

    /// <summary>
    /// Question service
    /// </summary>
    public class QuestionInMemoryService : IQuestionInMemoryService
    {
        private List<Question> questions;

        /// <summary>
        /// Constructor
        /// </summary>
        public QuestionInMemoryService()
        {
            questions = new List<Question>();
            Answer RightAnswer = new Answer { Id = Guid.NewGuid(), Body = "Pingüino" };

            Question question = new Question
            {
                Id = Guid.NewGuid(),
                QuestionCategory = QuestionCategory.sport,
                PlayerCategory = PlayerCategory.Preschool,
                Body = "¿Cual de los siguientes animales es un ave?",
                RighAnswerId = RightAnswer.Id,
                Answers = new List<Answer> { RightAnswer, new Answer { Id = Guid.NewGuid(), Body = "Ratón" }, new Answer { Id = Guid.NewGuid(), Body = "Lagartija" }, new Answer { Id = Guid.NewGuid(), Body = "Zorro" }
            }
            };
            questions.Add(question);
        }

        /// <summary>
        /// Return the current list of questions
        /// </summary>
        /// <returns></returns>
        public List<Question> GetQuestions()
        {
            return questions;
        }

        /// <summary>
        /// Add a new question
        /// </summary>
        /// <param name="request"></param>
        /// <param name="question"></param>
        /// <returns></returns>
        public Question AddQuestion(QuestionRequest request, QuestionResponse question)
        {
            Question newQuestion = new Question()
            {
                Id = Guid.NewGuid()
            };
            newQuestion.QuestionCategory = Enum.TryParse(request.topic.ToLower(), true, out QuestionCategory qCategory) ? qCategory : QuestionCategory.neutral;
            newQuestion.PlayerCategory = Enum.TryParse(request.personType.ToLower(), true, out PlayerCategory pCategory) ? pCategory : PlayerCategory.HighSchool;
            newQuestion.Body = question.question;
            newQuestion.Answers = question.options.Select(o => new Answer() { Id = Guid.NewGuid(), Body = o.body, Selector = o.selector }).ToList();
            newQuestion.RighAnswerId = newQuestion.Answers.First(a => a.Selector == question.rightAnswer).Id;
            newQuestion.context = question.context;
            newQuestion.AdditionalSquares = 1;
            questions.Add(newQuestion);
            return newQuestion;
        }
    }
}