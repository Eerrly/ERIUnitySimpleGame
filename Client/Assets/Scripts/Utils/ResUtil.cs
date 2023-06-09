﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEditor;
using System.Linq;
using ICSharpCode.SharpZipLib.Zip;
using System;

public class ResUtil
{
    

    public static string GetAtlasPathBySpritePath(string spritePath)
    {
        var atlasName = FileUtil.CombinePaths("Sources", Path.GetDirectoryName(spritePath)).Replace("/", "_").ToLower();
        var atlasPath = FileUtil.CombinePaths(Setting.EditorSpriteAtlasPath, atlasName) + Constant.ATLASSPRITE_EXTENSION;
        return atlasPath;
    }

    public static string GetFileNameWithoutExtension(string path)
    {
        var fileName = Path.GetFileNameWithoutExtension(path);
        return fileName;
    }

    private static string[] GetAddressableNames(string root, string[] rawNames)
    {
        var names = new string[rawNames.Length];
        for (var i = 0; i < names.Length; ++i)
        {
            names[i] = FileUtil.Normalized(rawNames[i]).ToLower().Replace(root + "/", "");
        }
        return names;
    }

    private static void CheckLoop(AssetBundleManifest manifest, Dictionary<string, string> hash2Name, string name, Stack<string> tracker)
    {
        foreach (var sub in tracker)
        {
            if (name == sub)
            {
                var sb = new System.Text.StringBuilder();
                tracker.Push(name);
                while (tracker.Count > 0)
                {
                    sb.AppendLine(hash2Name[tracker.Pop()]);
                }
                throw new System.Exception("loop dependencies!\n" + sb.ToString());
            }
        }

        tracker.Push(name);
        var dependencies = new Queue<string>(manifest.GetAllDependencies(name));
        foreach (var dependency in dependencies)
        {
            var count = tracker.Count;
            CheckLoop(manifest, hash2Name, dependency, tracker);
            while (tracker.Count > count)
            {
                tracker.Pop();
            }
        }
    }

#if UNITY_EDITOR
    /// <summary>
    /// 构建Lua脚本资源
    /// </summary>
    public static void BuildLuaScripts()
    {
        var start = System.DateTime.Now;
        try
        {
            var files = Directory.GetFiles(Setting.EditorLuaScriptRoot, "*.lua", SearchOption.AllDirectories).ToList();
            if (!(ComplieFiles(files, "32", true) && ComplieFiles(files, "64", true)))
            {
                Debug.LogError("build scripts has error !!");
            }
            AssetDatabase.Refresh();
        }
        catch(System.Exception e)
        {
            UnityEngine.Debug.LogException(e);
        }
        finally
        {
            UnityEngine.Debug.Log("build all scripts cost time : " + (System.DateTime.Now - start).TotalMilliseconds + " ms");
        }
    }

    /// <summary>
    /// 通过LuaJit编译Lua脚本
    /// </summary>
    /// <param name="files">Lua脚本列表</param>
    /// <param name="tag">系统架构</param>
    /// <param name="checkError">是否检测Lua错误</param>
    /// <returns></returns>
    private static bool ComplieFiles(List<string> files, string tag, bool checkError)
    {
        var luaTargetDirectory = FileUtil.CombinePaths(Application.dataPath.Replace("/Assets", ""), Setting.EditorScriptBundleName, tag);
        if (!Directory.Exists(luaTargetDirectory))
        {
            Directory.CreateDirectory(luaTargetDirectory);
        }

        var hasError = false;
        var luajit = FileUtil.CombinePaths(Application.dataPath, string.Format("Examples/Tools/LuaJit/luajit{0}.exe", tag));
        var L = XLua.LuaDLL.Lua.luaL_newstate();
        try
        {
            for (int i = 0; i < files.Count; i++)
            {
                var targetFile = FileUtil.CombinePaths(luaTargetDirectory, files[i].Replace(".lua", ".bytes").Replace(Setting.EditorLuaScriptRoot, ""));
                var index = targetFile.LastIndexOf("/");
                var targetFileDir = targetFile.Substring(0, index);
                if (!Directory.Exists(targetFileDir))
                {
                    FileUtil.CreateDirectory(targetFileDir);
                }
                if (!Directory.Exists(files[i]))
                {
                    var bytes = File.ReadAllBytes(files[i]);
                    if (bytes.Length > 3 && bytes[0] == 0xEF && bytes[1] == 0xBB && bytes[2] == 0xBF)
                    {
                        var temp = new byte[bytes.Length - 3];
                        Array.Copy(bytes, 3, temp, 0, bytes.Length - 3);
                        bytes = temp;
                    }
                    if (checkError)
                    {
                        if (XLua.LuaDLL.Lua.xluaL_loadbuffer(L, bytes, bytes.Length, files[i]) != 0)
                        {
                            hasError = true;
                            var error = XLua.LuaDLL.Lua.lua_tostring(L, -1);
                            UnityEngine.Debug.LogError(error);
                        }
                    }
                    if (Util.ExecuteBat(Path.GetDirectoryName(luajit), luajit, string.Format("{0} {1} {2}", "-b", files[i], targetFile)) == 1)
                    {
                        hasError = true;
                        UnityEngine.Debug.LogError("luajit compile failed:" + files[i]);
                    }
                }
            }
            AssetDatabase.ImportAsset(luaTargetDirectory, ImportAssetOptions.Default);
        }
        catch(System.Exception e)
        {
            UnityEngine.Debug.LogException(e);
        }
        finally
        {
            XLua.LuaDLL.Lua.lua_close(L);
        }

        return !hasError;
    }

    /// <summary>
    /// 构建资源
    /// </summary>
    public static void Build()
    {
        var version = PatchUtil.GetGitVersion();
        var version_path = FileUtil.CombinePaths(Setting.EditorResourcePath, Setting.EditorConfigPath, Constant.VERSION_TXT_NAME);
        File.WriteAllText(version_path, version);
        AssetDatabase.ImportAsset(version_path, ImportAssetOptions.ForceUpdate);

        var cfg = Util.LoadConfig<BuildToolsConfig>(Constant.CLIENT_CONFIG_NAME);
        var bundleList = new List<AssetBundleBuild>();
        var hash2Name = new Dictionary<string, string>();
        var hash2Path = new Dictionary<string, string>();
        var mainBundleList = new List<string>();
        var configItemMap = new Dictionary<string, BuildToolsConfig.BuildToolsConfigItem>();

        BuildLuaScripts();

        foreach (var cur in cfg.itemList)
        {
            string[] items = null;
            if (cur.directories)
            {
                items = Directory.GetDirectories(FileUtil.CombinePaths(Setting.EditorBundlePath, cur.root), cur.filter, (SearchOption)cur.searchoption);
            }
            else
            {
                items = Directory.GetFiles(FileUtil.CombinePaths(Setting.EditorBundlePath, cur.root), cur.filter, (SearchOption)cur.searchoption);
            }
            foreach (var item in items)
            {
                var path = FileUtil.Normalized(item).ToLower();
                var keyPath = path.Replace("assets/sources/", "");
                if (keyPath.EndsWith(".meta"))
                {
                    continue;
                }
                var name = Util.HashPath(keyPath).ToString() + ".s";
                hash2Name.Add(name, path);
                hash2Path.Add(name, keyPath);
                configItemMap.Add(name, cur);
                if (cur.directories)
                {
                    var subItems = Directory.GetFiles(item, "*.*", SearchOption.AllDirectories);
                    var newList = new List<string>();
                    foreach (var subItem in subItems)
                    {
                        if (!Path.GetFileName(subItem).Contains("."))
                        {
                            continue;
                        }

                        if (!subItem.EndsWith(".meta"))
                        {
                            newList.Add(subItem);
                        }
                    }
                    var refItem = newList.ToArray();
                    bundleList.Add(new AssetBundleBuild()
                    {
                        assetBundleName = name,
                        assetNames = refItem,
                        addressableNames = GetAddressableNames(path, refItem),
                    });
                    mainBundleList.Add(name);
                }
                else
                {
                    bundleList.Add(new AssetBundleBuild()
                    {
                        assetBundleName = name,
                        addressableNames = new string[] { "_" },
                        assetNames = new string[] { item },
                    });
                    mainBundleList.Add(name);
                }
            }
        }

        AssetBundleManifest manifest = null;
        FileUtil.CreateDirectory(Setting.EditorBundleBuildCachePath);
        manifest = BuildPipeline.BuildAssetBundles(
            Setting.EditorBundleBuildCachePath,
            bundleList.ToArray(),
            BuildAssetBundleOptions.ChunkBasedCompression | BuildAssetBundleOptions.DisableLoadAssetByFileName | BuildAssetBundleOptions.DisableLoadAssetByFileNameWithExtension,
            EditorUserBuildSettings.activeBuildTarget);

        var bundleNames = manifest.GetAllAssetBundles();
        var bundleManifest = new ManifestConfig();
        var mainBundleItems = new List<ManifestItem>(bundleNames.Length);

        // loop check
        for (var i = 0; i < bundleNames.Length; ++i)
        {
            var tracker = new Stack<string>();
            CheckLoop(manifest, hash2Name, bundleNames[i], tracker);
        }

        if (Directory.Exists(Setting.StreamingBundleRoot))
        {
            Directory.Delete(Setting.StreamingBundleRoot, true);
        }
        FileUtil.CreateDirectory(Setting.StreamingRoot);
        FileUtil.CreateDirectory(Setting.StreamingBundleRoot);

        var abMainFilePath = FileUtil.CombinePaths(Setting.StreamingBundleRoot, "main.s");
        var abMainFile = new FileStream(abMainFilePath, FileMode.Create);

        var head = new byte[] { 0xAA, 0xBB, 0x10, 0x12 };
        abMainFile.Write(head, 0, head.Length);

        uint offset = (uint)head.Length;
        for (int i = 0; i < bundleNames.Length; i++)
        {
            if (mainBundleList.Contains(bundleNames[i]))
            {
                var dependencies = manifest.GetAllDependencies(bundleNames[i]);
                for (int j = 0; j < dependencies.Length; j++)
                {
                    if (!mainBundleList.Contains(dependencies[j]))
                    {
                        mainBundleList.Add(dependencies[j]);
                    }
                }
            }
        }

        for (int i = 0; i < bundleNames.Length; i++)
        {
            var hash = bundleNames[i].Substring(0, bundleNames[i].Length - 2);
            var bytes = File.ReadAllBytes(FileUtil.CombinePaths(Setting.EditorBundleBuildCachePath, bundleNames[i]));
            if (mainBundleList.Contains(bundleNames[i]))
            {
                abMainFile.Write(bytes, 0, bytes.Length);
            }
            var strDependencies = manifest.GetAllDependencies(bundleNames[i]);
            var uintDependencies = new uint[strDependencies.Length];
            for (int j = 0; j < strDependencies.Length; j++)
            {
                uintDependencies[j] = uint.Parse(strDependencies[j].Replace(".s", ""));
            }
            if (mainBundleList.Contains(bundleNames[i]))
            {
                mainBundleItems.Add(new ManifestItem()
                {
                    hash = uint.Parse(hash),
                    dependencies = uintDependencies,
                    offset = offset,
                    size = bytes.Length,
                    directories = configItemMap[bundleNames[i]].directories,
                    extension = configItemMap[bundleNames[i]].extension,
                    packageResourcePath = hash2Path.ContainsKey(bundleNames[i]) ? hash2Path[bundleNames[i]] : string.Empty,
                    md5 = Util.MD5(bytes),
                });
            }
            offset += (uint)bytes.Length;
        }
        abMainFile.Close();
        AssetDatabase.ImportAsset(Setting.StreamingBundleRoot, ImportAssetOptions.ForceUpdate);

        bundleManifest.items = mainBundleItems.ToArray();
        Util.SaveConfig(bundleManifest, Constant.ASSETBUNDLES_CONFIG_NAME);

        AssetDatabase.Refresh();
    }

    /// <summary>
    /// 构建热更
    /// </summary>
    /// <param name="patchList">热更文件列表</param>
    public static void Patch(HashSet<string> patchList)
    {
        var patchMap = new Dictionary<string, string>();
        var bundleList = new List<AssetBundleBuild>();
        var configMap = new Dictionary<string, BuildToolsConfig.BuildToolsConfigItem>();
        var hash2Name = new Dictionary<string, string>();
        var hash2Path = new Dictionary<string, string>();

        var resourceVersion = PatchUtil.GetGitVersion();

        BuildLuaScripts();
        var assetBundleNames = AssetDatabase.GetAllAssetBundleNames();
        foreach (var assetBundleName in assetBundleNames)
        {
            AssetDatabase.RemoveAssetBundleName(assetBundleName, true);
        }
        var patchingNoteList = new System.Text.StringBuilder();

        foreach (var cur in Setting.Config.itemList)
        {
            string[] files = null;
            if (cur.directories)
            {
                files = Directory.GetDirectories(FileUtil.CombinePaths(Setting.EditorBundlePath, cur.root), cur.filter, (SearchOption)cur.searchoption);
            }
            else
            {
                files = Directory.GetFiles(FileUtil.CombinePaths(Setting.EditorBundlePath, cur.root), cur.filter, (SearchOption)cur.searchoption);
            }
            foreach (var item in files)
            {
                var path = FileUtil.Normalized(item).ToLower();
                var keyPath = path.Replace("assets/sources/", "");
                if (keyPath.EndsWith(".meta"))
                {
                    continue;
                }
                if(!cur.directories && patchList.Contains(keyPath))
                {
                    patchMap.Add(keyPath, "");
                    patchingNoteList.AppendLine("patch:" + keyPath);
                    patchList.Remove(keyPath);
                }

                var name = Util.HashPath(keyPath).ToString() + ".s";
                configMap.Add(name, cur);
                hash2Name.Add(name, path);
                hash2Path.Add(name, keyPath);

                if (cur.directories)
                {
                    var patchItems = Directory.GetFiles(item, "*.*", SearchOption.AllDirectories);
                    var newList = new List<string>();
                    foreach (var patchItem in patchItems)
                    {
                        if (patchItem.EndsWith(".meta"))
                        {
                            continue;
                        }

                        if (!Path.GetFileName(patchItem).Contains("."))
                        {
                            continue;
                        }

                        var patchPath = patchItem.Replace('\\', '/').ToLower().Replace("assets/sources/", "");
                        if(patchPath.StartsWith("lua/32/", StringComparison.OrdinalIgnoreCase) || patchPath.StartsWith("lua/64/", StringComparison.OrdinalIgnoreCase))
                        {
                            patchPath = patchPath.Substring(7, patchPath.Length - 7);
                        }
                        if (patchList.Contains(patchPath))
                        {
                            newList.Add(patchItem);
                        }
                    }

                    if (newList.Count == 0)
                    {
                        continue;
                    }

                    var refItems = newList.ToArray();
                    bundleList.Add(new AssetBundleBuild()
                    {
                        assetBundleName = name + ".p",
                        assetNames = refItems,
                        addressableNames = GetAddressableNames(path, refItems)
                    });
                }
                else
                {
                    bundleList.Add(new AssetBundleBuild()
                    {
                        assetBundleName = name,
                        addressableNames = new string[] { "_" },
                        assetNames = new string[] { item },
                    });
                }
            }
        }

        if (patchList.Count > 0)
        {
            var withoutExtensions = new List<string>() { ".prefab", ".unity", ".mat" };
            var files = Directory.GetFiles(Setting.EditorBundlePath, "*.*", SearchOption.AllDirectories)
                    .Where(s => withoutExtensions.Contains(Path.GetExtension(s).ToLower())).ToArray();
            var depMap = new Dictionary<string, List<string>>(files.Length);
            foreach (var file in files)
            {
                var rawFile = file.Replace("\\", "/");
                var keyPath = rawFile.ToLower().Replace("assets/sources/", "");
                var deps = AssetDatabase.GetDependencies(rawFile, false);
                foreach (var dep in deps)
                {
                    var depKeyPath = dep.ToLower().Replace("\\", "/").Replace("assets/sources/", "");
                    List<string> list = null;
                    if (!depMap.TryGetValue(depKeyPath, out list))
                    {
                        list = new List<string>();
                        depMap.Add(depKeyPath, list);
                    }
                    list.Add(keyPath);
                }
            }
            var checkQueue = new Queue<string>();
            foreach (var patch in patchList)
            {
                checkQueue.Enqueue(patch);
            }
            while (checkQueue.Count > 0)
            {
                var patch = checkQueue.Dequeue();
                if (depMap.ContainsKey(patch))
                {
                    var list = depMap[patch];
                    foreach (var parent in list)
                    {
                        var key = Util.HashPath(parent) + ".s";
                        BuildToolsConfig.BuildToolsConfigItem item = null;
                        if (configMap.TryGetValue(key, out item) && !patchMap.ContainsKey(parent))
                        {
                            patchMap.Add(parent, "");
                            patchingNoteList.AppendLine("patch:" + parent);
                        }
                    }
                }
            }
        }

        AssetBundleManifest manifest = null;
        FileUtil.CreateDirectory(Setting.EditorBundleBuildCachePath);
        manifest = BuildPipeline.BuildAssetBundles(
            Setting.EditorBundleBuildCachePath,
            bundleList.ToArray(),
            BuildAssetBundleOptions.ChunkBasedCompression | BuildAssetBundleOptions.DisableLoadAssetByFileName | BuildAssetBundleOptions.DisableLoadAssetByFileNameWithExtension,
            EditorUserBuildSettings.activeBuildTarget);

        var assetsRootPath = Application.dataPath.Replace("/Assets", "");
        var patchFilePath = FileUtil.CombinePaths(assetsRootPath, UnityEditor.FileUtil.GetUniqueTempPathInProject());
        var versionPatchFilePath = FileUtil.CombinePaths(patchFilePath, resourceVersion.ToString());
        FileUtil.CreateDirectory(patchFilePath);
        FileUtil.CreateDirectory(versionPatchFilePath);

        var bundleNames = manifest.GetAllAssetBundles();
        var bundleManifestFile = new ManifestConfig();
        var items = new List<ManifestItem>(bundleNames.Length);

        for (var i = 0; i < bundleNames.Length; ++i)
        {
            var isPatchAB = bundleNames[i].EndsWith(".p");
            bundleNames[i] = isPatchAB ? bundleNames[i].Replace(".p", "") : bundleNames[i];
            var hash = bundleNames[i].Substring(0, bundleNames[i].Length - 2);
            var filnalName = hash + ".s";
            if (patchMap.ContainsKey(hash2Path[filnalName]))
            {
                var destFile = FileUtil.CombinePaths(versionPatchFilePath, bundleNames[i]);
                var sourceBytes = File.ReadAllBytes(FileUtil.CombinePaths(Setting.EditorBundleBuildCachePath, isPatchAB ? bundleNames[i] + ".p" : bundleNames[i]));
                if (File.Exists(destFile)) File.Delete(destFile);
                File.WriteAllBytes(destFile, sourceBytes);

                var nameDependencies = manifest.GetAllDependencies(bundleNames[i]);
                var dependencies = new uint[nameDependencies.Length];
                for (var j = 0; j < nameDependencies.Length; ++j)
                {
                    dependencies[j] = uint.Parse(nameDependencies[j].Replace(".s", "").Replace(".p", ""));
                }

                var name = hash + ".s";
                items.Add(new ManifestItem() {
                    hash = uint.Parse(hash),
                    dependencies = dependencies,
                    offset = 0,
                    size = sourceBytes.Length,
                    directories = configMap[bundleNames[i]].directories,
                    extension = configMap[bundleNames[i]].extension,
                    md5 = Util.MD5(sourceBytes),
                });
            }
        }

        bundleManifestFile.items = items.ToArray();
        AssetDatabase.ImportAsset(Setting.StreamingBundleRoot, ImportAssetOptions.ForceUpdate);

        var jsonTexts = JsonUtility.ToJson(bundleManifestFile);
        var listFile = FileUtil.CombinePaths(Setting.EditorBundleBuildCachePath, "rc.txt");
        File.WriteAllText(listFile, jsonTexts);

        var manifestFilePath = FileUtil.CombinePaths(versionPatchFilePath, "rc.bytes");
        File.WriteAllText(manifestFilePath, jsonTexts);

        File.WriteAllText(FileUtil.CombinePaths(patchFilePath, "v.bytes"), resourceVersion.ToString() + "," + Util.MD5(File.ReadAllBytes(manifestFilePath)));

        var compressed = new MemoryStream();
        ZipOutputStream compressor = new ZipOutputStream(compressed);
        var fileMap = Directory.GetFiles(patchFilePath, "*.*", SearchOption.AllDirectories);
        foreach (var file in fileMap)
        {
            var _filename = file.Substring(patchFilePath.Length, file.Length - patchFilePath.Length);
            var _entry = new ZipEntry(_filename);
            _entry.DateTime = new DateTime();
            _entry.DosTime = 0;
            compressor.PutNextEntry(_entry);
            if (Directory.Exists(file))
            {
                continue;
            }
            var _bytes = File.ReadAllBytes(file);
            var offset = 0;
            compressor.Write(_bytes, offset, _bytes.Length - offset);
        }
        if (patchingNoteList.Length > 0)
        {
            var filename = "NOTE_" + resourceVersion + ".txt";
            var entry = new ZipEntry(filename);
            entry.DateTime = new DateTime();
            entry.DosTime = 0;
            compressor.PutNextEntry(entry);
            var bytes = System.Text.UTF8Encoding.Default.GetBytes(patchingNoteList.ToString());
            compressor.Write(bytes, 0, bytes.Length);
        }
        
        compressor.Finish();
        compressed.Flush();

        if (!Directory.Exists(Setting.EditorPatchPath))
        {
            Directory.CreateDirectory(Setting.EditorPatchPath);
        }
        var fileBytes = new byte[compressed.Length];
        Array.Copy(compressed.GetBuffer(), fileBytes, fileBytes.Length);
        var fileName = string.Format("{0}/{1}-{2}-{3}.zip",
            Setting.EditorPatchPath,
            resourceVersion,
            DateTime.Now.ToString("yyyy.MM.dd_HH.mm.s"),
            Util.MD5(fileBytes));
        using(var fs = new FileStream(fileName, FileMode.Create, FileAccess.Write))
        {
            fs.Write(fileBytes, 0, fileBytes.Length);
        }

        AssetDatabase.Refresh();
    }

#endif

}
