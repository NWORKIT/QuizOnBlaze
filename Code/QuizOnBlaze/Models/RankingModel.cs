namespace QuizOnBlaze.Models
{
    public class RankingModel
    {
        public string GamePin { get; set; }

        public Guid PlayerID { get; set; }

        public string Name { get; set; }

        public int Score { get; set; }

        public int Possition { get; set; }
    }
}
