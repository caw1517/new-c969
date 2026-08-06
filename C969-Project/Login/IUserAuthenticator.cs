namespace C969_Project.Login
{
    //The credential lookup is injected so the login decision logic can be
    //exercised with no database present. The real implementation arrives in
    //ticket 04 (DatabaseManager.ValidateUser).
    public interface IUserAuthenticator
    {
        bool Validate(string userName, string password);
    }
}
