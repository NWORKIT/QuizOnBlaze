using Newtonsoft.Json;

namespace QuizOnBlaze.Models
{
    public class QuestionModel
    {
        [JsonProperty("QuestionText")]
        public string Text { get; set; } = string.Empty;

        [JsonProperty("QuestionImage")]
        public string ImageUrl { get; set; } = string.Empty;

        [JsonProperty("QuestionOptions")]
        public List<string> Options { get; set; } = new List<string>();

        [JsonProperty("QuestionCorrectAnswer")]
        public int CorrectOptionIndex { get; set; }
    }
}
