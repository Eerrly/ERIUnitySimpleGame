using UnityEngine;

public class BattleConstant
{
    public static int SelfID = 0;

    // ֡���
    public static readonly int FrameInterval = 30;

    // �������λ�ü��
    public static readonly float normalPlayerPositionOffset = 10f;

    // ���Prefab
    public static readonly string playerCharacterPath = "Prefabs/Cube";

    // ��λ
    public static readonly string[] buttonNames = new string[] { "j", "k", "l" };

    // �������
    public static readonly uint randomSeed = 114514;

    // ���ӵĿ�
    public static readonly float spaceX = 100f;

    // ���ӵĸ�
    public static readonly float spaceZ = 100f;

    // ����Ƕ�
    public static readonly int angle = 90;

    // ���ӳߴ�
    public static readonly Vector3 cellSize = new Vector3(10f, 0, 10f);

    public static readonly float[] hpStates = new float[] { 0, 30.0f, 60.0f, 100 };

}