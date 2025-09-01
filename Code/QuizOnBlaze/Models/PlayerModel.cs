namespace QuizOnBlaze.Services
{
    /// <summary>
    /// Represents a player in the quiz
    /// </summary>
    public class PlayerModel
    {

        /// <summary>
        /// Unique identifier for the player
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Player's name
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Current score of the player
        /// </summary>
        public int Score { get; set; } = 0;

        /// <summary>
        /// Player generated avatar
        /// </summary>
        public string AvatarSeed { get; set; } = string.Empty;



        /// <summary>
        /// Indicates if the player is active in the current round
        /// </summary>
        /// 
        public bool IsActive { get; set; }
    }
}
