using UnityEngine;
using System.Collections;
using System;

public class NeuronDisplay : MonoBehaviour {
    public static NeuronDisplay TheOne;

    Creature selection;

	// Use this for initialization
	void Start () {
        if (TheOne == null) TheOne = this;
        gameObject.SetActive(false);
	}

    internal void SetSelection(Creature selection)
    {
        this.selection = selection;
        Debug.Log(selection.brain.ToString());
    }
}
