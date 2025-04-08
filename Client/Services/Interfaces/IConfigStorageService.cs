using System.Threading.Tasks;
using Client.Models;

namespace Client.Services.Interfaces;

/// <summary>
/// 配置存储服务接口
/// </summary>
public interface IConfigStorageService
{
    /// <summary>
    /// 保存配置
    /// </summary>
    /// <param name="config">配置对象</param>
    /// <param name="level">配置层级</param>
    Task SaveConfigAsync(AppConfig config, ConfigLevel level);

    /// <summary>
    /// 加载配置
    /// </summary>
    /// <param name="level">配置层级</param>
    /// <returns>配置对象</returns>
    Task<AppConfig> LoadConfigAsync(ConfigLevel level);

    /// <summary>
    /// 检查配置是否存在
    /// </summary>
    /// <param name="level">配置层级</param>
    /// <returns>是否存在</returns>
    Task<bool> ConfigExistsAsync(ConfigLevel level);
    
    /// <summary>
    /// 获取配置文件路径
    /// </summary>
    /// <param name="level">配置层级</param>
    /// <returns>配置文件路径</returns>
    string GetConfigPath(ConfigLevel level);
} 