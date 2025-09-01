namespace QuizOnBlaze.Models
{
    public class PlayerAnswerInfoModel
    {
        public int QuestionIndex { get; set; }
        public string? Answer { get; set; }
        public bool IsCorrect { get; set; }

        // If the date is null it means the player did not answer the question
        public DateTime? AnsweredAt { get; set; }

        public int PointsEarned { get; set; }
    }
}
