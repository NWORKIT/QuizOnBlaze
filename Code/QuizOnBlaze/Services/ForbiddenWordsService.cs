using System.Text.RegularExpressions;

namespace QuizOnBlaze.Services
{
    public class ForbiddenWordsService
    {
        private string[] _forbiddenWords;

        public ForbiddenWordsService(string dataFolderPath)
        {
            var filePath = Path.Combine(dataFolderPath, "forbidden_names.txt");

            if (File.Exists(filePath))
            {
                _forbiddenWords = File.ReadAllLines(filePath)
                    .Where(line => !string.IsNullOrWhiteSpace(line))
                    .Select(line => line.Trim().ToLowerInvariant())
                    .ToArray();
            }
            else
            {
                _forbiddenWords = Array.Empty<string>();
            }
        }

        public bool IsNameAllowed(string name)
        {
            var lowerName = name.ToLowerInvariant();

            // Check if any forbidden name is contained in the name
            foreach (var forbidden in _forbiddenWords)
            {
                // Exact word, separated by word boundaries
                var pattern = Regex.Escape(forbidden);
                if (Regex.IsMatch(lowerName, pattern, RegexOptions.IgnoreCase))
                {
                    return false;
                }
            }
            return true;
        }
    }
}
