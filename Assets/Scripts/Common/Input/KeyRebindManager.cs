using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine.InputSystem;
using UnityEngine;
using Debug = UnityEngine.Debug;
using TMPro;
using UnityEditor;
using System.IO;

public class KeyRebindManager : MonoBehaviour
{
    public InputActionAsset asset;
    public GameObject promptUI;
    public TMP_Text promptText;

    List<string> keysToBind = new List<string>
    {
        "Drum/LeftKa",
        "Drum/RightKa",
        "Drum/LeftDon",
        "Drum/RightDon"
    };

    static string KeyBindingPath = "Options/KeyBindings";

    void Awake()
    {
        promptUI.SetActive(false);
        if (FileManager.Instance.ExistsFile(KeyBindingPath))
        {
            var json = FileManager.Instance.ReadFile(KeyBindingPath);
            asset.LoadBindingOverridesFromJson(json);
            KeyBindingProvider.Instance.UpdateKeyBindings(asset);
            Debug.Log("Loaded key bindings from file.");
        }
    }

    public IEnumerator StartBind()
    {
        asset.Disable();
        promptUI.SetActive(true);

        foreach (var key in keysToBind)
        {
            var action = asset.FindAction(key, true);

            promptText.text = $"Press a key to bind for {key}";

            var rebind = action.PerformInteractiveRebinding()
                .WithControlsHavingToMatchPath("<Keyboard>")
                .OnMatchWaitForAnother(0.1f)
                .OnComplete(operation =>
                {
                    operation.Dispose();
                }).Start();

            // Wait until the rebind is complete
            while (!rebind.completed)
                yield return null;
        }

        asset.Enable();
        promptUI.SetActive(false);

        var json = asset.SaveBindingOverridesAsJson();
        FileManager.Instance.WriteFile(KeyBindingPath, json);

        KeyBindingProvider.Instance.UpdateKeyBindings(asset);
    }
}
