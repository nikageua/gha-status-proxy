namespace GhaStatusProxy.Abstractions;

using GhaStatusProxy.Models;

public interface IGhaService
{

    #region Methods: Public

    Task<IReadOnlyList<GhaRunDto>> GetLatestRunsAsync(string token, IEnumerable<string> repos);

    #endregion
}
