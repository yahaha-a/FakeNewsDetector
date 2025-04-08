namespace Client.Models;

/// <summary>
/// 配置层级
/// </summary>
public enum ConfigLevel
{
    /// <summary>
    /// 系统级配置，所有用户共享
    /// </summary>
    System,
    
    /// <summary>
    /// 用户级配置，特定用户专用
    /// </summary>
    User,
    
    /// <summary>
    /// 会话级配置，仅当前会话有效
    /// </summary>
    Session
} 