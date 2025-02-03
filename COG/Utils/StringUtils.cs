using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace COG.Utils;

public static class StringUtils
{
    public static string GetSHA1Hash(this string input)
    {
        using var sha1 = SHA1.Create();
        var inputBytes = Encoding.UTF8.GetBytes(input);
        var hashBytes = sha1.ComputeHash(inputBytes);

        var sb = new StringBuilder();
        foreach (var b in hashBytes) sb.Append(b.ToString("x2"));

        return sb.ToString();
    }

    public static string RemoveLast(this string input)
    {
        return new string(input.Take(input.Length - 1).ToArray());
    }

    public static string CustomFormat(this string text, params object[] args)
    {
        if (text is null || args is null)
            throw new ArgumentNullException(text is null ? nameof(text) : nameof(args));

        var result = new StringBuilder(text.Length);
        var argIndex = 0;
        var isInPlaceholder = false;

        // 单次遍历同时完成格式解析和替换
        foreach (var t in text)
        {
            if (t == '%')
            {
                if (!isInPlaceholder)
                {
                    // 记录占位符起始位置
                    isInPlaceholder = true;
                }
                else
                {
                    // 检测到闭合占位符
                    if (argIndex >= args.Length)
                        throw new ArgumentOutOfRangeException(nameof(args), "参数数量不足");

                    // 直接追加参数值（跳过占位符内容）
                    result.Append(args[argIndex++]?.ToString() ?? string.Empty);
                    isInPlaceholder = false;
                }
                continue;
            }

            if (!isInPlaceholder)
            {
                result.Append(t);
            }
        }

        // 最终状态检查
        if (isInPlaceholder)
            throw new FormatException("未闭合的占位符");

        if (argIndex != args.Length)
            throw new ArgumentException($"参数数量不匹配，需要 {argIndex} 个参数，实际提供 {args.Length} 个");

        return result.ToString();
    }
}