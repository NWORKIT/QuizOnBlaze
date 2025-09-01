using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using QuizOnBlaze.Models;

namespace QuizOnBlaze.Services
{

    /// <summary>
    /// Player data in terms of persistence.
    /// </summary>
    public class PlayerStateService
    {
        private readonly ProtectedSessionStorage _sessionStorage;
        private const string StorageKey = "PlayerState";

        public Guid Id { get; set; }
        public string? Name { get; set; }
        public string? SessionPin { get; set; }
        public string? AvatarSeed { get; set; }

        private int _score;

        public int Score
        {
            get => _score;
            set
            {
                _score = value;
                NotifyStateChanged();
            }
        }

        // Local event for User Interface
        public event Action? OnChange;

        public PlayerStateService(ProtectedSessionStorage sessionStorage)
        {
            _sessionStorage = sessionStorage;
        }



        public void SetPlayer(Guid id, string name, string pin, string avatarSeed)
        {
            Id = id; 
            Name = name;
            SessionPin = pin;
            AvatarSeed = avatarSeed;
            NotifyStateChanged(); // User Interface
        }


        public void Clear()
        {
            Name = null;
            SessionPin = null;
            AvatarSeed = null;
        }

        public async Task SaveStateAsync()
        {
            var state = new PlayerStateSnapshotModel
            {
                Id = this.Id,
                Name = this.Name,
                SessionPin = this.SessionPin,
                AvatarSeed = this.AvatarSeed
            };
            await _sessionStorage.SetAsync(StorageKey, state);
        }

        public async Task LoadStateAsync()
        {
            var result = await _sessionStorage.GetAsync<PlayerStateSnapshotModel>(StorageKey);
            if (result.Success && result.Value != null)
            {
                this.Id = (Guid)result.Value.Id;
                this.Name = result.Value.Name;
                this.SessionPin = result.Value.SessionPin;
                this.AvatarSeed = result.Value.AvatarSeed;
            }
        }


        
        private void NotifyStateChanged() => OnChange?.Invoke();

    }

}
