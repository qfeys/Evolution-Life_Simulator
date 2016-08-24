using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System;

public class PromptWindow {

    GameObject window;

    public PromptWindow(Canvas canvas, GameObject standardWindow, GameObject standardbutton, string text, List<KeyValuePair<string, Action>> actions)
    {
        window = UnityEngine.Object.Instantiate(standardWindow);
        window.transform.SetParent(canvas.transform);
        window.transform.localPosition = Vector2.zero;
        window.transform.GetChild(0).GetComponent<Text>().text = text;
        foreach (var action in actions)
        {
            var button = UnityEngine.Object.Instantiate(standardbutton);
            button.transform.SetParent(window.transform.GetChild(1));
            button.transform.GetChild(0).GetComponent<Text>().text = action.Key;
            button.GetComponent<Button>().onClick.AddListener(Destroy);
            button.GetComponent<Button>().onClick.AddListener(new UnityEngine.Events.UnityAction(action.Value));
        }
    }

    public PromptWindow(Canvas canvas, GameObject standardWindow, GameObject standardbutton, GameObject standardInputField,
        string headText, string initInput, string buttonText, Action<string> action)
    {
        window = UnityEngine.Object.Instantiate(standardWindow);
        window.transform.SetParent(canvas.transform);
        window.transform.localPosition = Vector2.zero;
        window.transform.GetChild(0).GetComponent<Text>().text = headText;

        var inputfield = UnityEngine.Object.Instantiate(standardInputField);
        inputfield.transform.SetParent(window.transform.GetChild(1));
        inputfield.GetComponent<InputField>().text = initInput;
        inputfield.GetComponent<InputField>().onEndEdit.AddListener(new UnityEngine.Events.UnityAction<string>(action));
        inputfield.GetComponent<InputField>().onEndEdit.AddListener(s => Destroy());

        var button = UnityEngine.Object.Instantiate(standardbutton);
        button.transform.SetParent(window.transform.GetChild(1));
        button.transform.GetChild(0).GetComponent<Text>().text = buttonText;
        button.GetComponent<Button>().onClick.AddListener(inputfield.GetComponent<InputField>().ActivateInputField);

    }

    void Destroy()
    {
        UnityEngine.Object.Destroy(window);
    }
}
