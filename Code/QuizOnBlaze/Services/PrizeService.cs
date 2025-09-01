using System;

namespace QuizOnBlaze.Services
{
    public class PrizeService
    {
        private readonly PlayerService _playerService;
        private readonly Random _rand = new();

        public PrizeService(PlayerService playerService)
        {
            _playerService = playerService;
        }

        public List<PlayerModel> DrawWinners(int numberOfWinners = 3)
        {
            var topPlayers = _playerService.GetTopPlayers(numberOfWinners * 2).ToList();
            var winners = new List<PlayerModel>();

            while (winners.Count < numberOfWinners && topPlayers.Count > 0)
            {
                var index = _rand.Next(topPlayers.Count);
                winners.Add(topPlayers[index]);
                topPlayers.RemoveAt(index);
            }
            return winners;
        }
    }
}
