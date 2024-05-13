public interface IUserRepository
{
    Task<bool> UserExistsAsync(string name);
    Task<bool> AddUserAsync(string name, string passwordHash);
    Task<string?> GetUserPasswordAsync(string name);
}