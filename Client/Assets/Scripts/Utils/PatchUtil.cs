﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

/// <summary>
/// 热更工具类
/// </summary>
public class PatchUtil
{
    private static string[] patchfiles = null;
    private static readonly string calclist = FileUtil.CombinePaths(UnityEngine.Application.dataPath, "Editor/Tools/calclist.bat");
    private static readonly string getversion = FileUtil.CombinePaths(UnityEngine.Application.dataPath, "Editor/Tools/getversion.bat");

    /// <summary>
    /// 获取需要热更的文件数组
    /// </summary>
    /// <param name="startVersion">开始版本</param>
    /// <param name="endVersion">结束版本</param>
    /// <returns>热更的文件数组</returns>
    public static string[] GetPatchFiles(string startVersion, string endVersion)
    {
        if (patchfiles != null)
        {
            System.Array.Clear(patchfiles, 0, patchfiles.Length);
        }
#if UNITY_EDITOR
        var dir = FileUtil.CombinePaths(UnityEngine.Application.dataPath, "Sources");
        var output = FileUtil.CombinePaths(UnityEngine.Application.dataPath, "Editor/Tools/diff.txt");
        var arg = string.Format("{0} {1} {2}", startVersion, endVersion, output);
        if (Util.ExecuteBat(dir, calclist, arg) == 0)
        {
            patchfiles = System.IO.File.ReadAllLines(output).Where(path => path.StartsWith("Client/Assets/Sources/")).ToArray();
        }
#endif
        return patchfiles;
    }

    /// <summary>
    /// 获取当前版本
    /// </summary>
    /// <returns>版本</returns>
    public static string GetGitVersion()
    {
#if UNITY_EDITOR
        var dir = FileUtil.CombinePaths(UnityEngine.Application.dataPath, "Sources");
        var output = FileUtil.CombinePaths(UnityEngine.Application.dataPath.Replace("/Assets", ""), UnityEditor.FileUtil.GetUniqueTempPathInProject());
        var arg = string.Format("{0}", output);
        if (Util.ExecuteBat(dir, getversion, arg) == 0)
        {
            return System.IO.File.ReadLines(output).First();
        }
#endif
        return string.Empty;
    }

}
