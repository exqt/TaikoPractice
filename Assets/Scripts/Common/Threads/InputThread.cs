using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Events;

public class InputThread : ThreadBase
{
#region P/Invoke
    [DllImport("user32.dll")]
    private static extern short GetAsyncKeyState(int vKey);
    #endregion

    readonly Dictionary<TaikoKeyType, char[]> keyToChars = new Dictionary<TaikoKeyType, char[]>()
    {
        { TaikoKeyType.LEFT_KA, new char[] { 'E', 'D' } },
        { TaikoKeyType.LEFT_DON, new char[] { 'R', 'F' } },
        { TaikoKeyType.RIGHT_DON, new char[] { 'I', 'J' } },
        { TaikoKeyType.RIGHT_KA, new char[] { 'O', 'K' } }
    };

    readonly bool[] state = new bool[256];

    public Action<TaikoKeyType> OnKeyPressed;
    public Action OnUpdate;

    public InputThread()
    {
        Debug.Log("InputThread created");
    }

    //  값
    // 0x0000 이전에 누른 적이 없고 호출 시점에도 눌려있지 않은 상태
    // 0x0001 이전에 누른 적이 있고 호출 시점에는 눌려있지 않은 상태
    // 0x8000 이전에 누른 적이 없고 호출 시점에는 눌려있는 상태
    // 0x8001 이전에 누른 적이 있고 호출 시점에도 눌려있는 상태
    protected override void Update()
    {
        foreach (var pair in keyToChars)
        {
            var key = (int)pair.Key;
            foreach (var c in pair.Value)
            {
                var prevState = state[c];
                var currState = (GetAsyncKeyState(c) & 0x8000) != 0;

                if (!prevState && currState)
                {
                    OnKeyPressed?.Invoke(pair.Key);
                }

                state[c] = currState;
            }
        }

        OnUpdate?.Invoke();
    }
}
