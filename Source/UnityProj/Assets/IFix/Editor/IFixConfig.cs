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
    [IFix]
    static IEnumerable<Type> ToProcess
    {
        get
        {
            //return Enumerable.Empty<Type>();
            // 

            var classList = (from type in Assembly.Load("Assembly-CSharp").GetTypes()
                             where !type.Name.Contains("<")

                             //配置表相关


                             // 第三方插件过滤
                             && !type.FullName.StartsWith("Spine.")
                             && !type.FullName.StartsWith("RootMotion.")

                             && !type.FullName.StartsWith("Google.Protobuf.")

                             && !type.FullName.StartsWith("ICSharpCode.SharpZipLib.")
                             && !type.FullName.StartsWith("XLua.")
                             && !type.FullName.EndsWith("Wrap")

                             && !type.FullName.StartsWith("NiceJson.")

                             && !type.FullName.StartsWith("System.")
                             && !type.FullName.StartsWith("Microsoft.")
                             && !type.FullName.StartsWith("IFix.")


                             // 协议相关

                             // Unity引擎类过滤 
                             && !type.Name.StartsWith("UnityEngine")
                             && !type.FullName.StartsWith("UnityEngine")

                             // 框架类过滤
                             select type);

            OutputClassList(classList);

            return classList;
        }
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
                int i = 1;
                foreach (var item in classList)
                {
                    writer.WriteLine(string.Format("{0}\t{1}\t{2}", i++, item.Name, item.FullName));
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
