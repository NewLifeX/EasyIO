using NewLife;
using NewLife.Data;
using XCode;

namespace EasyWeb.Data;

public partial class FileEntry : Entity<FileEntry>
{
    #region 对象操作
    static FileEntry()
    {
        // 累加字段，生成 Update xx Set Count=Count+1234 Where xxx
        var df = Meta.Factory.AdditionalFields;
        df.Add(nameof(Times));

        // 过滤器 UserModule、TimeModule、IPModule
        Meta.Modules.Add<TimeModule>();
        Meta.Modules.Add(new IPModule { AllowEmpty = false });
        Meta.Modules.Add<TraceModule>();

        // 实体缓存
        // var ec = Meta.Cache;
        // ec.Expire = 60;
    }

    /// <summary>验证并修补数据，返回验证结果，或者通过抛出异常的方式提示验证失败。</summary>
    /// <param name="method">添删改方法</param>
    public override Boolean Valid(DataMethod method)
    {
        //if (method == DataMethod.Delete) return true;
        // 如果没有脏数据，则不需要进行任何处理
        if (!HasDirty) return true;

        // 建议先调用基类方法，基类方法会做一些统一处理
        if (!base.Valid(method)) return false;

        if (Path.IsNullOrEmpty() && Parent != null)
            Path = $"{Parent.Path}/{Name}";

        return true;
    }
    #endregion

    #region 扩展属性
    //[XmlIgnore, IgnoreDataMember]
    //public FileEntry Parent => Extends.Get(nameof(Parent), k => FindById(ParentId));
    #endregion

    #region 扩展查询
    /// <summary>根据编号查找</summary>
    /// <param name="id">编号</param>
    /// <returns>实体对象</returns>
    public static FileEntry FindById(Int32 id)
    {
        if (id <= 0) return null;

        // 实体缓存
        if (Meta.Session.Count < 1000) return Meta.Cache.Find(e => e.Id == id);

        // 单对象缓存
        return Meta.SingleCache[id];

        //return Find(_.Id == id);
    }

    /// <summary>根据仓库、上级目录、名称查找</summary>
    /// <param name="storageId">仓库</param>
    /// <param name="parentId">上级目录</param>
    /// <param name="name">名称</param>
    /// <returns>实体对象</returns>
    public static FileEntry FindByStorageIdAndParentIdAndName(Int32 storageId, Int32 parentId, String name)
    {
        // 实体缓存
        if (Meta.Session.Count < 1000) return Meta.Cache.Find(e => e.StorageId == storageId && e.ParentId == parentId && e.Name.EqualIgnoreCase(name));

        return Find(_.StorageId == storageId & _.ParentId == parentId & _.Name == name);
    }

    /// <summary>根据上级目录查找</summary>
    /// <param name="parentId">上级目录</param>
    /// <returns>实体列表</returns>
    public static IList<FileEntry> FindAllByParentId(Int32 parentId)
    {
        if (parentId <= 0) return new List<FileEntry>();

        // 实体缓存
        if (Meta.Session.Count < 1000) return Meta.Cache.FindAll(e => e.ParentId == parentId);

        return FindAll(_.ParentId == parentId);
    }

    /// <summary>根据来源、上级目录查找</summary>
    /// <param name="sourceId">来源</param>
    /// <param name="parentId">上级目录</param>
    /// <returns>实体列表</returns>
    public static IList<FileEntry> FindAllBySourceIdAndParentId(Int32 sourceId, Int32 parentId)
    {
        // 实体缓存
        if (Meta.Session.Count < 1000) return Meta.Cache.FindAll(e => e.SourceId == sourceId && e.ParentId == parentId);

        return FindAll(_.SourceId == sourceId & _.ParentId == parentId);
    }

    /// <summary>根据仓库、上级目录查找</summary>
    /// <param name="storageId">仓库</param>
    /// <param name="parentId">上级目录</param>
    /// <returns>实体列表</returns>
    public static IList<FileEntry> FindAllByStorageIdAndParentId(Int32 storageId, Int32 parentId)
    {
        // 实体缓存
        if (Meta.Session.Count < 1000) return Meta.Cache.FindAll(e => e.StorageId == storageId && e.ParentId == parentId);

        return FindAll(_.StorageId == storageId & _.ParentId == parentId);
    }

    public static FileEntry FindByStorageIdAndPath(Int32 storageId, String path)
    {
        // 实体缓存
        if (Meta.Session.Count < 1000) return Meta.Cache.Find(e => e.StorageId == storageId && e.Path == path);

        return Find(_.StorageId == storageId & _.Path == path);
    }

    /// <summary>根据仓库、路径查找</summary>
    /// <param name="storageId">仓库</param>
    /// <param name="path">路径</param>
    /// <returns>实体列表</returns>
    public static IList<FileEntry> FindAllByStorageIdAndPath(Int32 storageId, String path)
    {
        // 实体缓存
        if (Meta.Session.Count < 1000) return Meta.Cache.FindAll(e => e.StorageId == storageId && e.Path.EqualIgnoreCase(path));

        return FindAll(_.StorageId == storageId & _.Path == path);
    }
    #endregion

    #region 高级查询
    /// <summary>高级查询</summary>
    /// <param name="storageId">仓库</param>
    /// <param name="name">名称</param>
    /// <param name="parentId">上级目录。0表示顶级</param>
    /// <param name="start">更新时间开始</param>
    /// <param name="end">更新时间结束</param>
    /// <param name="key">关键字</param>
    /// <param name="page">分页参数信息。可携带统计和数据权限扩展查询等信息</param>
    /// <returns>实体列表</returns>
    public static IList<FileEntry> Search(Int32 storageId, String name, Int32 parentId, Boolean? isDir, Boolean? enable, DateTime start, DateTime end, String key, PageParameter page)
    {
        var exp = new WhereExpression();

        if (storageId >= 0) exp &= _.StorageId == storageId;
        if (!name.IsNullOrEmpty()) exp &= _.Name == name;
        if (parentId >= 0) exp &= _.ParentId == parentId;
        if (isDir != null) exp &= _.IsDirectory == isDir;
        if (enable != null) exp &= _.Enable == enable;

        exp &= _.UpdateTime.Between(start, end);

        if (!key.IsNullOrEmpty()) exp &= _.Name.StartsWith(key) | _.FullName.StartsWith(key) | _.Title.Contains(key) | _.Path.StartsWith(key) | _.RawUrl.Contains(key) | _.LinkTarget.Contains(key) | _.Remark.Contains(key);

        return FindAll(exp, page);
    }

    public static IList<FileEntry> Search(Int32 storageId, Int32 parentId, Boolean? enable)
    {
        var exp = new WhereExpression();

        if (storageId >= 0) exp &= _.StorageId == storageId;
        if (parentId >= 0) exp &= _.ParentId == parentId;
        if (enable != null) exp &= _.Enable == enable;

        return FindAll(exp);
    }

    // Select Count(Id) as Id,Category From FileEntry Where CreateTime>'2020-01-24 00:00:00' Group By Category Order By Id Desc limit 20
    //static readonly FieldCache<FileEntry> _CategoryCache = new FieldCache<FileEntry>(nameof(Category))
    //{
    //Where = _.CreateTime > DateTime.Today.AddDays(-30) & Expression.Empty
    //};

    ///// <summary>获取类别列表，字段缓存10分钟，分组统计数据最多的前20种，用于魔方前台下拉选择</summary>
    ///// <returns></returns>
    //public static IDictionary<String, String> GetCategoryList() => _CategoryCache.FindAllName();
    #endregion

    #region 业务操作
    #endregion
}