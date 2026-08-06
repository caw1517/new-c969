using System;
using System.Collections.Generic;
using System.Text;

namespace C969_Project.Database
{
    public enum CustomerFormType
    {
        Edit,
        Add,
    }

    public enum Language
    {
        English,
        German,
    }

    public enum LoginResult
    {
        Success,
        EmptyFields,
        InvalidCredentials,
    }
}
