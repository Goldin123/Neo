// Neo.Domain/Interfaces/IPasswordHasher.cs
namespace Neo.Domain.Interfaces;
public interface IPasswordHasher
{
    string HashPassword(string password);
    bool VerifyPassword(string password, string passwordHash);
}
