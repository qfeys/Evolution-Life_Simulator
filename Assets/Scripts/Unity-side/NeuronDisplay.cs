using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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
        if (this.selection == selection) return;
        if (this.selection != null) CleanUp();
        this.selection = selection;
        gameObject.SetActive(true);
        List<string> neurons = new List<string>(selection.brain.ToString().Split('|'));
        float size = ((RectTransform)transform).rect.width / neurons.Count;
        float radius = ((RectTransform)transform).rect.width / 2 - size * 1.5f;
        float arcDelta = Mathf.PI * 2 / neurons.Count;
        Vector3 offset = new Vector3(-((RectTransform)transform).rect.width / 2, ((RectTransform)transform).rect.height / 2);
        for (int i = 0; i < neurons.Count; i++)
        {
            string neuron = neurons[i];
            GameObject neu = Instantiate(NeuronPrefab);
            neu.transform.SetParent(transform);
            neu.transform.localPosition = new Vector3(Mathf.Sin(arcDelta * i) * radius, Mathf.Cos(arcDelta * i) * radius) + offset;
            ((RectTransform)(neu.transform)).sizeDelta = new Vector2(size, size);
            int cluster = int.Parse(neuron.Split(',')[0]);
            neu.GetComponent<Image>().color = FindColor(cluster);
            var connections = neuron.Split(';');
            for (int j = 1; j < connections.Length; j++)
            {
                var values = connections[j].Split(',');
                int endpos = neurons.FindIndex(s =>
                {
                    var val = s.Split(';')[0].Split(',');
                    return val[0].Equals(values[0]) && val[1].Equals(values[1]);
                });
                DrawBezier(new Vector3(Mathf.Sin(arcDelta * i) * radius, Mathf.Cos(arcDelta * i) * radius),
                    new Vector3(Mathf.Sin(arcDelta * endpos) * radius / 2, Mathf.Cos(arcDelta * endpos) * radius / 2),
                    i != endpos ? new Vector3(Mathf.Sin(arcDelta * endpos) * radius, Mathf.Cos(arcDelta * endpos) * radius) :
                        new Vector3(Mathf.Sin(arcDelta * (endpos + 0.3f)) * radius, Mathf.Cos(arcDelta * (endpos + 0.3f)) * radius),
                    float.Parse(values[2]));
            }
        }
    }

    private Color FindColor(int cluster)
    {
        if (cluster == 0) return Color.green;
        if (cluster == 1) return Color.red;
        return Color.white;
    }

    private void CleanUp()
    {
        var children = new List<GameObject>();
        foreach (Transform child in transform) children.Add(child.gameObject);
        children.ForEach(child => Destroy(child));
    }

    private void DrawBezier(Vector2 p0, Vector2 p1, Vector2 p2, float value)
    {
        GameObject bezier = new GameObject();
        bezier.transform.SetParent(transform);
        bezier.transform.localPosition = Vector3.zero;
        bezier.AddComponent<RectTransform>().sizeDelta = Vector2.zero;
        var renderer = bezier.AddComponent<UILineRenderer>();
        renderer.raycastTarget = false;
        renderer.color = value < 0 ? Color.red : Color.green;
        renderer.LineThickness = Mathf.Clamp(Mathf.Abs(value), 0, 6);
        int size = 9;
        renderer.Points = new Vector2[size];
        for (int i = 0; i < size; i++)
        {
            renderer.Points[i] = Bezier.GetPoint(p0, p1, p2, 1f / (size-1) * i);
        }
    }

    public static class Bezier
    {
        public static Vector3 GetPoint(Vector3 p0, Vector3 p1, Vector3 p2, float t)
        {
            t = Mathf.Clamp01(t);
            float oneMinusT = 1f - t;
            return
                oneMinusT * oneMinusT * p0 +
                2f * oneMinusT * t * p1 +
                t * t * p2;
        }

        public static Vector3 GetFirstDerivative(Vector3 p0, Vector3 p1, Vector3 p2, float t)
        {
            return
                2f * (1f - t) * (p1 - p0) +
                2f * t * (p2 - p1);
        }
    }
    
}
