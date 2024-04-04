using gameQnA.Dto;
using Newtonsoft.Json;
using OpenAI_API;
using OpenAI_API.Chat;
using System.Text;

namespace gameQnA.Services
{
    public interface IOpenAIService
    {
        Task<QuestionResponse> GetQuestion(QuestionRequest request);
    }

    public class OpenAIService : IOpenAIService
    {
        private Conversation chat = null;

        public OpenAIService(IConfiguration configuration)
        {
            OpenAIAPI api = new OpenAIAPI(configuration["OPENAI_API_KEY"]); //Get open IA key from secrets

            chat = api.Chat.CreateConversation();     //create chat bot
            var context = SetUpContext();
            chat.AppendSystemMessage(context);     //set up context for the chat bot
        }

        public async Task<QuestionResponse> GetQuestion(QuestionRequest request)
        {
            var json = JsonConvert.SerializeObject(request);
            chat.AppendUserInput(json);
            string response = await chat.GetResponseFromChatbotAsync();
            return JsonConvert.DeserializeObject<QuestionResponse>(response);
        }

        private string SetUpContext()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("Generate a question in Spanish from the information provided by the user that will indicate a topic, for example \"art\", ")
               .Append("and a type of person to whom it is addressed, for example \"preschool\" or \"college\". With this information, ")
              .Append("you must formulate one question in Spanish at a time, taking into account the topic and the type of person to whom it is addressed, ")
              .Append("offering four possible answers, also in Spanish, including only one correct answer with the rest of incorrect options, all in random positions, ")
              .Append("and must also indicate which is the correct answer and add a maximum of a couple of lines as context, explaining or adding ")
              .Append("information about the answer, all in Spanish. All communication will be in JSON format, do not add anything outside the JSON format. For example: ")
              .AppendLine("The user sends ")
              .Append("{ ")
              .Append("\"Topic\":  \"science\",")
              .Append("\"PersonType\":  \"preschool\"")
              .AppendLine("} ")
              .AppendLine("You answer: ")
              .Append("{ ")
              .Append("\"Question\":\" ¿Cual de estos animales es un ave?\",")
              .Append("\"Options\": [ ")
              .Append("{ ")
              .Append("\"Selector\":\"A\",")
              .Append("\"Body\": \"Perro\"},")
              .Append("{ ")
              .Append("\"Selector\":\"B\",")
              .Append("\"Body\": \"Gato\"},")
              .Append("{ ")
              .Append("\"Selector\":\"C\",")
              .Append("\"Body\": \"Loro\"},")
              .Append("{ ")
              .Append("\"Selector\":\"D\",")
              .Append("\"Body\": \"Delfin\"}")
              .Append("],")
              .Append("\"RightAnswer\": \"C\"")
              .Append("\"Context\": \"El Loro es un ave que habita en muchas zonas del mundo\" }")
              .AppendLine("} ")
              .AppendLine("Another example, the user sends: ")
              .Append("{ ")
              .Append("\"Topic\": \"Art\", ")
              .Append("\"PersonType\": \"College\" ")
              .AppendLine("} ")
              .AppendLine("You answer: ")
              .Append("{ ")
              .Append("\"Question\": \"¿Quien pintó La Gioconda?\", ")
              .Append("\"Options\": [ ")
              .Append("{\"Selector\":\"A\", ")
              .Append("\"Body\": \"Miguel Angel\"}, ")
              .Append("{ ")
              .Append("\"Selector\":\"B\", ")
              .Append("\"Body\": \"Leonardo DaVinci \"}, ")
              .Append("{ ")
              .Append("\"Selector\":\"C\", ")
              .Append("\"Body\": \"Rafael Sanzio\"}, ")
              .Append("{ ")
              .Append("\"Selector\":\"D\", ")
              .Append("\"Body\": \"Sandro Botticelli\"} ")
              .Append("], ")
              .Append("\"RightAnswer\":\"B\" ")
              .Append(" \"Context\":\" La Gioconda es un óleo sobre tabla de álamo de 79 × 53 cm, pintado entre 1503 y 1519 ")
              .Append("por Leonando DaVinci. Se Cree es el retrato de Lisa Gherardini, esposa de Francesco del Giocondo.\" ")
              .AppendLine("} ")
              .Append("The questions, options and context you provide must always be in the Spanish language, even if the user ")
              .Append("provides the subject and personType in English. The personTypes can be Preschool from 0 to 5 years, ")
              .Append("ElementarySchool from 5 to 11 years, MiddleSchool from 11 to 14 years, HighSchool from 14 to 19 years, and College ")
              .Append("over 19 years, take this into account when formulating the appropriate question with a complexity according to the ")
              .Append("personType and also that the question generated must be according to the topic provided by the user.")
              .Append("Wait for the user to make a request before generating the question.");

            return sb.ToString();
            /*
            string x =
            "Genera una pregunta basada en la información que proporciona el usuario quien te va indicar un tema, " +
            "por ejemplo \"Arte\", y un tipo de persona de a quién va dirigida la pregunta, por ejemplo \"Preescolar\" o \"Adulto\".Con esa información," +
            "debes formular una pregunta por vez, tomando en cuenta el tema y el tipo de persona a quien va dirigida ofreciendo cuatro posibles respuestas," +
            "incluyendo solo una única respuesta correcta con las demás opciones incorrectas, todas en posiciones aleatorias, ademas debes indicar " +
            "cual es la respuesta correcta y añadir un máximo de un par de líneas a modo de contexto, explicando o añadiendo información de la respuesta. " +
            "Toda la comunicación será en formato JSON.No añadas nada fuera del formato JSON. " +
            "Por ejemplo: " +
            "el usuario envía: " +
            "{ " +
            "\"Topic\":  \"Ciencia\"," +
            "\"PersonType\":  \"Preescolar\"" +
            "} " +
            "Tú respondes: " +
            "{ " +
            "\"Question\":\" ¿Cual de estos animales es un ave?\"," +
            "\"Options\": [ " +
            "{ " +
            "\"Selector\":\"A\"," +
            "\"Body\": \"Perro\"}," +
            "{ " +
            "\"Selector\":\"B\"," +
            "\"Body\": \"Gato\"}," +
            "{ " +
            "\"Selector\":\"C\"," +
            "\"Body\": \"Loro\"}," +
            "{ " +
            "\"Selector\":\"D\"," +
            "\"Body\": \"Delfin\"}" +
            "]," +
            "\"RightAnswer\": \"C\"" +
            "\"Context\": \"El Loro es un ave que habita en muchas zonas del mundo\" }" +
            "} " +
            "Otro ejemplo: " +
            "El usuario envia " +
            "{ " +
            "\"Topic\": \"Arte\", " +
            "\"PersonType\": \"Adulto\" " +
            "} " +
            "Tu respondes:  " +
            "{ " +
            "\"Question\": \"¿Quien pintó La Gioconda?\", " +
            "\"Options\": [ " +
            "{\"Selector\":\"A\", " +
            "\"Body\": \"Miguel Angel\"}, " +
            "{ " +
            "\"Selector\":\"B\", " +
            "\"Body\": \"Leonardo DaVinci \"}, " +
            "{ " +
            "\"Selector\":\"C\", " +
            "\"Body\": \"Rafael Sanzio\"}, " +
            "{ " +
            "\"Selector\":\"D\", " +
            "\"Body\": \"Sandro Botticelli\"} " +
            "], " +
            "\"RightAnswer\":\"B\" " +
            " \"Context\":\" La Gioconda es un óleo sobre tabla de álamo de 79 × 53 cm, pintado entre 1503 y 1519 por Leonando DaVinci. Se Cree es el retrato de Lisa Gherardini, esposa de Francesco del Giocondo.\" " +
            "} " +
            "Las preguntas que generes siembre deben estar en el idiomaa español, aun cuando el usuario provee el topic y el personType en inglés. " +
            "Espera a que el usuario haga su peticion antes de generar la pregunta. ";
            */
        }
    }
}