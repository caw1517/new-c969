using C969_Project.Database;

namespace C969_Project.Login
{
    //The production credential lookup: adapts the static DatabaseManager to the
    //injected IUserAuthenticator seam so LoginService never names the database
    //directly and stays testable with a stub. Wired up in ticket 07.
    public class DatabaseUserAuthenticator : IUserAuthenticator
    {
        public bool Validate(string userName, string password) =>
            DatabaseManager.ValidateUser(userName, password);
    }
}
