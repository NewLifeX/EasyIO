using EasyWeb.Data;
using NewLife;
using NewLife.Caching;

namespace EasyWeb.Services;

public class EntryService
{
    private readonly ICacheProvider _cacheProvider;

    public EntryService(ICacheProvider cacheProvider) => _cacheProvider = cacheProvider;

    /// <summary>获取默认存储</summary>
    /// <returns></returns>
    public FileStorage GetDefaultStorage()
    {
        return _cacheProvider.Cache.GetOrAdd($"DefaultStorage",
            k => FileStorage.FindAllWithCache().FirstOrDefault(e => e.Enable), 60);
    }

    public IList<FileEntry> GetFiles(Int32 storageId, Int32 parentId)
    {
        if (storageId == 0) storageId = GetDefaultStorage()?.Id ?? 0;

        return _cacheProvider.Cache.GetOrAdd($"Files:{storageId}:{parentId}",
            k => FileEntry.FindAllByStorageIdAndParentId(storageId, parentId), 60);
    }

    public FileEntry GetFile(Int32 storageId, String path)
    {
        if (path.IsNullOrEmpty()) return null;
        if (storageId == 0) storageId = GetDefaultStorage()?.Id ?? 0;

        return _cacheProvider.Cache.GetOrAdd($"File:{storageId}:{path}",
            k => FileEntry.FindByStorageIdAndPath(storageId, path), 60);
    }
}
