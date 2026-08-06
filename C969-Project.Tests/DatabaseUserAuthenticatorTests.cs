using C969_Project.Login;
using C969_Project.Database;
using Xunit;

namespace C969_Project.Tests
{
    public class DatabaseUserAuthenticatorTests
    {
        //The credential lookup itself talks to MySQL and has no seam to stub, so it
        //is verified by hand against a live database (see ticket 04). What is worth
        //pinning here is the one thing about the production authenticator that can
        //fail without a database: LoginService must still short-circuit blank fields
        //before reaching for a connection that has not been opened.
        [Fact]
        public void Attempt_ReturnsEmptyFields_WithProductionAuthenticator_AndNoOpenConnection()
        {
            DatabaseManager.Conn = null;
            var service = new LoginService(new DatabaseUserAuthenticator());

            var result = service.Attempt("", "");

            Assert.Equal(LoginResult.EmptyFields, result);
        }

        //Guards the same short-circuit for a blank password, which is the case that
        //would reach the database first if the check were ever reordered.
        [Fact]
        public void Attempt_ReturnsEmptyFields_ForBlankPassword_WithProductionAuthenticator()
        {
            DatabaseManager.Conn = null;
            var service = new LoginService(new DatabaseUserAuthenticator());

            var result = service.Attempt("test", "");

            Assert.Equal(LoginResult.EmptyFields, result);
        }
    }
}
