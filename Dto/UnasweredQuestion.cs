namespace gameQnA.Dto
{
    public class UnasweredQuestion
    {
        public Guid Id { get; set; }
        public string Body { get; set; }

        public List<Answer> Answers { get; set; }

        public UnasweredQuestion()
        {
            Answers = new List<Answer>();
        }
    }
}