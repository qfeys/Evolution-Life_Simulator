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
        gameObject.SetActive(true);
        Debug.Log(selection.brain.ToString());
        List<string> neurons = new List<string>(selection.brain.ToString().Split('|'));
        neurons.ForEach(n => Debug.Log(n));
        float size = ((RectTransform)transform).rect.width / neurons.Count;
        float radius = ((RectTransform)transform).rect.width / 2 - size * 1.5f;
        float arcDelta = Mathf.PI * 2 / neurons.Count;
        Vector3 offset = new Vector3(-((RectTransform)transform).rect.width / 2, ((RectTransform)transform).rect.height / 2);
        for (int i = 0; i < neurons.Count; i++)
        {
            string neuron = neurons[i];
            GameObject go = Instantiate(NeuronPrefab);
            go.transform.SetParent(transform);
            go.transform.localPosition = new Vector3(Mathf.Sin(arcDelta * i) * radius, Mathf.Cos(arcDelta * i) * radius) + offset;
            ((RectTransform)(go.transform)).sizeDelta = new Vector2(size, size);
        }
    }
}
