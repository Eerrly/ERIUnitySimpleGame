using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ���밴��
/// </summary>
public class InputKeyCode
{
    public string _name;
    public bool _state = false;
    public bool _enable = true;

    /// <summary>
    /// ��ȡ��ǰ״̬
    /// </summary>
    public bool State
    {
        get
        {
            return _enable && (_state || InputManager.GetKey(_name));
        }
    }

    /// <summary>
    /// ���ÿ���
    /// </summary>
    /// <param name="enable">����</param>
    public void SetEnable(bool enable)
    {
        if (!enable)
        {
            _state = false;
        }
        _enable = enable;
    }

}

/// <summary>
/// ��ҵ�����
/// </summary>
public class PlayerInput : MonoBehaviour
{
    private FrameBuffer.Input _input;
    private Vector3 _moveInput;
    private byte _keyState;
    private Vector3 _lastMoveInput = Vector3.zero;

    public List<InputKeyCode> keys = new List<InputKeyCode>();

    /// <summary>
    /// ��Ӱ���
    /// </summary>
    /// <param name="key"></param>
    public void AddKey(InputKeyCode key)
    {
        keys.Add(key);
    }

    /// <summary>
    /// ͨ�����ID��ȡ��ǰ������
    /// </summary>
    /// <param name="playerId">���ID</param>
    /// <returns></returns>
    public FrameBuffer.Input GetPlayerInput(int playerId)
    {
        _input = new FrameBuffer.Input();
        _input.pos = (byte)playerId;
        _input.yaw = (byte)FixedMath.Format8DirInput(new FixedVector3(_moveInput));
        _input.key = _keyState;
        return _input;
    }

    void Update()
    {
        // ����
        byte _tmpKeyState = 0;
        for (var i = 0; i < keys.Count; ++i)
        {
            if (keys[i].State)
            {
                _tmpKeyState |= (byte)(1 << i);
                break;
            }
        }
        _keyState = _tmpKeyState;

        // ҡ��
        Vector3 moveInput = (InputManager.Vertical * Vector3.forward) + (InputManager.Horizontal * Vector3.right);
        if(_lastMoveInput != Vector3.zero && moveInput == Vector3.zero)
            _moveInput = _lastMoveInput;
        else
            _moveInput = moveInput.normalized;
        _lastMoveInput = moveInput.normalized;
    }

}
