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

    void Destroy()
    {
        UnityEngine.Object.Destroy(window);
    }
}
