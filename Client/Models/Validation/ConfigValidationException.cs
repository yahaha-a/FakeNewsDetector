using System;
using System.Collections.Generic;
using System.Linq;
using FluentValidation.Results;

namespace Client.Models.Validation;

/// <summary>
/// 配置验证异常
/// </summary>
public class ConfigValidationException : Exception
{
    /// <summary>
    /// 验证错误列表
    /// </summary>
    public IReadOnlyList<ValidationFailure> Errors { get; }
    
    /// <summary>
    /// 初始化配置验证异常
    /// </summary>
    /// <param name="errors">验证错误列表</param>
    public ConfigValidationException(IEnumerable<ValidationFailure> errors)
        : base(BuildErrorMessage(errors))
    {
        Errors = errors.ToList().AsReadOnly();
    }
    
    /// <summary>
    /// 构建错误信息
    /// </summary>
    /// <param name="errors">验证错误列表</param>
    /// <returns>格式化的错误信息</returns>
    private static string BuildErrorMessage(IEnumerable<ValidationFailure> errors)
    {
        var errorMessages = errors.Select(e => $"- {e.PropertyName}: {e.ErrorMessage}");
        return $"配置验证失败，共有 {errors.Count()} 个错误:\n{string.Join("\n", errorMessages)}";
    }
} 