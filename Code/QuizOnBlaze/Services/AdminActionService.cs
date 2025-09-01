using QuizOnBlaze.Enum;

namespace QuizOnBlaze.Services
{
    public class AdminActionService
    {

        public event Func<AdminAction, Task>? OnMenuAction;

        public async Task TriggerMenuAction(AdminAction action)
        {
            if (OnMenuAction != null)
                await OnMenuAction.Invoke(action);

          
        }
    }
}
