﻿using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using System;
using System.Collections.Generic;

public class AssetBundleToolsEditorWin : OdinEditorWindow
{
    public static AssetBundleToolsEditorWin Open()
    {
        var win = GetWindow<AssetBundleToolsEditorWin>("AssetBundles Tools");
        var rect = GUIHelper.GetEditorWindowRect().AlignCenter(860, 700);
        return win;
    }

    private AssetBundleToolsConfig cfg;

    protected override void OnEnable()
    {
        base.OnEnable();
        LoadConfigFile();
    }

    private void LoadConfigFile()
    {
        cfg = Util.LoadConfig<AssetBundleToolsConfig>(Constant.ASSETBUNDLES_CONFIG_NAME);
        itemList.Clear();
        foreach (var item in cfg.items)
        {
            itemList.Add(new AssetBundleToolsItemEidtor(this, item));
        }
    }

    [HideLabel]
    [LabelText("资源数据")]
    public List<AssetBundleToolsItemEidtor> itemList = new List<AssetBundleToolsItemEidtor>();

    public struct AssetBundleToolsItemEidtor
    {
        AssetBundleToolsEditorWin win;

        private uint hash;
        private List<uint> dependencies;

        [LabelText("AssetBundle文件"), LabelWidth(100)]
        [HideLabel, FilePath]
        [HorizontalGroup("Zero")]
        public string assetBundleFile;

        [LabelText(",字节总长"), LabelWidth(50)]
        [HideLabel]
        [HorizontalGroup("One")]
        public int size;

        [LabelText("路径"), LabelWidth(50)]
        [HideLabel]
        [HorizontalGroup("Two")]
        public string packageResourcePath;

        [LabelText("所有的依赖"), LabelWidth(50)]
        [HideLabel]
        [HorizontalGroup("Four")]
        public List<AssetBundleToolsItemEidtor> assetBundleToolsItemEidtors;

        public AssetBundleToolsItemEidtor(AssetBundleToolsEditorWin win, AssetBundleToolsConfig.AssetBundleToolsConfigItem item)
        {
            this.win = win;

            hash = item.hash;
            size = item.size;
            packageResourcePath = item.packageResourcePath;
            dependencies = item.dependencies;

            assetBundleFile = FileUtil.CombinePaths(Setting.EditorBundleBuildCachePath, hash + ".s");
            assetBundleToolsItemEidtors = new List<AssetBundleToolsItemEidtor>();
            
            FindDependencies(ref assetBundleToolsItemEidtors);
        }

        private List<AssetBundleToolsItemEidtor> FindDependencies(ref List<AssetBundleToolsItemEidtor> assetBundleToolsItemEidtors)
        {
            foreach (var v in win.cfg.items)
            {
                if (dependencies.Contains(v.hash))
                {
                    assetBundleToolsItemEidtors.Add(new AssetBundleToolsItemEidtor(win, v));
                }
            }
            return assetBundleToolsItemEidtors;
        }

    }
}
