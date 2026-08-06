using System;
using C969_Project.Database;
using C969_Project.Login;
using Xunit;

namespace C969_Project.Tests
{
    public class LoginServiceTests
    {
        [Fact]
        public void Attempt_ReturnsSuccess_ForValidCredentials()
        {
            var stub = new StubAuthenticator { Result = true };
            var service = new LoginService(stub);

            var result = service.Attempt("test", "test");

            Assert.Equal(LoginResult.Success, result);
        }

        [Fact]
        public void Attempt_ReturnsInvalidCredentials_WhenAuthenticatorRejects()
        {
            var stub = new StubAuthenticator { Result = false };
            var service = new LoginService(stub);

            var result = service.Attempt("test", "wrong");

            Assert.Equal(LoginResult.InvalidCredentials, result);
        }

        [Fact]
        public void Attempt_ReturnsEmptyFields_ForBlankUsername()
        {
            var stub = new StubAuthenticator { Result = true };
            var service = new LoginService(stub);

            var result = service.Attempt("", "test");

            Assert.Equal(LoginResult.EmptyFields, result);
            Assert.Equal(0, stub.CallCount);
        }

        [Fact]
        public void Attempt_ReturnsEmptyFields_ForBlankPassword()
        {
            var stub = new StubAuthenticator { Result = true };
            var service = new LoginService(stub);

            var result = service.Attempt("test", "");

            Assert.Equal(LoginResult.EmptyFields, result);
            Assert.Equal(0, stub.CallCount);
        }

        [Fact]
        public void Attempt_ReturnsEmptyFields_ForWhitespaceOnlyUsername()
        {
            var stub = new StubAuthenticator { Result = true };
            var service = new LoginService(stub);

            var result = service.Attempt("   ", "test");

            Assert.Equal(LoginResult.EmptyFields, result);
            Assert.Equal(0, stub.CallCount);
        }

        [Fact]
        public void Attempt_ReturnsEmptyFields_ForWhitespaceOnlyPassword()
        {
            var stub = new StubAuthenticator { Result = true };
            var service = new LoginService(stub);

            var result = service.Attempt("test", "   ");

            Assert.Equal(LoginResult.EmptyFields, result);
            Assert.Equal(0, stub.CallCount);
        }

        [Fact]
        public void Attempt_ReturnsSuccess_ForUsernameWithSurroundingSpaces()
        {
            var stub = new StubAuthenticator { Result = true };
            var service = new LoginService(stub);

            var result = service.Attempt("  test  ", "test");

            Assert.Equal(LoginResult.Success, result);
        }

        [Fact]
        public void Attempt_PassesTrimmedUsername_ToAuthenticator()
        {
            var stub = new StubAuthenticator { Result = true };
            var service = new LoginService(stub);

            service.Attempt("  test  ", "test");

            Assert.Equal("test", stub.LastUserName);
        }

        [Fact]
        public void Attempt_PassesPasswordUntrimmed_ToAuthenticator()
        {
            var stub = new StubAuthenticator { Result = true };
            var service = new LoginService(stub);

            service.Attempt("test", " pw ");

            Assert.Equal(" pw ", stub.LastPassword);
        }

        [Fact]
        public void Attempt_ReturnsSameResult_ForWrongUsernameAndWrongPassword()
        {
            var stub = new StubAuthenticator { Result = false };
            var service = new LoginService(stub);

            var wrongUsername = service.Attempt("nobody", "test");
            var wrongPassword = service.Attempt("test", "nope");

            Assert.Equal(wrongUsername, wrongPassword);
        }

        [Fact]
        public void Constructor_Throws_ForNullAuthenticator()
        {
            Assert.Throws<ArgumentNullException>(() => new LoginService(null!));
        }

        private sealed class StubAuthenticator : IUserAuthenticator
        {
            public int CallCount;
            public string? LastUserName;
            public string? LastPassword;
            public bool Result;

            public bool Validate(string userName, string password)
            {
                CallCount++;
                LastUserName = userName;
                LastPassword = password;
                return Result;
            }
        }
    }
}
