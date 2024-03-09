using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace COG.Utils;

public static class ClassUtils
{
    /// <summary>
    /// 获取一个类的所有子类(包括所有次级子类)
    /// </summary>
    /// <param name="parentType">弗雷</param>
    /// <returns></returns>
    public static List<Type> GetAllSubclasses(this Type parentType)
    {
        var subclassList = new List<Type>();
        foreach (var type in Assembly.GetAssembly(parentType)!.GetTypes())
        {
            if (!type.IsSubclassOf(parentType)) continue;
            subclassList.Add(type);
            subclassList.AddRange(GetAllSubclasses(type)); // 递归找到所有的子类
        }
        return subclassList.Distinct().ToList(); // 移除重复项
    }
}