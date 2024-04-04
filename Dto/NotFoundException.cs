namespace gameQnA.Dto
{
    /// <summary>
    /// Represent a Not found exception in the services
    /// </summary>
    public class NotFoundException
    {
        /// <summary>
        /// Message to return
        /// </summary>
        public string message { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="message"></param>
        public NotFoundException(string message)
        {
            this.message = message;
        }
    }
}