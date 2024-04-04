namespace gameQnA.Dto
{
    /// <summary>
    /// Represent a Question for ChatGPT
    /// </summary>
    public class QuestionRequest
    {
        public string topic { get; set; }
        public string personType { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="topic"></param>
        /// <param name="personType"></param>
        public QuestionRequest(string topic, string personType)
        {
            this.topic = topic;
            this.personType = personType;
        }
    }
}