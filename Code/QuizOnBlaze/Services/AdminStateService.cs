using QuizOnBlaze.Models;
using System;

namespace QuizOnBlaze.Services
{
    /// <summary>
    /// Local state service to share CurrentSession between 'Admin.razor' component and 'AdminLayout.razor' layout.
    /// </summary>
    public class AdminStateService
    {

        private GameSessionModel? _currentSession;


        public void SetCurrentSession(GameSessionModel? session)
        {
            CurrentSession = session;

        }


        public GameSessionModel? CurrentSession
        {
            get => _currentSession;
            private set
            {
                if (_currentSession != value)
                {
                    _currentSession = value;
                    NotifyStateChanged();
                }
            }
        }

        public event Action? OnChange;

        private void NotifyStateChanged() => OnChange?.Invoke();

    }
}
