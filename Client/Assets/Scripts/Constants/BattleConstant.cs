using UnityEngine;

public class BattleConstant
{
    public static int SelfID = 0;

    // ֡���
    public static readonly int FrameInterval = 30;

    // ���Prefab
    public static readonly string playerCharacterPath = "Prefabs/CompleteTank";
    // �������λ��
    public static readonly Vector3[] InitPlayerPos = { new Vector3(-3, 0, 30), new Vector3(13, 0, -5) };
    // ���������ת
    public static readonly Quaternion[] InitPlayerRot = { new Quaternion(0, -180, 0, 0), new Quaternion(0, 0, 0, 0) };
    // ���������ɫ
    public static readonly Color[] InitPlayerColor = { new Color((float)42/255, (float)100 /255, (float)178 /255), new Color((float)229/255, (float)46 /255, (float)40 /255) };

    // ��λ
    public static readonly string[] buttonNames = new string[] { "j", "k", "l" };

    // �������
    public static readonly uint randomSeed = 114514;

    // ���ӵĿ�
    public static readonly float spaceX = 100f;

    // ���ӵĸ�
    public static readonly float spaceZ = 100f;

    // ����Ƕ�
    public static readonly FixedNumber angle = FixedNumber.MakeFixNum(90 * FixedMath.DataConrvertScale, FixedMath.DataConrvertScale);

    // ���ӳߴ�
    public static readonly Vector3 cellSize = new Vector3(10f, 0, 10f);

}