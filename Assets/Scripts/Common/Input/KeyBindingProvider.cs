using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

// Provides mapping from TaikoKeyType to Windows virtual key codes derived from InputActionAsset.
// Assumes keyboard bindings. Converts effective binding paths like <Keyboard>/a to virtual key codes via KeyCode.
public class KeyBindingProvider
{
    public static KeyBindingProvider Instance; // singleton

    public InputActionAsset asset; // assign in inspector

    readonly Dictionary<TaikoKeyType, int[]> keyMap = new();
    int version; // increment when rebuilt

    public KeyBindingProvider()
    {
        Instance = this;
        Rebuild();
    }

    // Returns snapshot for thread consumption (copy to avoid enumeration issues)
    public Dictionary<TaikoKeyType, int[]> GetSnapshot()
    {
        var copy = new Dictionary<TaikoKeyType, int[]>();
        foreach (var kv in keyMap) copy[kv.Key] = kv.Value;
        return copy;
    }

    public int GetVersion() { return version; }

    public void UpdateKeyBindings(InputActionAsset newAsset)
    {
        if (newAsset != null) asset = newAsset;
        Rebuild();
    }

    void Rebuild()
    {
        keyMap.Clear();
        if (asset == null) { version++; return; }
        TryAdd("Drum/LeftKa", TaikoKeyType.LEFT_KA);
        TryAdd("Drum/RightKa", TaikoKeyType.RIGHT_KA);
        TryAdd("Drum/LeftDon", TaikoKeyType.LEFT_DON);
        TryAdd("Drum/RightDon", TaikoKeyType.RIGHT_DON);
        version++;
    }

    void TryAdd(string actionName, TaikoKeyType keyType)
    {
        var action = asset.FindAction(actionName);
        if (action == null) return;
        var codes = new List<int>();
        for (int i = 0; i < action.bindings.Count; i++)
        {
            var b = action.bindings[i];
            if (!b.isComposite && b.effectivePath != null &&
                b.effectivePath.StartsWith("<Keyboard>/"))
            {
                var keyStr = b.effectivePath.Substring("<Keyboard>/".Length);
                if (TryGetVirtualKey(keyStr, out int vk)) codes.Add(vk);
            }
        }
        if (codes.Count > 0) keyMap[keyType] = codes.ToArray();
    }

    // Map key string (e.g. "a", "space") to Windows virtual key code.
    bool TryGetVirtualKey(string keyStr, out int vk)
    {
        vk = 0;
        if (string.IsNullOrEmpty(keyStr)) return false;
        keyStr = keyStr.ToLowerInvariant();

        // letters
        if (keyStr.Length == 1 && keyStr[0] >= 'a' && keyStr[0] <= 'z') { vk = char.ToUpperInvariant(keyStr[0]); return true; }
        // digits
        if (keyStr.Length == 1 && keyStr[0] >= '0' && keyStr[0] <= '9') { vk = keyStr[0]; return true; }

        // common special keys
        switch (keyStr)
        {
            case "space": vk = 0x20; return true;
            case "enter": vk = 0x0D; return true;
            case "tab": vk = 0x09; return true;
            case "escape": vk = 0x1B; return true;
            case "leftshift": vk = 0xA0; return true;
            case "rightshift": vk = 0xA1; return true;
            case "leftctrl": vk = 0xA2; return true;
            case "rightctrl": vk = 0xA3; return true;
            case "leftalt": vk = 0xA4; return true;
            case "rightalt": vk = 0xA5; return true;
        }
        return false;
    }
}
