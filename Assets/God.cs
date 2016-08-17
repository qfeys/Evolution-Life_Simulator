using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Threading;
using System;

public class God : MonoBehaviour
{
    public static God TheOne;
    Thread simThread;
    public Transform UIPanel;
    public int presentTime { get; private set; }
    float spareTime = 0;

    bool isActive;

    // Use this for initialization
    void Start()
    {
        if (TheOne == null) TheOne = this;
    }

    public void StartNewSimulation()
    {
        if (isActive) return;
        simThread = new Thread(() => SafeExecute(() => Simulation.Main(), Handler));
        simThread.Start();
        isActive = true;
        displayMap();
    }

    public void OnDestroy()
    {
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
        if (Simulation.IsActive == false)
        {
            UIPanel.Find("Simulation progress").Find("Number").GetComponent<Text>().text = "--";
            UIPanel.Find("Render progress").Find("Number").GetComponent<Text>().text = "--";
            UIPanel.Find("Render backlog").Find("Number").GetComponent<Text>().text = "--";
            UIPanel.Find("Living creatures").Find("Number").GetComponent<Text>().text = "--";
        }
        else if (spareTime > Simulation.deltaTime) { spareTime -= Simulation.deltaTime; }
        else if (Simulation.Data.Count == 0) { }
        else
        {
            Simulation.Frame nextFrame = null;

            try
            {
                nextFrame = Simulation.Data.Dequeue();
                presentTime = nextFrame.time;
                int backlog = Simulation.Data.Count;
                UIPanel.Find("Simulation progress").Find("Number").GetComponent<Text>().text = (presentTime + backlog).ToString();
                UIPanel.Find("Render progress").Find("Number").GetComponent<Text>().text = presentTime.ToString();
                UIPanel.Find("Render backlog").Find("Number").GetComponent<Text>().text = backlog.ToString();
                UIPanel.Find("Living creatures").Find("Number").GetComponent<Text>().text = nextFrame.data.Count.ToString();
            }
            catch (NullReferenceException e)
            {
                Debug.Log("Failed reading data: " + e.ToString());
            }

            if (nextFrame != null)
            {
                int i = 0;
                foreach (Simulation.SimInfo smfo in nextFrame)
                {
                    if (i++ > 500) break;
                    CreatureAnimator.TheOne.RequestDraw(smfo.ID, new Vector3(smfo.X, smfo.Y, smfo.Th));
                }
            }
            else Debug.Log("No frame available. Drawing skipped.");
            spareTime += Simulation.deltaTime - Time.deltaTime;
        }
        //GetComponent<TextMesh>().text = Simulation.Data.Dequeue().time.ToString();
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
}
