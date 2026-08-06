using System;
using C969_Project.Database;

namespace C969_Project.Login
{
    //Decides the outcome of a login attempt. Kept free of WinForms and of the
    //database so the rules below can be tested directly.
    public class LoginService
    {
        private readonly IUserAuthenticator _authenticator;

        public LoginService(IUserAuthenticator authenticator)
        {
            ArgumentNullException.ThrowIfNull(authenticator);

            _authenticator = authenticator;
        }

        public LoginResult Attempt(string username, string password)
        {
            //Short-circuit before touching the authenticator - a blank field is a
            //form-level mistake, not a failed credential check, and it should not
            //cost a database round trip.
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                return LoginResult.EmptyFields;
            }

            //The username is trimmed because surrounding spaces are typing noise,
            //but the password is passed through untouched - a deliberate leading or
            //trailing space is part of the password and trimming it would lock the
            //user out of their own account.
            var isValid = _authenticator.Validate(username.Trim(), password);

            //A wrong username and a wrong password collapse into the same result so
            //nothing downstream can reveal which of the two fields was correct.
            return isValid ? LoginResult.Success : LoginResult.InvalidCredentials;
        }
    }
}
