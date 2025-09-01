namespace QuizOnBlaze.Services
{
    public class AvatarService
    {
        private const string BaseUrl = "https://api.dicebear.com/9.x/bottts/svg?seed=";

        public string GetAvatarUrl(string seed)
        {
            return $"{BaseUrl}{Uri.EscapeDataString(seed)}";
        }

        public string GenerateRandomSeed()
        {
            return Guid.NewGuid().ToString();
        }

    }

}
