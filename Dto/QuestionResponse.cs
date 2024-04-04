namespace gameQnA.Dto
{
    public class QuestionResponse
    {
        public string question { get; set; }
        public QuestionOption[] options { get; set; }
        public char rightAnswer { get; set; }
        public string context { get; set; }
    }
}