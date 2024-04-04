using gameQnA.Enums;

namespace gameQnA.Dto
{
    public class Question
    {
        public Guid Id { get; set; }
        public QuestionCategory QuestionCategory { get; set; }

        public PlayerCategory PlayerCategory { get; set; }

        public int AdditionalSquares { get; set; }

        public string Body { get; set; }

        public List<Answer> Answers { get; set; }

        public Guid RighAnswerId { get; set; }

        public string context { get; set; }

        public Question()
        {
            Answers = new List<Answer>();
        }
    }
}