using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using QuizOnBlaze.Components.Pages.Player;
using QuizOnBlaze.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace QuizOnBlaze.Services
{
    public class SessionManager
    {
        // Colection for Session ID
        private readonly Dictionary<string, GameSessionModel> _sessionsById = new();
        
        // Collection for Game PinD
        private readonly Dictionary<string, GameSessionModel> _sessionsByPin = new();

        private readonly string _basePath;

        private static Random _random = new Random();

        private readonly ILogger<SessionManager> _logger;

        private static readonly object _fileLock = new();


        public SessionManager(string basePath, ILogger<SessionManager> logger)
        {
            _basePath = basePath;
            Directory.CreateDirectory(_basePath);
            _logger = logger;
        }


        // Event to notify whenever a session changes.
        public event Action? SessionsChanged;

        private void OnSessionsChanged()
        {
            SessionsChanged?.Invoke();
        }

        // Event to nottify when the player list changes from CurrentSession.
        public event Action? PlayersChanged;

        private void OnPlayersChanged()
        {
            PlayersChanged?.Invoke();
        }


        public GameSessionModel CreateSession(List<QuestionModel> questions)
        {
            var session = new GameSessionModel
            {
                SessionId = Guid.NewGuid().ToString(),
                GamePin = _random.Next(10000, 99999).ToString(),
                Questions = questions,
                CurrentQuestionIndex = 0,
                Players = new List<PlayerModel>(),
                CreatedAt = DateTime.UtcNow,
                LastUpdatedAt = DateTime.UtcNow
            };

            // Writes a reference to the session object using the SessionId key.
            _sessionsById[session.SessionId] = session;

            // Writes another reference (same object in memory) using the GamePin key.
            _sessionsByPin[session.GamePin] = session;

            SaveSession(session);
            return session;
        }

        public GameSessionModel? GetSession(string sessionId)
        {
            if (_sessionsById.TryGetValue(sessionId, out var session))
                return session;

            var path = GetSessionFilePath(sessionId);
            if (File.Exists(path))
            {
                var json = File.ReadAllText(path);
                var loadedSession = JsonConvert.DeserializeObject<GameSessionModel>(json);
                if (loadedSession != null)
                {
                    _sessionsById[sessionId] = loadedSession;
                    return loadedSession;
                }
            }
            return null;
        }

        public GameSessionModel? GetSessionByPin(string pin)
        {
            _sessionsByPin.TryGetValue(pin, out var session);
            return session;
        }

        public void SaveSession(GameSessionModel session)
        {
            lock (_fileLock)
            {
                session.LastUpdatedAt = DateTime.UtcNow;
                var path = GetSessionFilePath(session.SessionId);
                var json = JsonConvert.SerializeObject(session, Formatting.Indented);
                File.WriteAllText(path, json);
            }
        }

        private string GetSessionFilePath(string sessionId)
        {
            return Path.Combine(_basePath, $"{sessionId}.json");
        }

        



        /// <summary>
        /// Load questions from JSON file
        /// </summary>
        /// <param name="questionsFilePath"></param>
        /// <returns>List of questions</returns>
        /// <exception cref="FileNotFoundException"></exception>
        public async Task<List<QuestionModel>> LoadQuestionsFromFileAsync(string questionsFilePath = "data/questions.json")
        {
            if (!File.Exists(questionsFilePath))
                throw new FileNotFoundException("Questions file not found.", questionsFilePath);

            using var reader = new StreamReader(questionsFilePath);
            var json = await reader.ReadToEndAsync();
            var questions = JsonConvert.DeserializeObject<List<QuestionModel>>(json);
            return questions ?? new List<QuestionModel>();
        }


        public void AddPlayerToSession(string pin, PlayerModel player)
        {
            var session = GetSessionByPin(pin);
            if (session != null)
            {
                session.Players.Add(player);
                SaveSession(session);

                // Triggers a event to notify that sessions have changed
                OnSessionsChanged();

                // Triggers a specific event for the player list
                OnPlayersChanged();
            }
        }

        public PlayerModel GetPlayerModelFromSession(string pin, string playerName)
        {
            var session = GetSessionByPin(pin);
            if (session != null)
            {
                var playerModel = from p in session.Players
                    where p.Name == playerName
                    select p;

                return (PlayerModel)playerModel;


            }

            return null;
        }



        /// <summary>
        /// Gets all active sessions in memory.
        /// </summary>
        /// <returns>List of active sessions in memory.</returns>
        public IReadOnlyCollection<GameSessionModel> GetAllSessions() => _sessionsById.Values.ToList().AsReadOnly();


        /// <summary>
        /// Load all sessions stored in JSON.
        /// </summary>
        public void LoadAllSessions()
        {
            // Clear old sessions
            _sessionsById.Clear();
            _sessionsByPin.Clear();

            var sessionFiles = Directory.GetFiles(_basePath, "*.json");
            foreach (var file in sessionFiles)
            {
                var json = File.ReadAllText(file);
                var loadedSession = JsonConvert.DeserializeObject<GameSessionModel>(json);
                if (loadedSession != null)
                {
                    _sessionsById[loadedSession.SessionId] = loadedSession;

                    if (!string.IsNullOrEmpty(loadedSession.GamePin))
                    {
                        _sessionsByPin[loadedSession.GamePin] = loadedSession;
                    }

                }
            }
        }


        public bool RemoveSession(string sessionId)
        {
            if (!_sessionsById.TryGetValue(sessionId, out var session))
            {
                _logger.LogInformation("Removing session {sessionId} from the collection.", sessionId);
                return false;
            }
           

            bool removedById = _sessionsById.Remove(sessionId);
            bool removedByPin = false;


            if (!string.IsNullOrEmpty(session.GamePin))
            {
                removedByPin = _sessionsByPin.Remove(session.GamePin);
            }



            if (removedById)
            {
                _sessionsByPin.Remove(sessionId);

                _logger.LogInformation("Session {sessionId} removed from collection.", sessionId);

                var path = GetSessionFilePath(sessionId);
                if (File.Exists(path))
                {
                    File.Delete(path);
                }

                // Invoke event to notify changes
                SessionsChanged?.Invoke();

            }
            else
            {
                _logger.LogInformation("Failed to remove session {sessionId} from the collection.", sessionId);
            }

            return removedById;

        }


        public void RegisterPlayerAnswer(string gamePin, Guid playerId, int questionIndex, string answer, bool isCorrect, int pointsEarned)
        {
            var session = GetSessionByPin(gamePin);
            if (session == null)
                return;


            // Ensures there is collection for the current issue
            if (!session.AnswersByQuestion.ContainsKey(questionIndex))
            {
                session.AnswersByQuestion[questionIndex] = new Dictionary<Guid, PlayerAnswerInfoModel>();
            }

            // Records the player answer to the current question
            session.AnswersByQuestion[questionIndex][playerId] = new PlayerAnswerInfoModel
            {
                QuestionIndex = questionIndex,
                Answer = answer,
                IsCorrect = isCorrect,
                PointsEarned = pointsEarned,
                AnsweredAt = DateTime.UtcNow
            };

            // Updates the player score
            AddScore(session, playerId, pointsEarned);

            SaveSession(session); // ensures immediate persistence in JSON
            OnSessionsChanged();
        }


        // Updates the permanent player score
        private void AddScore(GameSessionModel session, Guid playerId, int points)
        {
            var player = session.Players.FirstOrDefault(p => p.Id == playerId);
            if (player != null)
            {
                player.Score += points;
            }
        }
    }
}
