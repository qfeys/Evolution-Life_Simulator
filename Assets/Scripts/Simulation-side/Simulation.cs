using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Collections;

static class Simulation
{
    public const int mapsize = 100;
    public const float deltaTime = 1.0f / 30;
    public static Queue<Frame> Data = new Queue<Frame>();
    public static volatile bool IsActive = false;
    public static volatile bool Aborted = false;
    public static volatile bool HasFinished = false;
    static List<SimInfo> Creatures;
    static HashSet<int> MarkedForTermination;
    static Dictionary<Creature, Vector2> NewBorns;
    public static Dictionary<int, Creature> AllCreatures;
    /// <summary>
    /// Saves the new position of the nodes (as in Node.Posiiton) until aplication at the end of the tick.
    /// </summary>
    public static Dictionary<Node, float> NodePositionChanges;

    public static bool ended = false;
    static int Time;
    static int Generation = 1;
    
    public static void Main()
    {
        IsActive = true;
        //while (Generation < 20)
        {
            Initialise();
            Populate(startingPopulation.fresh);
            Run();
            Evaluate();
        }
        IsActive = false;
    }

    static void Initialise()
    {
        Creatures = new List<SimInfo>();
        AllCreatures = new Dictionary<int, Creature>();
        MarkedForTermination = new HashSet<int>();
        NewBorns = new Dictionary<Creature, Vector2>();

        Maps.Light.Initialise();
        Time = 0;
        Debug.Log("Generation " + Generation + " initialised.");
    }

    enum startingPopulation { fresh, load, nextGen}

    static void Populate(startingPopulation start)
    {
        switch (start)
        {
        case startingPopulation.fresh:
            AddCreature(Creature.Generate());
            for (int i = 0; i < 600; i++) AddCreature(Creature.Generate());
            break;
        case startingPopulation.load:
            foreach (string dna in IOHandler.ListLoading("DNA/Gen" + Generation + ".dna"))
            {
                AddCreature(Creature.Generate(DNA.FromString(dna)));
            }
            break;
        case startingPopulation.nextGen:
            foreach (string dna in IOHandler.ListLoading("DNA/Gen"+Generation+".dna"))
            {
                AddCreature(Creature.Generate(DNA.FromString(dna)));
                for (int i = 0; i < 200; i++)
                {
                    DNA fromString = DNA.FromString(dna);
                    AddCreature(Creature.Generate(new DNA(fromString,20)));
                }
            }
            Generation++;
            break;
        }
        Debug.Log("Generation " + Generation + " populated with " + Creatures.Count + " creatures.");
    }

    static void Run()
    {
        while (ended == false && Aborted == false)
        {
            foreach (SimInfo c in Creatures)
            {
                Tick(c);
            }
            foreach (SimInfo c in Creatures)
            {
                FoodTick(c);
            }
            RemoveDeath();
            UpdateMovement();
            AddNew();
            PushData();
            Time++;
            if (Time > 40000) ended = true;
        }
    }

    static void Tick(SimInfo c)
    {
        List<Node.Sensor> sensorRequests = c.creature.SensorRequests();
        float[] sensorAnswer = new float[sensorRequests.Count];
        for (int i = 0; i < sensorRequests.Count; i++)
        {
            Vector2 pos = new Vector2(c.X, c.Y) + (Vector2)(Quaternion.Euler(0, 0, c.Th) * sensorRequests[i].parent.RealPos);
            switch (sensorRequests[i].type)
            {
            case Node.Sensor.SensorType.light:
                sensorAnswer[i] = Maps.Light.GetValue(pos);
                break;
            default:
                throw new NotImplementedException("Sensor type \"" + sensorRequests[i].type + "\" cannot be procesed");
            }
        }
        c.requestedAcceleration = c.creature.actuatorRequest(sensorAnswer);
    }

    static void FoodTick(SimInfo c)
    {
        float energyGain = 0;
        foreach (Creature.FoodEvent fe in c.creature.foodRequests)
        {
            switch (fe.source)
            {
            case Creature.FoodEvent.Source.light:
                energyGain += fe.strength * Maps.Light.GetValue(fe.pos + c.pos) * deltaTime;
                break;
            case Creature.FoodEvent.Source.meat:
                throw new NotImplementedException("meat food source not implemented");
            }
        }
        if (c.creature.GiveFood(energyGain) == false) MarkedForTermination.Add(c.ID);
        else
        {
            Creature newcreature = c.creature.Reproduce();
            if (newcreature != null) NewBorns.Add(newcreature, c.pos);
        }
    }

    static void RemoveDeath()
    {
        Creatures.RemoveAll(simInf => MarkedForTermination.Contains(simInf.ID));
        MarkedForTermination = new HashSet<int>();
    }

    static void UpdateMovement()
    {
        foreach (SimInfo c in Creatures)
        {
            c.UpdateVelocity();
        }
    }

    static void AddNew()
    {
        foreach(KeyValuePair<Creature,Vector2> nb in NewBorns)
        {
            Creatures.Add(new SimInfo(nb.Key, nb.Value.x, nb.Value.y));
        }
        NewBorns = new Dictionary<Creature, Vector2>();
    }

    static void PushData()
    {
        Frame nFrame = new Frame(Time, Creatures);
        Data.Enqueue(nFrame);
    }
    
    static Creature AddCreature(Creature creature)
    {
        SimInfo si = new SimInfo(creature,mapsize/3,mapsize/3);
        Creatures.Add(si);
        AllCreatures.Add(si.ID, si.creature);
        return creature;
    }

    static void Evaluate()
    {
        Creatures = Creatures.OrderBy(c => -c.creature.energy).ToList();
        HasFinished = true;
    }

    static public void Save(int number, string path)
    {
        if (HasFinished == false)
        {
            Debug.LogError("Cannot save. HAs not finished.");
            return;
        }
        path = System.IO.Path.ChangeExtension(path, "dna");
        number = number == 0 ? Creatures.Count : number;
        for (int i = 0; i < 10; i++)
        {
            Creatures[i].creature.SaveDna(path);
        }
    }

    static float Random(int seed,int n, float max = 1, float min = 0)
    {
        int prime1 = 100511;
        int prime2 = 99577;
        long prime3 = 2147483629;
        return (float)(((n * prime3 + seed) % prime1) * n % prime2) / prime2;
    }

    /// <summary>
    /// A container to keep track of the creature during the simulation. This way the creature does not need to keep track
    /// of its position and other simulation related variables.
    /// </summary>
    internal class SimInfo
    {
        public Creature creature { get; private set; }
        public float X;
        public float Y;
        public float Th;    // Thèta, the orientation of the creature in radians, 0 is along the x-axis, climbing counterclockwise.
        public int ID { get; private set; }
        public Vector2 pos { get { return new Vector2(X, Y); } }

        public Vector3 velocity;    // velocity and acceleration are rotated relative to the creature
        public Vector3 requestedAcceleration;

        static int nextID = 0;


        public SimInfo(Creature creature, float X = 0, float Y = 0, float Th = 0)
        {
            this.creature = creature;
            this.X = X; this.Y = Y;
            this.Th = Th % 2 * Mathf.PI;
            ID = nextID;
            nextID++;
            velocity = Vector3.zero;
            requestedAcceleration = Vector3.zero;
        }

        public SimInfo(Creature creature, float X, float Y, float Th, int ID) : this(creature, X, Y, Th)
        {
            this.ID = ID;
        }

        public void UpdateVelocity()
        {
            velocity += requestedAcceleration * deltaTime;
            Vector3 absVel = new Vector3(Mathf.Abs(velocity.x), Mathf.Abs(velocity.y), Mathf.Abs(velocity.z));
            velocity -= 0.1f * Vector3.Scale( Vector3.Scale(velocity, absVel),new Vector3(1,1,10));
            X += velocity.x * Mathf.Cos(Th) + velocity.y * Mathf.Sin(Th);
            Y += velocity.x * Mathf.Sin(Th) + velocity.y * Mathf.Cos(Th);
            Th += velocity.z;
        }

        /// <summary>
        /// The copy's x, y, th and id will not be changing
        /// </summary>
        /// <returns></returns>
        internal SimInfo Copy()
        {
            return new SimInfo(creature, X, Y, Th, ID);
        }
    }

    /// <summary>
    /// A container used to store the state of the simulation. Normally used at each timeframe.
    /// </summary>
    internal class Frame : IEnumerable<SimInfo>
    {
        public int time { get; private set; }
        public List<SimInfo> data { get; private set; }
        public Frame(int time, List<SimInfo> data)
        {
            this.time = time;
            this.data = new List<SimInfo>(data.Count);
            for (int i = 0; i < data.Count; i++)
            {
                this.data.Add(data[i].Copy());
            }
        }

        public Frame()
        {
            time = -1; data = new List<SimInfo>(0);
        }

        public IEnumerator<SimInfo> GetEnumerator()
        {
            return data.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable<SimInfo>)data).GetEnumerator();
        }
    }
}

