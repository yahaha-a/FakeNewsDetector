using System.Threading.Tasks;

namespace Client.Services.Interfaces
{
    /// <summary>
    /// 应用设置服务接口
    /// </summary>
    public interface ISettingsService
    {
        /// <summary>
        /// 获取设置值
        /// </summary>
        /// <typeparam name="T">设置值类型</typeparam>
        /// <param name="key">设置键</param>
        /// <param name="defaultValue">默认值</param>
        /// <returns>设置值</returns>
        Task<T> GetSettingAsync<T>(string key, T? defaultValue = default);

        /// <summary>
        /// 保存设置值
        /// </summary>
        /// <typeparam name="T">设置值类型</typeparam>
        /// <param name="key">设置键</param>
        /// <param name="value">设置值</param>
        /// <returns>是否成功</returns>
        Task<bool> SaveSettingAsync<T>(string key, T value);

        /// <summary>
        /// 移除设置
        /// </summary>
        /// <param name="key">设置键</param>
        /// <returns>是否成功</returns>
        Task<bool> RemoveSettingAsync(string key);

        /// <summary>
        /// 清除所有设置
        /// </summary>
        /// <returns>是否成功</returns>
        Task<bool> ClearAllSettingsAsync();

        /// <summary>
        /// API服务基地址
        /// </summary>
        string ApiBaseUrl { get; set; }

        /// <summary>
        /// 是否启用深色模式
        /// </summary>
        bool IsDarkMode { get; set; }

        /// <summary>
        /// 是否自动保存分析历史
        /// </summary>
        bool AutoSaveHistory { get; set; }
    }
} 