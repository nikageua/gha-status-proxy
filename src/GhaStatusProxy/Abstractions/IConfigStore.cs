namespace GhaStatusProxy.Abstractions;

using GhaStatusProxy.Models;

public interface IConfigStore
{

    #region Methods: Public

    Task<AppConfig> LoadAsync();
    Task SaveAsync(AppConfig cfg);

    #endregion
}
