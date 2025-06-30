using System;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

public class UIOptionGroup : MonoBehaviour, ITaikoUIMove
{
    UIMainListItemContent uIMainListItemContent;

    [NonSerialized]
    public UIOption[] options;

    public string groupName;

    OptionGroup OptionGroup;

    UIOption selectedOption;

    int selectedIndex = -1;

    public UnityAction OnEnd;

    public OptionGroup GetOptionGroup()
    {
        return OptionGroup;
    }

    public void SetValues(OptionGroup group)
    {
        if (group == null) return;

        for (int i = 0; i < options.Length; i++)
        {
            var field = group.GetType().GetField(options[i].fieldName);
            if (field != null)
            {
                var value = field.GetValue(group);
                options[i].SetValue(value);
            }
        }
    }

    public void BindOptions(OptionGroup group)
    {
        OptionGroup = group;

        // Get fields with OptionDesc attribute
        var optionFields = group.GetType().GetFields()
            .Where(field => field.GetCustomAttributes(typeof(OptionDescAttribute), false).Any())
            .ToArray();

        // Make sure we have enough option UI elements
        if (optionFields.Length > options.Length)
        {
            Debug.LogWarning($"Not enough option UI elements for all fields in {group.GetType().Name}");
        }

        // Bind each field to an option UI
        for (int i = 0; i < Math.Min(optionFields.Length, options.Length); i++)
        {
            var field = optionFields[i];
            var fieldName = field.Name;

            // Get the appropriate choices array based on field type/name
            object[] choices = null;

            // Try to find a matching choices array in OptionChoices            // Check for a choicesName in the OptionDesc attribute
            var attributes = (OptionDescAttribute[])field.GetCustomAttributes(typeof(OptionDescAttribute), false);
            string choicesName = null;
            if (attributes.Length > 0 && !string.IsNullOrEmpty(attributes[0].choicesName))
            {
                choicesName = attributes[0].choicesName;
            }
            else
            {
                // Fall back to the field name with first letter capitalized
                choicesName = char.ToUpper(fieldName[0]) + fieldName.Substring(1);
            }

            var choicesField = typeof(OptionChoices).GetField(
                choicesName,
                System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);

            if (choicesField != null)
            {                // Convert the strongly typed array to object[]
                var choicesArray = choicesField.GetValue(null);
                if (choicesArray is Array array)
                {
                    object[] objectArray = new object[array.Length];
                    for (int j = 0; j < array.Length; j++)
                    {
                        objectArray[j] = array.GetValue(j);
                    }
                    choices = objectArray;
                }
            }
            else if (field.FieldType.IsEnum || (Nullable.GetUnderlyingType(field.FieldType)?.IsEnum == true))
            {
                // If it's an enum type, use the enum values
                var enumType = field.FieldType.IsEnum ? field.FieldType : Nullable.GetUnderlyingType(field.FieldType);
                choices = Enum.GetValues(enumType).Cast<object>().ToArray();
            }

            options[i].BindField(group, fieldName);
            if (choices != null)
            {
                options[i].SetChoices(choices);
            }
        }
    }

    void Awake()
    {
        uIMainListItemContent = GetComponentInParent<UIMainListItemContent>();
        options = GetComponentsInChildren<UIOption>();

        switch (groupName)
        {
            case "PatternPractice":
                var globalPlayOption = PatternPracticeOptionGroup.Load();
                BindOptions(globalPlayOption);
                break;
            case "SystemSetting":
                var systemSetting = SystemOptionGroup.Load();
                BindOptions(systemSetting);
                break;
            case "Recording":
                BindOptions(new RecordingOptionGroup());
                break;
            default:
                Debug.LogError($"Unknown group name: {groupName}");
                break;
        }
    }

    public void OnTaikoUIMoveLeft()
    {
        if (selectedIndex == -1) uIMainListItemContent.SubUIMoveLeft();
        else selectedOption.OnTaikoUIMoveLeft();
    }

    public void OnTaikoUIMoveRight()
    {
        if (selectedIndex == -1) uIMainListItemContent.SubUIMoveRight();
        else selectedOption.OnTaikoUIMoveRight();
    }

    public virtual void OnTaikoUISelect()
    {
        NextOption();
    }

    public void NextOption()
    {
        selectedIndex++;
        if (selectedIndex >= options.Length) selectedIndex = -1;
        else
        {
            selectedOption = options[selectedIndex];
            TaikoUIMoveManager.Instance.SetItem(selectedOption.GetComponent<ITaikoUIMove>());
        }

        if (selectedIndex == -1)
        {
            TaikoUIMoveManager.Instance.SetItem(this);
            OnEnd?.Invoke();
            return;
        }
    }
}
