using UnityEngine;

public static class MathManager
{
    /// <summary>
    /// Բ�ĽǶ����ֵ 360
    /// </summary>
    public static readonly FixedNumber AngleMax = FixedNumber.MakeFixNum(360, 1);

    /// <summary>
    /// ��Բ�Ƕ����ֵ 180
    /// </summary>
    public static readonly FixedNumber HalfAngleMax = AngleMax / 2;

    /// <summary>
    /// ����ƶ������Ƕȼ����ƫ��ֵ,PlayerEntity��InputComponent����е�yawֵΪFrameBuffer.Input�е�yaw��ȥYawOffset
    /// </summary>
    public const int YawOffset = 1;

    /// <summary>
    /// Ĭ��ҡ��
    /// </summary>
    public const int YawStop = -YawOffset;

    /// <summary>
    /// �ƶ������нǶ���
    /// </summary>
    public static readonly FixedNumber DivAngle = FixedNumber.MakeFixNum(450000, 10000);

    /// <summary>
    /// �ƶ������нǶ�����һ��
    /// </summary>
    public static readonly FixedNumber HalfDivAngle = DivAngle / 2;

    /// <summary>
    /// ͨ������
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    public static int Format8DirInput(FixedVector3 input)
    {
        input.y = FixedNumber.Zero;
        if(input.sqrMagnitudeLong > 0)
        {
            FixedVector3 dir = input.Normalized;
            FixedNumber angle = SignedAngle(FixedVector3.Forward, dir, FixedVector3.Up);

            angle = angle < 0 ? AngleMax + angle : angle;
            int div;
            if (angle <= HalfDivAngle || angle > AngleMax - HalfDivAngle)
            {
                div = 0;
            }
            else
            {
                var val = (angle - HalfDivAngle) / DivAngle;
                val += 1;
                div = val.ToInt();
            }
            return div + YawOffset;
        }
        return 0;
    }

    /// <summary>
    /// ͨ��ҡ�˻�ȡ��Ӧ����ת
    /// </summary>
    /// <param name="yaw">ҡ��</param>
    /// <returns>��ת��Ԫ��</returns>
    public static FixedQuaternion FromYaw(int yaw)
    {
        FixedQuaternion r = FixedQuaternion.Euler(FixedNumber.Zero, (yaw) * DivAngle, FixedNumber.Zero);
        return r;
    }

    /// <summary>
    /// ����ҡ�˶�Ӧ�ĵ�λ��������
    /// </summary>
    private static FixedVector3[] _cacheYawToVector3 = new FixedVector3[8];
    /// <summary>
    /// ͨ��ҡ�˻�ȡ��λ��������
    /// </summary>
    /// <param name="yaw">ҡ��</param>
    /// <returns>��λ��������</returns>
    private static FixedVector3 _FromYawToVector3(int yaw)
    {
        switch (yaw)
        {
            case 0: return new FixedVector3(FixedNumber.Zero, FixedNumber.Zero, FixedNumber.One);
            case 2: return new FixedVector3(FixedNumber.One, FixedNumber.Zero, FixedNumber.Zero);
            case 4: return new FixedVector3(FixedNumber.Zero, FixedNumber.Zero, -FixedNumber.One);
            case 6: return new FixedVector3(-FixedNumber.One, FixedNumber.Zero, FixedNumber.Zero);
        }
        FixedQuaternion rot = FromYaw(yaw);
        return rot * FixedVector3.Forward;
    }

    /// <summary>
    /// ͨ��ҡ�˻�ȡ��Ӧ�ĵ�λ��������
    /// </summary>
    /// <param name="yaw">ҡ��</param>
    /// <returns>��λ��������</returns>
    public static FixedVector3 FromYawToVector3(int yaw)
    {
        if (yaw < 0)
        {
            return FixedVector3.Zero;
        }
        yaw = yaw % 8;
        if (_cacheYawToVector3[yaw] == FixedVector3.Zero)
        {
            _cacheYawToVector3[yaw] = _FromYawToVector3(yaw);
        }
        return _cacheYawToVector3[yaw];
    }

    /// <summary>
    /// ��ȡ���������ļнǣ�������
    /// </summary>
    /// <param name="lhs">����</param>
    /// <param name="rhs">����</param>
    /// <param name="axis">��ת��</param>
    /// <returns>�н�</returns>
    public static FixedNumber SignedAngle(FixedVector3 lhs, FixedVector3 rhs, FixedVector3 axis)
    {
        FixedNumber num = FixedVector3.AngleInt(lhs, rhs);
        FixedVector3 rotateAxis = FixedVector3.Cross(lhs, rhs).Normalized;
        int num2 = Sign(FixedVector3.Dot(axis, rotateAxis, true));
        return (num * num2);
    }

    public static int Sign(FixedNumber value)
    {
        return value >= 0 ? 1 : -1;
    }

}
