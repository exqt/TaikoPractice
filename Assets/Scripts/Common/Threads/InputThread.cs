using System;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Events;

public class InputThread : ThreadBase
{
#region P/Invoke
    [DllImport("user32.dll")]
    private static extern short GetAsyncKeyState(int vKey);
    #endregion

    readonly TaikoKeyType[] keysToRead = new TaikoKeyType[]
    {
        TaikoKeyType.LEFT_KA,
        TaikoKeyType.LEFT_DON,
        TaikoKeyType.RIGHT_DON,
        TaikoKeyType.RIGHT_KA
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
        for (int i = 0; i < keysToRead.Length; i++)
        {
            var key = (int)keysToRead[i];
            var prevState = state[key];
            var currState = (GetAsyncKeyState(key) & 0x8000) != 0;

            if (!prevState && currState)
            {
                OnKeyPressed?.Invoke(keysToRead[i]);
            }

            state[key] = currState;
        }

        OnUpdate?.Invoke();
    }
}
