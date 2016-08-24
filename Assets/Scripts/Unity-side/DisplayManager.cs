using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class DisplayManager : MonoBehaviour
{
    static public DisplayManager TheOne;
    public GameObject StandardWindow;
    public GameObject StandardButton;
    public GameObject StandardInputField;
    Creature selection;

    public void Awake()
    {
        if (TheOne == null) TheOne = this;
    }

    public void Start()
    {
        setBlank();
    }

    public void setBlank()
    {
        transform.Find("Simulation progress").Find("Number").GetComponent<Text>().text = "--";
        transform.Find("Render progress").Find("Number").GetComponent<Text>().text = "--";
        transform.Find("Render backlog").Find("Number").GetComponent<Text>().text = "--";
        transform.Find("Living creatures").Find("Number").GetComponent<Text>().text = "--";
        transform.Find("Selection ID").Find("Number").GetComponent<Text>().text = "--";
        transform.Find("Energy").Find("Number").GetComponent<Text>().text = "--";
        transform.Find("Playback").Find("Number").GetComponent<Text>().text = "1.0";
    }

    public void setFields(int simProg, int renProg, int renBack, int LivCrea)
    {
        transform.Find("Simulation progress").Find("Number").GetComponent<Text>().text = simProg.ToString();
        transform.Find("Render progress").Find("Number").GetComponent<Text>().text = renProg.ToString();
        transform.Find("Render backlog").Find("Number").GetComponent<Text>().text = renBack.ToString();
        transform.Find("Living creatures").Find("Number").GetComponent<Text>().text = LivCrea.ToString();
    }

    internal void SetSelection(Creature selection, int id)
    {
        this.selection = selection;
        transform.Find("Selection ID").Find("Number").GetComponent<Text>().text = id.ToString();
        transform.Find("Energy").Find("Number").GetComponent<Text>().text = selection.energy.ToString("n2");
        NeuronDisplay.TheOne.SetSelection(selection);
    }

    public void OnGUI()
    {
        if (selection != null)
            transform.Find("Energy").Find("Number").GetComponent<Text>().text = selection.energy.ToString("n2");
    }

    internal void SetPlayback(float playbackModifier)
    {
        transform.Find("Playback").Find("Number").GetComponent<Text>().text = playbackModifier.ToString("n1");
    }

    internal void SetSaveButton(bool enable)
    {
        transform.Find("SaveButton").GetComponent<Button>().interactable = enable;
    }

    public void DisplaySaveSelectionPrompt()
    {
        new PromptWindow(transform.parent.GetComponent<Canvas>(), StandardWindow, StandardButton, "What do you want to save?",
            new List<KeyValuePair<string, Action>>() { new KeyValuePair<string, Action>("Best creature",() => God.TheOne.Save(0)),
            new KeyValuePair<string, Action>("Best 10 creatures", () => God.TheOne.Save(1)),
            new KeyValuePair<string, Action>("All creatures", () => God.TheOne.Save(2))});
    }

    public void DisplaySaveLocationPrompt(string standardPath, Action<string> saveAction)
    {
        new PromptWindow(transform.parent.GetComponent<Canvas>(), StandardWindow, StandardButton, StandardInputField, "Chose a location",
            standardPath, "Save", saveAction);
    }
}
