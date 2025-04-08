using Client.Models;
using System.Threading.Tasks;

namespace Client.Services;

public interface IConfigService
{
    /// <summary>
    /// 获取当前配置
    /// </summary>
    AppConfig GetConfig();

    /// <summary>
    /// 更新配置
    /// </summary>
    Task UpdateConfigAsync(AppConfig config);

    /// <summary>
    /// 保存配置到文件
    /// </summary>
    Task SaveConfigAsync();

    /// <summary>
    /// 从文件加载配置
    /// </summary>
    Task LoadConfigAsync();

    /// <summary>
    /// 重置配置为默认值
    /// </summary>
    Task ResetConfigAsync();
} 