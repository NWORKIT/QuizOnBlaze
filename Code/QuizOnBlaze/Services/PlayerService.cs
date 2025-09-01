using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;

namespace QuizOnBlaze.Services
{
    public class PlayerService
    {
        private readonly List<PlayerModel> _players = new();

        private readonly string _filePath;

        //  multithread sync
        private readonly object _locker = new();

        public PlayerService(string filePath)
        {
            _filePath = filePath ?? throw new ArgumentNullException(nameof(filePath));

            // Ensure directory exists
            var directory = Path.GetDirectoryName(_filePath);
            if (!string.IsNullOrEmpty(directory))
                Directory.CreateDirectory(directory);

            LoadPlayers();
        }

        private void LoadPlayers()
        {
            if (File.Exists(_filePath))
            {
                var json = File.ReadAllText(_filePath);
                var players = JsonConvert.DeserializeObject<List<PlayerModel>>(json);
                if (players != null)
                    _players.AddRange(players);
            }
        }

        public IReadOnlyList<PlayerModel> Players => _players.AsReadOnly();

        /// <summary>
        /// Adds a new player or updates existing player's score.
        /// </summary>
        public void AddOrUpdatePlayer(string playerName, int score)
        {
            lock (_locker)
            {
                if (string.IsNullOrWhiteSpace(playerName))
                    throw new ArgumentException("Player name must be provided.", nameof(playerName));

                var cleanedName = playerName.Trim();

                var player = _players.FirstOrDefault(p => p.Name.Equals(cleanedName, StringComparison.OrdinalIgnoreCase));
                if (player == null)
                {
                    player = new PlayerModel { Name = playerName, Score = score };
                    _players.Add(player);
                }
                else
                {
                    player.Score = score;
                }

                SavePlayers();
            }
        }

        private void SavePlayers()
        {
            lock (_locker)
            {
                var json = JsonConvert.SerializeObject(_players, Formatting.Indented);
                File.WriteAllText(_filePath, json);
            }
        }

        /// <summary>
        /// Returns the top players ordered by score descending.
        /// </summary>
        public IEnumerable<PlayerModel> GetTopPlayers(int count)
        {
            return _players.OrderByDescending(p => p.Score).Take(count);
        }

        /// <summary>
        /// Resets all players' scores to zero and persists changes.
        /// </summary>
        public void ResetScores()
        {
            foreach (var player in _players)
            {
                player.Score = 0;
            }
            SavePlayers();
        }


    }
}
