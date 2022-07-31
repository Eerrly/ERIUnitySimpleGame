using UnityEngine;

/// <summary>
/// ���������
/// </summary>
public static class InputManager
{
    /// <summary>
    /// ����
    /// </summary>
    public static bool enabled = true;

    /// <summary>
    /// Ĭ�ϵİ���״̬ ��J��K��L��
    /// </summary>
    public static bool[] defaultKeies = new bool[] { false, false, false, false };

    /// <summary>
    /// ����
    /// </summary>
    public static float Vertical
    {
        get
        {
            if (!enabled)
                return 0;
            return Input.GetAxisRaw(InputConstant.Vertical);
        }
    }

    /// <summary>
    /// ����
    /// </summary>
    public static float Horizontal
    {
        get
        {
            if (!enabled)
                return 0;
            return Input.GetAxisRaw(InputConstant.Horizontal);
        }
    }

    /// <summary>
    /// �Ƿ񰴼�
    /// </summary>
    /// <param name="name">����</param>
    /// <returns>�Ƿ񰴼�</returns>
    public static bool GetKey(string name)
    {
        if (!enabled)
            return false;
        var state = Input.GetKey(name);
        if (!state)
        {
            if (InputConstant.KeyCodeSpace.Equals(name))
            {
                state = defaultKeies[0];
            }
            else if (InputConstant.KeyCodeJ.Equals(name))
            {
                state = defaultKeies[1];
            }
            else if (InputConstant.KeyCodeK.Equals(name))
            {
                state = defaultKeies[2];
            }
            else if (InputConstant.KeyCodeL.Equals(name))
            {
                state = defaultKeies[3];
            }
        }
        return state;
    }

    public static void Reset()
    {
        for (var i = 0; i < defaultKeies.Length; ++i)
        {
            defaultKeies[i] = false;
        }
    }

}
