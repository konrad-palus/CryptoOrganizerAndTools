namespace CryptoOrganizerWebAPI.Interfaces
{
    public interface ICacheService
    {
        void Set<T>(string key, T data, TimeSpan? expiry = null);
        T? Get<T>(string key);
        void Remove(string key);
        bool Exists(string key);
    }
}