using System.IO;
using System.Collections.Generic;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities.Editor;
using Sirenix.Utilities;
using Sirenix.OdinInspector;
using UnityEditor;
using System.Linq;

public class PatchToolsEditorWin : OdinEditorWindow
{

    public static PatchToolsEditorWin Open()
    {
        var win = GetWindow<PatchToolsEditorWin>("Patch Tools");
        var rect = GUIHelper.GetEditorWindowRect().AlignCenter(860, 700);
        return win;
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        startVersion = UnityEngine.PlayerPrefs.GetString("PATCH_TOOLS_START_VERSION", "202e42c3");
        endVersion = UnityEngine.PlayerPrefs.GetString("PATCH_TOOLS_END_VERSION", "ce5db4c7");
    }

    [HideLabel, LabelWidth(150)]
    [LabelText("��ʼ�汾��StartVersion��")]
    public string startVersion = "";

    [HideLabel, HorizontalGroup("EndVersion"), LabelWidth(150)]
    [LabelText("�����汾��EndVersion��")]
    public string endVersion = "";

    [HorizontalGroup("EndVersion")]
    [Button("Head")]
    public void GetGitHeadVersion()
    {
        endVersion = PatchUtil.GetGitVersion();
    }

    [HideLabel, ReadOnly]
    [Title("�ȸ��ļ��б�")]
    [LabelText("�б�")]
    public string[] patchFiles;

    [Button("��ȡ�ȸ��ļ��б�", ButtonSizes.Large)]
    public void GetPatchFiles()
    {
        patchFiles = PatchUtil.GetPatchFiles(startVersion, endVersion);
        if (patchFiles != null)
        {
            this.ShowTip("��ȡ�ɹ���");
        }
    }

    [Button("��ʼ�ȸ�", ButtonSizes.Large)]
    public void PatchFiles()
    {
        this.ShowTip("TODO");

        UnityEngine.PlayerPrefs.SetString("PATCH_TOOLS_START_VERSION", startVersion);
        UnityEngine.PlayerPrefs.SetString("PATCH_TOOLS_END_VERSION", endVersion);
    }

}