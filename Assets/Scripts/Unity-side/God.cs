using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Threading;
using System;

public class God : MonoBehaviour
{
    public static God TheOne;
    Thread simThread;
    public Transform UIPanel;
    public int presentTime { get; private set; }
    float spareTime = 0;
    public float playbackModifier { get; private set; }
    float deltaTime { get { return Simulation.deltaTime / playbackModifier; } }

    bool isActive;

    public void Awake()
    {
        if (TheOne == null) TheOne = this;
    }

    // Use this for initialization
    void Start()
    {
        playbackModifier = 1;
        DisplayManager.TheOne.setBlank();
    }

    public void ToggleSimulation(bool enable)
    {
        if (enable) StartNewSimulation();
        else EndSimulation();
    }

    void StartNewSimulation()
    {
        if (isActive) return;
        simThread = new Thread(() => SafeExecute(() => Simulation.Main(), Handler));
        simThread.Start();
        isActive = true;
        displayMap();
    }

    void EndSimulation()
    {
        Simulation.Aborted = true;
    }

    public void OnDestroy()
    {
        Simulation.Aborted = true;
        simThread.Abort();
        Debug.Log("Secondary thread: " + simThread.ThreadState);
    }



    #region thread Exeption handeling

    static Exception exception = null;

    private static void SafeExecute(Action test, Action<Exception> handler)
    {
        try
        {
            test.Invoke();
        }
        catch (Exception ex)
        {
            Handler(ex);
        }
    }

    private static void Handler(Exception exception)
    {
        God.exception = exception;
    }

    #endregion

    // Update is called once per frame
    void Update()
    {
        if (Simulation.IsActive == false && presentTime == 0) { }   // Before the simulation starts
        else if (spareTime > deltaTime) { spareTime -= Time.deltaTime; }   // We are running to much in front of the simulation, wait a bit.
        else if (Simulation.Data.Count == 0) { }    // No available frames
        else
        {
            while (spareTime < 0 && Simulation.Data.Count > 1)   // We are running behind with our render. Skip frames to catch up.
            {
                Simulation.Data.Dequeue();
                spareTime += deltaTime;
            }
            Simulation.Frame nextFrame = null;

            try
            {
                nextFrame = Simulation.Data.Dequeue();
                presentTime = nextFrame.time;
                int backlog = Simulation.Data.Count;
                DisplayManager.TheOne.setFields(presentTime + backlog, presentTime, backlog, nextFrame.data.Count);
            }
            catch (NullReferenceException e)
            {
                Debug.Log("Failed reading data: " + e.ToString());
            }
            
                int i = 0;
            foreach (Simulation.SimInfo smfo in nextFrame)
            {
                if (i++ > 200) break;
                CreatureAnimator.TheOne.RequestDraw(smfo.ID, new Vector3(smfo.X, smfo.Y, smfo.Th));
            }

            spareTime += deltaTime - Time.deltaTime;    // Time we have left because we worked to fast
        }
        DisplayManager.TheOne.SetSaveButton(Simulation.HasFinished);
        if (exception != null) throw exception;
    }

    public void displayMap()
    {
        GameObject canvas = GameObject.Find("Map");
        GameObject goMap = new GameObject("map");
        goMap.transform.parent = canvas.transform;
        RectTransform mapRT = goMap.AddComponent<RectTransform>();
        mapRT.anchorMin = new Vector2(0, 0);
        mapRT.anchorMax = new Vector2(1, 1);
        mapRT.localScale = new Vector2(1, 1);
        mapRT.localPosition = new Vector3(0, 0);
        mapRT.offsetMin = new Vector2(0, 0);
        mapRT.offsetMax = new Vector2(0, 0);
        //((RectTransform)(goMap.transform)).rect = new Rect(0,0,)
        var camerapos = Camera.main.transform.position;
        var vertExtent = Camera.main.orthographicSize;
        var horzExtent = vertExtent * Screen.width / Screen.height;

        float minX = camerapos.x - horzExtent;
        float maxX = camerapos.x + horzExtent;
        float minY = camerapos.y - vertExtent;
        float maxY = camerapos.y + vertExtent;
        int resolution = (int)(Camera.main.pixelWidth / camerapos.x) / 10;
        var texture = Maps.Light.GetImage(minX, maxX, minY, maxY, resolution);
        goMap.AddComponent<Image>().sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0, 0), 100);

    }

    public void ChangePlayback(float factor)
    {
        playbackModifier *= factor;
        DisplayManager.TheOne.SetPlayback(playbackModifier);
    }

    public void Save(int mode)
    {
        switch (mode)
        {
        case 0:     // Save one creature as xml
            DisplayManager.TheOne.DisplaySaveLocationPrompt(@"Creatures\" + DateTime.Now.ToString(@"MMdd-HHmmss") + @".xml",
                Simulation.SaveBest);
            break;
        case 1:     // Save the 10 best creatures as dna
            DisplayManager.TheOne.DisplaySaveLocationPrompt(@"DNA\" + DateTime.Now.ToString(@"MMdd-HHmmss") + @".dna",
                s => Simulation.SaveDna(10, s));
            break;
        case 2:     // Save all creatures as dna
            DisplayManager.TheOne.DisplaySaveLocationPrompt(@"DNA\" + DateTime.Now.ToString(@"MMdd-HHmmss") + @".dna",
                s => Simulation.SaveDna(0, s));
            break;
        }
    }
}
