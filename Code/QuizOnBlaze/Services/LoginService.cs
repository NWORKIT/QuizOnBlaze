namespace QuizOnBlaze.Services
{
    public class LoginService
    {
        private readonly string _adminPassword;

        public LoginService(IConfiguration configuration)
        {
            _adminPassword = configuration["AdminSettings:AdminPassword"] ?? "";
        }

        public bool ValidateAdminPassword(string inputPassword)
        {
            return inputPassword == _adminPassword;
        }
    }
}
