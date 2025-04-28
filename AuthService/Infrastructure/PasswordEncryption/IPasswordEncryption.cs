namespace AuthService.Infrastructure.Encryption
{
    public interface IPasswordEncryption
    {
        bool CheckPasswordValid(string? password, string? encryptedPassword, string? encryptionKey);
        (string encryptedPassword, string encryptionKey) EncryptionPassword(string password);
    }
}