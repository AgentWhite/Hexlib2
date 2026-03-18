using System.Collections.Generic;
using System.Threading.Tasks;

namespace HexLib.Persistence;

/// <summary>
/// Defines a storage medium adapter for persisting raw data.
/// Allows swapping between File System, SQLite, Cloud Storage, etc.
/// </summary>
public interface IStorageAdapter
{
    /// <summary>
    /// Saves the provided string data to the specified key/identifier.
    /// </summary>
    Task SaveAsync(string key, string data);

    /// <summary>
    /// Loads the string data associated with the specified key/identifier.
    /// </summary>
    Task<string?> LoadAsync(string key);

    /// <summary>
    /// Lists all available keys/identifiers under a specific category or prefix.
    /// </summary>
    Task<IEnumerable<string>> ListKeysAsync(string category);

    /// <summary>
    /// Deletes the data associated with the specified key/identifier.
    /// </summary>
    Task DeleteAsync(string key);
}
