using Microsoft.AspNetCore.SignalR;
using QuizOnBlaze.Components.Pages.Player;
using QuizOnBlaze.Models;
using QuizOnBlaze.Services;
using System.Collections.Concurrent;
using System.Diagnostics.Metrics;
using System.Numerics;
using System.Threading.Tasks;


namespace QuizOnBlaze.Hubs
{
    public class QuizHub : Hub
    {

        private readonly SessionManager _sessionManager;

        // Player connection
        private static readonly Dictionary<Guid, string> _connections = new();

        public QuizHub(SessionManager sessionManager)
        {
            _sessionManager = sessionManager;
        }

        public override async Task OnConnectedAsync()
        {
            var httpContext = Context.GetHttpContext();
            var gamePin = httpContext.Request.Query["gamePin"].ToString();
            var playerIdStr = httpContext.Request.Query["playerId"].ToString();

            // Player connection Id
            if (Guid.TryParse(playerIdStr, out Guid playerId))
            {
                _connections[playerId] = Context.ConnectionId;
            }


            if (!string.IsNullOrEmpty(gamePin))
            {
                // Players group
                await Groups.AddToGroupAsync(Context.ConnectionId, gamePin);

                // Admin group
                if (gamePin.EndsWith("_admin"))
                {
                    await Groups.AddToGroupAsync(Context.ConnectionId, gamePin);
                }
            }

            await base.OnConnectedAsync();
        }


        // Removing the client on disconnection
        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var item = _connections.FirstOrDefault(x => x.Value == Context.ConnectionId);
            if (!item.Equals(default(KeyValuePair<Guid, string>)))
            {
                _connections.Remove(item.Key);
            }
            await base.OnDisconnectedAsync(exception);
        }


        public async Task Admin_SendLoadingWithNumber(string gamePin, int questionNumber)
        {
            await Clients.Group(gamePin).SendAsync("Player_ReceiveLoadingWithNumber", questionNumber);
        }


        // Notify current question for the player
        public async Task Admin_SendCurrentQuestion(string gamePin, QuestionModel question)
        {
            var session = _sessionManager.GetSessionByPin(gamePin);
            if (session == null) return;

            int idx = session.CurrentQuestionIndex;

            // Initialize collections if it doesn't exist
            if (!session.AnswersByQuestion.ContainsKey(idx))
                session.AnswersByQuestion[idx] = new Dictionary<Guid, PlayerAnswerInfoModel>();


            if (!session.AnswerCountsByQuestion.ContainsKey(idx))
                session.AnswerCountsByQuestion[idx] = 0;

            // WARNING: This will clear all answers 
            //session.CurrentAnswer.Clear();

            session.IsQuestionActive = true;

            _sessionManager.SaveSession(session); // Persistence

            // Sends the new question to all players (SignalR client) in the existing session
            var currentQuestion = session.Questions[idx];
            await Clients.Group(gamePin).SendAsync("Player_ReceiveQuestion", currentQuestion);

            // Update counter in admin (may already contain previous responses if they already exist)
            await Clients.Group(gamePin + "_admin").SendAsync("Admin_UpdateAnswerCount", session.AnswersByQuestion[idx].Count);


        }



        public async Task Admin_NavigateToQuestion(string gamePin, int targetIndex)
        {
            var session = _sessionManager.GetSessionByPin(gamePin);
            if (session == null) return;

            // Atualiza índice
            session.CurrentQuestionIndex = targetIndex;

            // Apenas cria as entradas se não existirem ainda
            if (!session.AnswersByQuestion.ContainsKey(targetIndex))
                session.AnswersByQuestion[targetIndex] = new Dictionary<Guid, PlayerAnswerInfoModel>();

            if (!session.AnswerCountsByQuestion.ContainsKey(targetIndex))
                session.AnswerCountsByQuestion[targetIndex] = 0;

            // Define estado
            session.IsQuestionActive = false; // ou true se for abrir
            _sessionManager.SaveSession(session);

            var currentQuestion = session.Questions[targetIndex];

            // Notifica todos os jogadores
            await Clients.Group(gamePin).SendAsync("Player_ReceiveQuestion", currentQuestion);

            // Notifica admins do novo contador
            await Clients.Group(gamePin + "_admin")
                         .SendAsync("Admin_UpdateAnswerCount", session.AnswersByQuestion[targetIndex].Count);
        }




        public async Task Player_SubmitAnswer(string gamePin, Guid playerId, string answer, int timeTakenSeconds)
        {
            if (gamePin == null)
                return;

            var session = _sessionManager.GetSessionByPin(gamePin);
            if (session == null || !session.IsQuestionActive)
                return; // Blocks player responses when time runs out.

            int currentQuestion = session.CurrentQuestionIndex;
            
            if (!session.AnswersByQuestion.ContainsKey(currentQuestion))
                session.AnswersByQuestion[currentQuestion] = new Dictionary<Guid, PlayerAnswerInfoModel>();

            var answersForQuestion = session.AnswersByQuestion[currentQuestion];

            // Checks if the player has already answered the question
            if (answersForQuestion.ContainsKey(playerId))
                return;


            // Number of responses received by the players
            if (!session.AnswerCountsByQuestion.ContainsKey(currentQuestion))
                session.AnswerCountsByQuestion[currentQuestion] = 0;
            session.AnswerCountsByQuestion[currentQuestion]++;


            int pointsEarned = EvaluateAnswer(currentQuestion, answer, timeTakenSeconds, session);

            bool isCorrect = pointsEarned > 0;

            answersForQuestion[playerId] = new PlayerAnswerInfoModel
            {
                QuestionIndex = currentQuestion,
                Answer = answer,
                IsCorrect = isCorrect,
                PointsEarned = pointsEarned,
                AnsweredAt = DateTime.UtcNow
            };

            // Update score
            session.Players.First(p => p.Id == playerId).Score += pointsEarned;

            _sessionManager.SaveSession(session);

            // Update to the number of responses received from players
            await Clients.Group(gamePin + "_admin").SendAsync("Admin_UpdateAnswerCount", answersForQuestion.Count);

            // Update ranking
            var ranking = CalculateRanking(session);
            await Clients.Group(gamePin + "_admin").SendAsync("Admin_ReceiveUpdateRanking", ranking);

            // Sends the question score to the player
            await Clients.Caller.SendAsync("Player_ReceiveQuestionScoreIndividual", isCorrect, pointsEarned);
        }

      

        // Receives the player response and generates the score
        private int EvaluateAnswer(int questionIndex, string givenAnswer, int timeTakenSeconds, GameSessionModel session)
        {
            // Get the correct answer to the question
            var correctAnswer = session.Questions[questionIndex].CorrectOptionIndex;

            if (!string.Equals(givenAnswer, correctAnswer.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                // Incorrect answer earns 0 points
                return 0;
            }

            // Calculates score based on response time
            // 15 seconds = 150 points, lose 10 points per second
            int maxPoints = 150;
            int penaltyPerSecond = 10;

            // Limit time to a maximum of 15 seconds
            if (timeTakenSeconds > 15)
                timeTakenSeconds = 15;

            int points = maxPoints - (penaltyPerSecond * timeTakenSeconds);
            return points < 0 ? 0 : points;
        }


        /// <summary>
        /// Method invoked by the client to get the current session state.
        /// Sends the current question and other necessary information to the client.
        /// </summary>
        /// <param name="gamePin">Game Pin</param>
        /// <param name="playerId">Player ID</param>
        public async Task Player_RequestCurrentState(string gamePin, string playerId)
        {
            var session = _sessionManager.GetSessionByPin(gamePin);
            if (session == null)
            {
                await Clients.Caller.SendAsync("SessionNotFound");
                return;
            }


            if (!session.IsQuestionActive)
            {
                // Sends loading waiting for the question to start
                await Clients.Caller.SendAsync("Player_ReceiveLoadingWithNumber", session.CurrentQuestionIndex);
                return;
            }

            var currentQuestionIndex = session.CurrentQuestionIndex;
            QuestionModel? currentQuestion = null;

            if (currentQuestionIndex >= 0 && currentQuestionIndex < session.Questions.Count)
            {
                currentQuestion = session.Questions[currentQuestionIndex];
            }

            // Check if the player has already answered this question
            PlayerAnswerInfoModel? playerAnswer = null;
            if (Guid.TryParse(playerId, out var parsedPlayerId))
            {
                if (session.AnswersByQuestion.ContainsKey(currentQuestionIndex))
                {
                    session.AnswersByQuestion[currentQuestionIndex]
                           .TryGetValue(parsedPlayerId, out playerAnswer);
                }
            }

            // Sends the current question + answer status to the player
            await Clients.Caller.SendAsync("Player_ReceiveCurrentState", currentQuestion, currentQuestionIndex, playerAnswer);
        }



        public async Task Admin_SendQuestionResults(string gamePin)
        {
            var session = _sessionManager.GetSessionByPin(gamePin);
            if (session == null) return;

            var counts = new Dictionary<int, int>();

            // Fetching answers to the current question
            if (session.AnswersByQuestion.ContainsKey(session.CurrentQuestionIndex))
            {
                foreach (var ans in session.AnswersByQuestion[session.CurrentQuestionIndex].Values)
                {
                    if (int.TryParse(ans.Answer, out int optionIdx))
                    {
                        if (!counts.ContainsKey(optionIdx))
                            counts[optionIdx] = 0;

                        counts[optionIdx]++;
                    }
                }
            }

            int correctIndex = session.Questions[session.CurrentQuestionIndex].CorrectOptionIndex;

            // Send results to admin
            await Clients.Group(gamePin + "_admin").SendAsync("Admin_ReceiveQuestionResults", counts, correctIndex);

            
        }



        // Question Ended + Also defines players who did not respond
        public async Task Admin_SendQuestionEnded(string gamePin)
        {
            var session = _sessionManager.GetSessionByPin(gamePin);
            if (session == null) return;


            session.IsQuestionActive = false;

            if (!session.AnswersByQuestion.ContainsKey(session.CurrentQuestionIndex))
                session.AnswersByQuestion[session.CurrentQuestionIndex] = new Dictionary<Guid, PlayerAnswerInfoModel>();

            var answersForQuestion = session.AnswersByQuestion[session.CurrentQuestionIndex];


            // This foreach is to define players who did not answer the question
            foreach (var player in session.Players)
            {
                if (!answersForQuestion.ContainsKey(player.Id))
                {
                    answersForQuestion[player.Id] = new PlayerAnswerInfoModel
                    {
                        QuestionIndex = session.CurrentQuestionIndex,
                        Answer = null,
                        IsCorrect = false,
                        PointsEarned = 0,
                        AnsweredAt = null
                    };

                    // Registers default response for those who did not respond
                    _sessionManager.RegisterPlayerAnswer(gamePin,  player.Id, session.CurrentQuestionIndex, null, false, 0);

                    // Notify player who did not respond
                    if (_connections.TryGetValue(player.Id, out string connectionId))
                    {
                        await Clients.Client(connectionId).SendAsync("Player_ReceiveQuestionScoreIndividual", (bool?)null, 0);
                    }

                }
            }

            _sessionManager.SaveSession(session);
        }

        public async Task Admin_SendLoadingToAll(string gamePin)
        {
            await Clients.Group(gamePin).SendAsync("Player_ReceiveLoading");
        }


        // Get answer counts by question
        private int GetAnswerCountForQuestion(string gamePin, int questionIndex)
        {
            var session = _sessionManager.GetSessionByPin(gamePin);
            if (session == null || session.AnswerCountsByQuestion == null)
                return 0;

            if (session.AnswerCountsByQuestion.TryGetValue(questionIndex, out int count))
                return count;

            return 0;
        }

        
        public int Admin_RequestCurrentAnswerCount(string gamePin, int questionIndex)
        {
            var session = _sessionManager.GetSessionByPin(gamePin);
                if (session == null) return 0;

            if (!session.AnswersByQuestion.ContainsKey(questionIndex))
                return 0;

            return session.AnswersByQuestion[questionIndex].Count;
        }


        // Ranking calculation
        private List<RankingModel> CalculateRanking(GameSessionModel session)
        {
            return session.Players
                .OrderByDescending(p => p.Score)
                .Select((p, index) => new RankingModel
                {
                    GamePin = session.GamePin,
                    PlayerID = p.Id,
                    Name = p.Name,
                    Score = p.Score,
                    Possition = index + 1
                })
                .ToList();
        }

        // Request Current Ranking For Admin
        public async Task<List<RankingModel>> Admin_RequestCurrentRanking(string gamePin)
        {
            var session = _sessionManager.GetSessionByPin(gamePin);
            if (session == null) return new List<RankingModel>();
            var ranking = CalculateRanking(session);
            
            await UpdatePlayerScoreIndividualAsync(session); // Send individual scores to each player
           
            return ranking;
        }

        // Send Current Ranking For Admin
        public async Task<List<RankingModel>> Admin_SendCurrentRanking(string gamePin)
        {
            var session = _sessionManager.GetSessionByPin(gamePin);
            if (session == null) return new List<RankingModel>();

            var ranking = CalculateRanking(session);

            await UpdatePlayerScoreIndividualAsync(session); // Send individual scores to each player

            await Clients.Group(gamePin + "_admin").SendAsync("Admin_ReceiveCurrentRanking", ranking);

            return ranking;
        }

        private async Task UpdatePlayerScoreIndividualAsync(GameSessionModel session)
        {
            // Send individual scores to each player
            foreach (var player in session.Players)
            {
                if (_connections.TryGetValue(player.Id, out string connectionId))
                {
                    await Clients.Client(connectionId).SendAsync("Player_ReceiveFinalScoreIndividual", player.Score);
                }
            }
        }

    }

}
