using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UI_InputWindow : MonoBehaviour
{
    private static UI_InputWindow instance;

    private Button okBtn;
    private Button cancelBtn;
    private TextMeshProUGUI titleText;
    private TMP_InputField inputField;

    private void Awake()
    {
        instance = this;

        okBtn = transform.Find("okBtn").GetComponent<Button>();
        cancelBtn = transform.Find("cancelBtn").GetComponent<Button>();
        titleText = transform.Find("titleText").GetComponent<TextMeshProUGUI>();
        inputField = transform.Find("inputField").GetComponent<TMP_InputField>();

        Hide();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter)) {
            ConfirmInput();
        }
        if (Input.GetKeyDown(KeyCode.Escape)) {
            CancelInput();
        }
    }

    private void Show(string title, string defaultValue, string validChars, int charLimit, Action onCancel, Action<string> onOk)
    {
        gameObject.SetActive(true);
        titleText.text = title;

        inputField.characterLimit = charLimit;
        inputField.onValidateInput = (text, index, addedChar) => ValidateChar(validChars, addedChar);
        inputField.text = defaultValue;
        inputField.Select();

        okBtn.onClick.RemoveAllListeners();
        okBtn.onClick.AddListener(() => ConfirmInput(onOk));

        cancelBtn.onClick.RemoveAllListeners();
        cancelBtn.onClick.AddListener(() => CancelInput(onCancel));
    }

    private void Hide()
    {
        gameObject.SetActive(false);
    }

    private void ConfirmInput(Action<string> onOk = null)
    {
        Hide();
        onOk?.Invoke(inputField.text);
    }

    private void CancelInput(Action onCancel = null)
    {
        Hide();
        onCancel?.Invoke();
    }

    private char ValidateChar(string validChars, char addedChar)
    {
        return validChars.Contains(addedChar) ? addedChar : '\0';
    }

    public static void Show_Static(string title, string defaultValue, string validChars, int charLimit, Action onCancel, Action<string> onOk)
    {
        instance.Show(title, defaultValue, validChars, charLimit, onCancel, onOk);
    }

    public static void Show_Static(string title, int defaultInt, Action onCancel, Action<int> onOk)
    {
        Show_Static(
            title,
            defaultInt.ToString(),
            "0123456789-",
            20,
            onCancel,
            input =>
            {
                if (int.TryParse(input, out int result))
                    onOk(result);
                else
                    onOk(defaultInt);
            }
        );
    }
}
