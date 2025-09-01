namespace QuizOnBlaze.Models
{
    /// <summary>
    /// PlayerStateService state serialization
    /// </summary>
    public class PlayerStateSnapshotModel
    {
        public Guid? Id { get; set; }
        public string? Name { get; set; }
        public string? SessionPin { get; set; }
        public string? AvatarSeed { get; set; }
    }
}
