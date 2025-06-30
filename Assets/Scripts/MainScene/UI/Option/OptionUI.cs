using System;
using System.Collections;
using System.Linq;
using System.Linq.Expressions;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIOption : MonoBehaviour, ITaikoUIMove
{
    Type valueType;
    public Button leftButton;
    public Button rightButton;

    public TMP_Text labelText;
    public TMP_Text valueText;

    public Image backgroundImage;

    [NonSerialized]
    public string fieldName;

    OptionGroup optionGroup;
    UIOptionGroup uiGroup;

    public enum OptionType
    {
        Default,
        Percent1,
        Percent2,
        TimeMS,
        Float1,
        Float2,
    }

    public OptionType optionType = OptionType.Default;

    Func<object> getter;
    Func<object, object> setter;

#region Data
    object[] choices = new string[] { "" };
    int currentIndex = 0;
#endregion

    void Awake()
    {
        uiGroup = GetComponentInParent<UIOptionGroup>();

        if (uiGroup == null)
        {
            Debug.LogError("UIOptionGroup not found in parent.");
            return;
        }

        InitializeUI();
    }

    public object GetValue()
    {
        if (valueType == null)
        {
            Debug.LogError("Value type is not set.");
            return null;
        }

        return Convert.ChangeType(choices[currentIndex % choices.Length], valueType);
    }

    public void SetData(string label, Type valueType, object[] choices)
    {
        this.valueType = valueType;
        this.choices = choices;

        labelText.SetText(label);
    }

    public UIOption BindField(OptionGroup group, string fieldName)
    {
        optionGroup = group;
        this.fieldName = fieldName;

        var field = group.GetType().GetField(fieldName);
        if (field == null)
        {
            Debug.LogError($"Field {fieldName} not found in {group.GetType().Name}");
            return this;
        }

        // Get the attribute to set label and choices
        var attributes = (OptionDescAttribute[])field.GetCustomAttributes(typeof(OptionDescAttribute), false);
        if (attributes.Length > 0)
        {
            var attr = attributes[0];
            labelText.text = attr.label;

            // If choicesName is set, try to get the choices from OptionChoices
            if (!string.IsNullOrEmpty(attr.choicesName))
            {
                var choicesField = typeof(OptionChoices).GetField(
                    attr.choicesName,
                    System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);

                if (choicesField != null)
                {
                    // Convert the strongly typed array to object[]
                    var choicesArray = choicesField.GetValue(null);
                    if (choicesArray is Array array)
                    {
                        object[] objectArray = new object[array.Length];
                        for (int i = 0; i < array.Length; i++)
                        {
                            objectArray[i] = array.GetValue(i);
                        }
                        choices = objectArray;
                    }
                }
                else
                {
                    Debug.LogWarning($"Choices field '{attr.choicesName}' not found in OptionChoices for field {fieldName}");
                }
            }
        }

        // Create getter and setter using reflection
        getter = () => field.GetValue(group);

        // Handle nullable types and type conversion
        setter = (value) =>
        {
            var fieldType = field.FieldType;
            // Check if field is nullable
            if (fieldType.IsGenericType && fieldType.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                fieldType = Nullable.GetUnderlyingType(fieldType);
            }
            field.SetValue(group, Convert.ChangeType(value, fieldType));
            return value;
        };

        // Initialize value from field
        object currentValue = getter();
        if (currentValue != null)
        {
            valueText.text = ConvertValueToString(currentValue);
            currentIndex = Array.IndexOf(choices, currentValue);
            if (currentIndex == -1) currentIndex = 0;
        }

        return this;
    }

    public string ConvertValueToString(object value)
    {
        if (value == null) return string.Empty;

        // Convert value Based on option type
        switch (optionType)
        {
            case OptionType.Percent1:
                return $"{Convert.ToDouble(value) * 100:F1}%";
            case OptionType.Percent2:
                return $"{Convert.ToDouble(value) * 100:F2}%";
            case OptionType.TimeMS:
                return $"{Convert.ToInt32(value)} ms";
            case OptionType.Float1:
                return $"{Convert.ToSingle(value):F1}";
            case OptionType.Float2:
                return $"{Convert.ToSingle(value):F2}";
            default:
                return value.ToString();
        }
    }

    public UIOption SetGetter(Func<object> getter)
    {
        this.getter = getter;
        return this;
    }

    public UIOption SetSetter(Func<object, object> setter)
    {
        this.setter = setter;
        return this;
    }

    public UIOption SetOptionGroup(OptionGroup optionGroup)
    {
        this.optionGroup = optionGroup;
        return this;
    }

    public UIOption SetChoices<T>(T[] choices)
    {
        this.choices = choices.Cast<object>().ToArray();
        return this;
    }

    public OptionDescAttribute GetDesc()
    {
        var field = optionGroup.GetType().GetField(fieldName);
        if (field == null)
        {
            Debug.LogError($"Field {fieldName} not found in {optionGroup.GetType().Name}.");
            return null;
        }

        var attributes = (OptionDescAttribute[])field.GetCustomAttributes(typeof(OptionDescAttribute), false);
        if (attributes.Length == 0)
        {
            Debug.LogError($"OptionDescAttribute not found on field {fieldName}.");
            return null;
        }

        var attribute = attributes[0];
        return attribute;
    }

    public void SetValue(object value)
    {
        if (optionGroup == null || fieldName == null) return;
        if (value == null) return;

        var field = optionGroup.GetType().GetField(fieldName);
        if (field == null) return;

        // get generic type of Nullable
        var fieldType = field.FieldType.IsGenericType && field.FieldType.GetGenericTypeDefinition() == typeof(Nullable<>)
            ? Nullable.GetUnderlyingType(field.FieldType)
            : field.FieldType;
        field.SetValue(optionGroup, Convert.ChangeType(value, fieldType));

        currentIndex = Array.IndexOf(choices, value);

        if (currentIndex == -1)
        {
            // add value to choices if not found
            var newChoices = new object[choices.Length + 1];
            Array.Copy(choices, newChoices, choices.Length);
            newChoices[choices.Length] = value;

            if (fieldType == typeof(int) || fieldType == typeof(long) || fieldType == typeof(short))
            {
                Array.Sort(newChoices, (x, y) => Convert.ToInt64(x).CompareTo(Convert.ToInt64(y)));
            }
            else if (fieldType == typeof(float) || fieldType == typeof(double))
            {
                Array.Sort(newChoices, (x, y) => Convert.ToDouble(x).CompareTo(Convert.ToDouble(y)));
            }

            choices = newChoices;

            currentIndex = Array.IndexOf(choices, value);
        }

        object[] objectChoices = choices.Select(x => (object)x).ToArray();
        valueText.text = ConvertValueToString(choices[currentIndex % choices.Length].ToString());
    }

    void InitializeUI()
    {
        backgroundImage.color =
            new Color(backgroundImage.color.r, backgroundImage.color.g, backgroundImage.color.b, 0.0f);
        ShowArrow(false);
    }

    void ShowArrow(bool show)
    {
        if (show)
        {
            leftButton.gameObject.SetActive(true);
            rightButton.gameObject.SetActive(true);
        }
        else
        {
            leftButton.gameObject.SetActive(false);
            rightButton.gameObject.SetActive(false);
        }
    }

    public void OnMoveLeft()
    {
        currentIndex = (currentIndex - 1 + choices.Length) % choices.Length;

        var value = choices[currentIndex];
        valueText.text = ConvertValueToString(value);
        SetValue(value);
    }

    public void OnMoveRight()
    {
        currentIndex = (currentIndex + 1) % choices.Length;

        var value = choices[currentIndex];
        valueText.text = ConvertValueToString(value);
        SetValue(value);
    }

    public void OnTaikoUIMoveLeft() => OnMoveLeft();
    public void OnTaikoUIMoveRight() => OnMoveRight();
    public void OnTaikoUISelect() => uiGroup.NextOption();

    public void OnTaikoUIMoveEnter()
    {
        var color = backgroundImage.color;
        color.a = 1f;
        backgroundImage.color = color;
        ShowArrow(true);
    }

    public void OnTaikoUIMoveLeave()
    {
        var color = backgroundImage.color;
        color.a = 0f;
        backgroundImage.color = color;
        ShowArrow(false);
    }
}
