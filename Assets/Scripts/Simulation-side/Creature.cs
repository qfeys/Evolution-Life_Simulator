using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

[System.Runtime.Serialization.DataContract]
public class Creature
{
    [System.Runtime.Serialization.DataMember(Order = 1,IsRequired = true)]
    public Brain brain;
    [System.Runtime.Serialization.DataMember(IsRequired = true,Order = 0)]
    public Node.Spine mainNode { get; private set; }
    [System.Runtime.Serialization.DataMember(IsRequired = true, Order = 2)]
    public DNA Dna { get; private set; }
    public List<FoodEvent> foodRequests;
    public float energy { get; private set; }
    List<Node.PhSyU> phSyUs;

    public Creature()
    {
        foodRequests = new List<FoodEvent>();
        energy = 10;
    }

    public void SetBrain(Brain brain)
    {
        if (brain != null) throw new Exception("Brain was already set!");
        this.brain = brain; 
    }

    internal List<Node.Sensor> SensorRequests()
    {
        return brain.GetSensorList();
    }

    /// <summary>
    /// Preforms all the actions the creature wants to preform, including moving, eating and sunbathing
    /// Returns the X-acceleration, the Y-acceleration and the angular acceleration
    /// </summary>
    internal Vector3 actuatorRequest(float[] sensorAnswer)
    {
        brain.Process(sensorAnswer);
        List<Brain.ActuatorResponse> aResps = brain.GetActuatorResponse();
        Vector3 forces = new Vector3();
        float mass = mainNode.totalMass;
        foreach(Brain.ActuatorResponse aResp in aResps)
        {
            if (aResp.value > 1) Debug.Log("Actuator Response > 1");
            if (aResp.value < 0) Debug.Log("Actuator Response < 0");
            switch (aResp.type)
            {
            case Brain.ActuatorResponse.ActuatorType.force:
                float magnetude = ((Node.Thruster)(aResp.node)).force * aResp.value;
                float fx = magnetude * Mathf.Cos(aResp.theta);
                float fy = magnetude * Mathf.Sin(aResp.theta);
                float tau = fx * aResp.y + fy * aResp.x;
                forces += new Vector3(fx, fy, tau);
                energy -= aResp.value * Simulation.deltaTime;
                break;
            case Brain.ActuatorResponse.ActuatorType.internalForce:
                float naturalForce = ((Node.Spine)(aResp.node)).normalPosition - aResp.node.position;
                float activeForce = ((Node.Spine)(aResp.node)).force * aResp.value;
                float netForce = naturalForce + activeForce;
                // UPGRADE: Appely proper torque on the total body.
                float newPos = aResp.node.position + ((Node.Spine)(aResp.node)).totalMass * netForce;
                Simulation.NodePositionChanges.Add(aResp.node, newPos);
                energy -= aResp.value * Simulation.deltaTime*10;
                break;
            case Brain.ActuatorResponse.ActuatorType.bite:
                if (aResp.value > 0.9)
                {
                    foodRequests.Add(new FoodEvent(FoodEvent.Source.meat, new Vector2(aResp.x, aResp.y),((Node.Grabber)aResp.node).pierce));
                    energy -= aResp.value * Simulation.deltaTime;
                }
                break;
            }
        }
        // Brain energy consumption
        energy -= brain.size * 0.2f *Simulation.deltaTime;
        foreach (Node.PhSyU ph in phSyUs)
        {
            foodRequests.Add(new FoodEvent(FoodEvent.Source.light, ph.RealPos, ph.size * 0.02f));
        }
        return new Vector3(forces.x / mass, forces.y / mass, forces.z / mainNode.totalMomentOfInertia);
    }
    
    /// <summary>
    /// Adds the recieved food for this creature. Returns wheter it'll survive
    /// </summary>
    internal bool GiveFood(float newEnergy)
    {
        energy += newEnergy;
        foodRequests = new List<FoodEvent>();
        return energy > 0;
    }

    public Creature Reproduce()
    {
        if (energy > 80)
        {
            energy -= 40;
            return Generate(new DNA(Dna, 5));
        }
        return null;
    }

    public static Creature Generate() { return Generate(new DNA()); }

    public static Creature Generate(DNA dna)
    {
        float[] values = dna.GetValues();
        Creature c = new Creature();
        c.Dna = dna;
        c.mainNode = new Node.Spine(null, 1, 0, 1, 0);
        if (values[0] != values[2])
            c.mainNode.AddNode(1, 2 * Mathf.PI / 3).AddNode<Node.Thruster>(1, Mathf.PI, new object[2] { 1, new Dictionary<float, float>() { { values[0], values[1] }, { values[2], values[3] } } });
        else
            c.mainNode.AddNode(1, 2 * Mathf.PI / 3).AddNode<Node.Thruster>(1, Mathf.PI, new object[2] { 1, new Dictionary<float, float>() { { values[0], values[1] + values[3] } } });
        if (values[4] != values[6])
            c.mainNode.AddNode(1, 4 * Mathf.PI / 3).AddNode<Node.Thruster>(1, Mathf.PI, new object[2] { 1, new Dictionary<float, float>() { { values[4], values[5] }, { values[6], values[7] } } });
        else
            c.mainNode.AddNode(1, 2 * Mathf.PI / 3).AddNode<Node.Thruster>(1, Mathf.PI, new object[2] { 1, new Dictionary<float, float>() { { values[4], values[5] + values[7] } } });
        c.mainNode.AddNode<Node.PhSyU>(1, Mathf.PI / 2).AddSensor(Node.Sensor.SensorType.light);
        c.mainNode.AddNode<Node.PhSyU>(1, Mathf.PI * 3 / 2).AddSensor(Node.Sensor.SensorType.light);
        c.brain = new Brain(c.mainNode);
        if (values[8] != values[10])
            c.brain.AddNeuron(new Dictionary<float, float>() { { values[8], values[9] }, { values[10], values[11] } }, 2);
        else
            c.brain.AddNeuron(new Dictionary<float, float>() { { values[8], values[9] + values[11] } }, 2);
        if (values[12] != values[14])
            c.brain.AddNeuron(new Dictionary<float, float>() { { values[12], values[13] }, { values[14], values[15] } }, 2);
        else
            c.brain.AddNeuron(new Dictionary<float, float>() { { values[12], values[13] + values[15] } }, 2);
        c.brain.Finalise();
        c.phSyUs = c.mainNode.AllChildNodes.FindAll(cn => cn is Node.PhSyU).Cast<Node.PhSyU>().ToList();

        return c;
    }

    public struct FoodEvent
    {
        public enum Source { light, meat}
        public readonly Source source;
        public readonly Vector2 pos;
        public readonly float strength;
        public FoodEvent(Source s, Vector2 p, float str)
        {
            source = s; pos = p; strength = str;
        }
    }

    public void SaveState() { IOHandler.SerializeCreature(this); }
    public void SaveDna(string url = "DNA.txt") { Dna.Save(url); }
}

