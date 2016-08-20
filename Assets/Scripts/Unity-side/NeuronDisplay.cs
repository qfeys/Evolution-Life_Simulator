using UnityEngine;
using System.Collections.Generic;
using System;

public class NeuronDisplay : MonoBehaviour
{
    public static NeuronDisplay TheOne;

    Creature selection;

    public GameObject NeuronPrefab;

    public void Awake()
    {
        if (TheOne == null) TheOne = this;
    }

    void Start()
    {
        gameObject.SetActive(false);
    }

    internal void SetSelection(Creature selection)
    {
        this.selection = selection;
        Debug.Log(selection.brain.ToString());
        List<string> neurons = new List<string>(selection.brain.ToString().Split('|'));
        neurons.ForEach(n => Debug.Log(n));
        float size = ((RectTransform)transform).rect.width / neurons.Count;
        float radius = ((RectTransform)transform).rect.width / 2 - size * 1.5f;
        float arcDelta = 360 / neurons.Count;
        foreach (string neuron in neurons)
        {
            GameObject go = Instantiate(NeuronPrefab);
            go.transform.SetParent(transform);
            go.transform.localPosition = new Vector3()  // SET THE TRANSFORM ON A CIRCLE & SET SIZE
        }
    }
}
