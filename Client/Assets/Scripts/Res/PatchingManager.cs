﻿using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

/// <summary>
/// 是否热更的状态
/// </summary>
enum CanDownloadState
{
    Confirm = 1,
    Cancel,
}

/// <summary>
/// 热更管理器
/// </summary>
public class PatchingManager : MonoBehaviour, IManager
{
    public bool IsInitialized { get; set; }

    /// <summary>
    /// 本地V文件路径
    /// </summary>
    private string vBytesFilePath;

    /// <summary>
    /// 本地RC文件路径
    /// </summary>
    private string rcBytesFilePath;

    private XLua.LuaFunction _callback;

    /// <summary>
    /// 本地版本号
    /// </summary>
    private string localVersionText = "";

    /// <summary>
    /// 远端版本号
    /// </summary>
    private string remoteVersionText = "";

    /// <summary>
    /// 是否要保存RC文件
    /// </summary>
    private bool bSaveRc = false;

    /// <summary>
    /// 热更需要下载的资源文件列表
    /// </summary>
    private List<ManifestItem> downloadList = null;

    /// <summary>
    /// 热更
    /// </summary>
    /// <param name="remoteUrl">远端链接地址</param>
    /// <param name="callback">Lua回调函数</param>
    /// <param name="o">Lua对象</param>
    public async void CoPatching(string remoteUrl, XLua.LuaFunction callback, object o)
    {
        FileUtil.CreateDirectory(Setting.CacheBundleRoot);

        _callback = callback;

        downloadList = new List<ManifestItem>();
        var patchingResult = false;
        var localMd5Map = new Dictionary<uint, string>();
        localVersionText = File.Exists(vBytesFilePath) ? File.ReadAllText(vBytesFilePath) : "";
        bSaveRc = string.IsNullOrEmpty(localVersionText);

        var remoteVerPath = FileUtil.CombinePaths(remoteUrl, "v.bytes");
        // 获取远端V文件版本号，对比本地V文件版本号比较
        await Global.Instance.HttpManager.CoHttpGet(remoteVerPath, 5, (state, text) =>
        {
            if (!state)
            {
#if UNITY_DEBUG
                Logger.Log(LogLevel.Error, $"热更网络获取 {remoteVerPath} 错误!!! 错误信息 : {text}");
#endif
            }
            else
            {
#if UNITY_DEBUG
                Logger.Log(LogLevel.Info, $"热更网络获取成功!! 获取结果 : {text}");
#endif
                remoteVersionText = text.Split(',')[0];
                bSaveRc = bSaveRc || localVersionText != remoteVersionText;
            }
        });
        if (bSaveRc)
        {
            if (File.Exists(rcBytesFilePath))
            {
                var localConfJson = System.Text.ASCIIEncoding.Default.GetString(File.ReadAllBytes(rcBytesFilePath));
                var localConf = Newtonsoft.Json.JsonConvert.DeserializeObject<ManifestConfig>(localConfJson);
                foreach (var item in localConf.items)
                {
                    localMd5Map.Add(item.hash, item.md5);
                }
            }

            var remoteVersion = remoteVersionText.Split(',')[0];
            var tmpLocalRcFilePath = rcBytesFilePath + ".tmp";
            var remoteRcPath = FileUtil.CombinePaths(remoteUrl, remoteVersion, "rc.bytes");
            // 下载RC文件
            await Global.Instance.HttpManager.CoHttpDownload(remoteRcPath, tmpLocalRcFilePath, false, null, (state, text) =>
            {
                if (!state)
                {
#if UNITY_DEBUG
                    Logger.Log(LogLevel.Error, $"热更网络下载 {remoteRcPath} 错误!!! 错误信息 : {text}");
#endif
                }
            });
            if (File.Exists(tmpLocalRcFilePath))
            {
                // 从RC文件中拿到需要下载的资源列表，比较MD5，如果不同则加入到下载集合中
                var tmpConfJson = System.Text.ASCIIEncoding.Default.GetString(File.ReadAllBytes(tmpLocalRcFilePath));
                var tmpConf = Newtonsoft.Json.JsonConvert.DeserializeObject<ManifestConfig>(tmpConfJson);
                foreach (var item in tmpConf.items)
                {
                    var tmpMd5 = "";
                    if (!localMd5Map.TryGetValue(item.hash, out tmpMd5) || tmpMd5 != item.md5)
                    {
                        FileUtil.DeleteFile(FileUtil.CombinePaths(Setting.CacheBundleRoot, item.hash + ".s"));
                        downloadList.Add(item);
                    }
                }

                // 等待Lua回应是否同意下载热更
                if(downloadList.Count > 0)
                {
                    await CallLuaPatchDownloadInfo(o);
                }

                // 开始下载热更
                var remoteFilePath = "";
                var localFilePath = "";
                while (downloadList.Count > 0)
                {
                    for (int i = downloadList.Count - 1; i >= 0; --i)
                    {
                        remoteFilePath = FileUtil.CombinePaths(remoteUrl, remoteVersion, downloadList[i].hash + ".s");
                        localFilePath = FileUtil.CombinePaths(Setting.CacheBundleRoot, downloadList[i].hash + ".s");
                        await Global.Instance.HttpManager.CoHttpDownload(
                            remoteFilePath,
                            localFilePath,
                            true,
                            (progress) =>
                            {
#if UNITY_DEBUG
                                Logger.Log(LogLevel.Info, $"热更网络下载 {remoteFilePath} 进度 : {progress}");
#endif
                                _callback?.Call(o, "donwload", progress);
                            },
                            (state, text) =>
                            {
                                if (!state)
                                {
#if UNITY_DEBUG
                                    Logger.Log(LogLevel.Error, $"热更网络下载 {remoteFilePath} 错误!!! 错误信息 : {text}");
#endif
                                }
                                else
                                {
                                    downloadList.RemoveAt(i);
                                }
                            });
                    }
                }

                // 替换新RC文件
                File.Copy(tmpLocalRcFilePath, rcBytesFilePath, true);
                FileUtil.DeleteFile(tmpLocalRcFilePath);

                // 替换新V文件
                File.WriteAllText(vBytesFilePath, remoteVersionText);
                patchingResult = true;
            }
        }
        if (patchingResult)
        {
            callback?.Call(o, "done");
            Global.Instance.OnPatchingDone?.Invoke();
        }
    }

    /// <summary>
    /// 等待Lua返回是否同意下载
    /// </summary>
    /// <param name="o">Lua对象</param>
    /// <returns></returns>
    private System.Collections.IEnumerator CallLuaPatchDownloadInfo(object o)
    {
        var bytes = 0;
        foreach (var file in downloadList)
        {
            bytes += file.size;
        }
        _callback?.Call(o, "setdownload", downloadList.Count, bytes);

        var canDownload = true;
        while (true)
        {
            var state = _callback?.Func<object, string, int>(o, "candownload");
            if (state == (int)CanDownloadState.Confirm)
            {
                canDownload = true;
                break;
            }
            else if (state == (int)CanDownloadState.Cancel)
            {
                canDownload = false;
                break;
            }
            yield return null;
        }
        if (!canDownload)
        {
            _callback?.Call(o, "error");
        }
    }

    /// <summary>
    /// 初始化
    /// </summary>
    public void OnInitialize()
    {
        vBytesFilePath = FileUtil.CombinePaths(Setting.CacheBundleRoot, "v.bytes");
        rcBytesFilePath = FileUtil.CombinePaths(Setting.CacheBundleRoot, "rc.bytes");
        IsInitialized = true;
    }

    /// <summary>
    /// 释放
    /// </summary>
    public void OnRelease()
    {
        IsInitialized = false;
        _callback = null;
        bSaveRc = false;
        downloadList.Clear();
        downloadList = null;
    }
}
