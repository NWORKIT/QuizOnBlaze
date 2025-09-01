using QuizOnBlaze.Components.Pages.Player;
using QuizOnBlaze.Services;

namespace QuizOnBlaze.Models
{
    public class GameSessionModel
    {
        /// <summary>
        /// Game Session ID
        /// </summary>
        public string SessionId { get; set; } = Guid.NewGuid().ToString();

        public List<QuestionModel> Questions { get; set; } = new();

        public int CurrentQuestionIndex { get; set; } = 0;

        /// <summary>
        /// Saves the status of players registered in this session
        /// </summary>
        public List<PlayerModel> Players { get; set; } = new();

        /// <summary>
        /// DON'T USE: this structure will not save the old answers.
        /// Saves player responses to the current question, per player
        /// </summary>
        //public Dictionary<Guid, PlayerAnswerInfoModel> CurrentAnswer { get; set; } = new Dictionary<Guid, PlayerAnswerInfoModel>();


        /// <summary>
        /// To control when to show question
        /// </summary>
        public Dictionary<int, Dictionary<Guid, PlayerAnswerInfoModel>> AnswersByQuestion { get; set; } = new();


        /// <summary>
        /// Groups responses by question.
        /// External key = question index
        /// Internal key = Player ID
        /// </summary>
        public bool IsQuestionVisible { get; set; } = false;

        /// <summary>
        /// To control when to show correct answer
        /// </summary>
        public bool IsAnswerVisible { get; set; } = false;

        /// <summary>
        /// Game Pin
        /// </summary>
        public string? GamePin { get; set; }


        /// <summary>
        /// Session creation date and time
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Date/time of last session update
        /// </summary>
        public DateTime LastUpdatedAt { get; set; }


        /// <summary>
        /// So that the question options do not appear before the question
        /// </summary>
        public bool IsQuestionActive { get; set; }


        /// <summary>
        /// Historical response count per question.
        /// </summary>
        public Dictionary<int, int> AnswerCountsByQuestion { get; set; } = new();



    }
}
