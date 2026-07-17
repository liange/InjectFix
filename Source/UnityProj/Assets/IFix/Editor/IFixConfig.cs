using IFix;
using System.Collections.Generic;
using System;
using System.Reflection;
using System.Linq;
using System.IO;
using UnityEngine;

[Configure]
public class IFanConfig
{
    /// <summary>
    /// 需要加载的程序集名称列表
    /// </summary>
    private static readonly string[] AssemblyNames = new string[]
    {
        "Assembly-CSharp",
        "Assembly-CSharp-firstpass",
    };

    private static List<Type> _cachedToProcess = null;

    [IFix]
    static IEnumerable<Type> ToProcess
    {
        get
        {
            if (_cachedToProcess != null)
            {
                return _cachedToProcess;
            }

            var classList = GetFilteredTypesFromAssemblies(AssemblyNames);

            OutputClassList(classList);

            _cachedToProcess = classList;
            return _cachedToProcess;
        }
    }

    /// <summary>
    /// 从多个程序集中获取过滤后的类型列表
    /// </summary>
    /// <param name="assemblyNames">程序集名称数组</param>
    /// <returns>过滤后的类型集合</returns>
    static List<Type> GetFilteredTypesFromAssemblies(params string[] assemblyNames)
    {
        var allTypes = new List<Type>();

        foreach (var assemblyName in assemblyNames)
        {
            try
            {
                var assembly = Assembly.Load(assemblyName);
                if (assembly != null)
                {
                    var types = assembly.GetTypes();
                    allTypes.AddRange(types);
                    Debug.Log($"[IFanConfig] 成功加载程序集: {assemblyName}, 类型数量: {types.Length}");
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[IFanConfig] 加载程序集失败: {assemblyName}, 错误: {ex.Message}");
            }
        }

        return FilterTypes(allTypes).ToList();
    }

    /// <summary>
    /// 过滤类型列表
    /// </summary>
    /// <param name="types">原始类型列表</param>
    /// <returns>过滤后的类型集合</returns>
    static IEnumerable<Type> FilterTypes(IEnumerable<Type> types)
    {
        return from type in types
               where !type.Name.Contains("<")


               // 第三方插件过滤
               && !type.FullName.StartsWith("Spine.")
               && !type.FullName.StartsWith("RootMotion.")

			   && !type.FullName.StartsWith("Google.Protobuf.")
               && !type.FullName.StartsWith("ICSharpCode.SharpZipLib.")
               && !type.FullName.StartsWith("XLua.")
               && !type.FullName.EndsWith("Wrap")
			   && !type.FullName.StartsWith("IFix.")
			   
			   //C#
			   && !type.FullName.StartsWith("System.")
               && !type.FullName.StartsWith("Microsoft.")
               
               // Unity引擎类过滤 
               && !type.Name.StartsWith("UnityEngine")
               && !type.FullName.StartsWith("UnityEngine")

               select type;
    }

    static void OutputClassList(IEnumerable<Type> classList)
    {
        var outputFile = $"{Application.dataPath}/ClassList_{GenerateTimeString()}.txt";
        Debug.Log($"OutputClassList inject = {outputFile}");

        if (File.Exists(outputFile))
            File.Delete(outputFile);

            using (FileStream fileStream = new FileStream(outputFile, FileMode.Create))
            {
                using (StreamWriter writer = new StreamWriter(fileStream))
                {
                    writer.WriteLine("No\tAssembly\tName\tFullName");

                    int i = 1;
                    foreach (var item in classList)
                    {
                        var asmName = item.Assembly.GetName().Name;
                        writer.WriteLine(string.Format("{0}\t{1}\t{2}\t{3}", i++, asmName, item.Name, item.FullName));
                    }

                    writer.Flush();
                    writer.Close();
                    writer.Dispose();
                }

                fileStream.Close();
                fileStream.Dispose();
            }
    }

    static string GenerateTimeString()
    {
        var dateString = System.DateTime.Now.Year.ToString() + "-" + System.DateTime.Now.Month.ToString() + "-" + System.DateTime.Now.Day.ToString();
        var timeString = System.DateTime.Now.ToLongTimeString().Replace(":", ".").Replace(" ", "");
        return dateString + "-" + timeString;
    }
}
