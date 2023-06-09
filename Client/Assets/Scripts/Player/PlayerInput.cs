using System.Collections.Generic;
using UnityEngine;

public class InputKeyCode
{
    public string _name;
    public bool _state = false;
    public bool _enable = true;

    public bool State
    {
        get
        {
            return _enable && (_state || InputManager.GetKey(_name));
        }
    }

    public void SetEnable(bool enable)
    {
        if (!enable)
        {
            _state = false;
        }
        _enable = enable;
    }

}

public class PlayerInput : MonoBehaviour
{
    private FrameBuffer.Input _input;
    private Vector3 _moveInput;
    private byte _keyState;
    private Vector3 _lastMoveInput = Vector3.zero;

    public List<InputKeyCode> keys = new List<InputKeyCode>();

    public void AddKey(InputKeyCode key)
    {
        keys.Add(key);
    }

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

        // ң��
        Vector3 moveInput = (InputManager.Vertical * Vector3.forward) + (InputManager.Horizontal * Vector3.right);
        if(_lastMoveInput != Vector3.zero && moveInput == Vector3.zero)
            _moveInput = _lastMoveInput;
        else
            _moveInput = moveInput.normalized;
        _lastMoveInput = moveInput.normalized;
    }

}
