using UnityEngine;

public class BattleConstant
{
    /// <summary>
    /// �Լ���ID
    /// </summary>
    public static int SelfID = 0;

    /// <summary>
    /// ֡���(����)
    /// </summary>
    public static readonly int FrameInterval = 33;

    /// <summary>
    /// ���ͻ�������
    /// </summary>
    public static readonly int MaxClientCount = 2;

    /// <summary>
    /// �������(֡)
    /// </summary>
    public static readonly int HeartBeatFrame = 100;

    /// <summary>
    /// ���Prefab
    /// </summary>
    public static readonly string playerCharacterPath = "Prefabs/Cube";

    /// <summary>
    /// �������λ��
    /// </summary>
    public static readonly Vector3[] InitPlayerPos = { new Vector3(-3, 0, 30), new Vector3(13, 0, -5) };

    /// <summary>
    /// ���������ת
    /// </summary>
    public static readonly Quaternion[] InitPlayerRot = { new Quaternion(0, -180, 0, 0), new Quaternion(0, 0, 0, 0) };

    /// <summary>
    /// ���������ɫ
    /// </summary>
    public static readonly Color[] InitPlayerColor = { new Color((float)42/255, (float)100 /255, (float)178 /255), new Color((float)229/255, (float)46 /255, (float)40 /255) };

    /// <summary>
    /// ��λ
    /// </summary>
    public static readonly string[] buttonNames = new string[] { "j", "k", "l" };

    /// <summary>
    /// �������
    /// </summary>
    public static readonly uint randomSeed = 114514;

    /// <summary>
    /// ���ӵĿ�
    /// </summary>
    public static readonly float spaceX = 100f;

    /// <summary>
    /// ���ӵĳ�
    /// </summary>
    public static readonly float spaceZ = 100f;

    /// <summary>
    /// ����Ƕ�
    /// </summary>
    public static readonly FixedNumber angle = FixedNumber.MakeFixNum(90 * FixedMath.DataConrvertScale, FixedMath.DataConrvertScale);

    /// <summary>
    /// ���ӳߴ�
    /// </summary>
    public static readonly Vector3 cellSize = new Vector3(10f, 0, 10f);

}