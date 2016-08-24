using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using UnityEngine;

/// <summary>
/// Forms the base structure of the Creature. Just attach more and more Like a tree.
/// </summary>
[DataContract(IsReference = true)]
public abstract class Node
{
    [System.Xml.Serialization.XmlIgnore]
    public Spine Parent { get { return parent; } private set { parent = value; } }

    /// <summary>
    /// This is the size relative to the parent node. For the base node this is absolute.
    /// </summary>
    [DataMember]
    public float size { get; private set; }
    public float realSize { get { return Parent == null ? size : size * Parent.realSize; } }
    public float distanceFromParent { get { return Parent == null ? 0 : (realSize + Parent.realSize) / 2; } }
    /// <summary>
    /// The position in radians relative to the forward (x) of the parent node
    /// </summary>
    [DataMember]
    public float position { get; private set; }
    public float orientation { get { return Parent == null ? 0 : position + Mathf.PI; } }
    public float realOriantation { get { return Parent == null ? 0 : Parent.realOriantation + orientation; } }
    /// <summary>
    ///  Warning, Don't call this to much!
    /// </summary>
    public Vector2 RealPos { get { return Parent == null ? Vector2.zero :
                Parent.RealPos + (realSize + parent.realSize) / 2 * new Vector2(-Mathf.Cos(realOriantation), -Mathf.Sin(realOriantation)); } }
    [DataMember]
    public float toughness { get; private set; }
    public float mass { get { return size * toughness; } }

    [DataMember]
    public List<Sensor> sensor { get; private set; }
    [DataMember]
    Dictionary<float, float> actuatorConnections;
    public abstract Brain.ActuatorResponse.ActuatorType actuatorType { get; } 

    #region Serializable fields
    [DataMember]
    Spine parent;
    // Constructor for deserialisation
    private Node() { }
    #endregion

    public Node(Spine parent, float size, float position, float toughness = 1)
    {
        Parent = parent; this.size = size; this.position = position; this.toughness = toughness;
        sensor = new List<Sensor>();
    }

    internal Dictionary<float, float> GetActuatorConnections()
    {
        return actuatorType==Brain.ActuatorResponse.ActuatorType.none? null :  actuatorConnections;
    }

    public void AddSensor(Sensor.SensorType type) { sensor.Add(new Sensor(this,type)); }

    [DataContract(IsReference = true)]
    public class Spine : Node
    {
        [DataMember]
        List<Node> childNodes;
        public List<Node> ChildNodes { get { return childNodes; } private set { childNodes = value; } }
        /// <summary>
        /// Finds the childs on all depths, including himselve
        /// </summary>
        public List<Node> AllChildNodes
        {
            get
            {
                List<Node> ret = new List<Node>() { this};
                foreach (Node child in childNodes)
                {
                    ret.Add(child);
                    if (child.GetType() == typeof(Spine)) ret.AddRange(((Spine)child).AllChildNodes);
                }
                return ret;
            }
        }
        /// <summary>
        /// Finds the Sensors on all depths
        /// </summary>
        public List<Sensor> AllSensors
        {
            get
            {
                List<Sensor> ret = new List<Sensor>();
                ret.AddRange(sensor);
                foreach (Node child in childNodes)
                {
                    if (child.GetType() == typeof(Spine)) ret.AddRange(((Spine)child).AllSensors);
                    else if(child.sensor != null) ret.AddRange(child.sensor);
                }
                return ret;
            }
        }

        public override Brain.ActuatorResponse.ActuatorType actuatorType { get { return Brain.ActuatorResponse.ActuatorType.internalForce; } }
        [DataMember]
        public float force { get; protected set; }
        [DataMember]
        public float normalPosition;

        public float totalMass { get { return AllChildNodes.Sum(n => n.mass); } }
        public float totalMomentOfInertia { get {return AllChildNodes.Sum(n => n.mass * Mathf.Pow(n.RealPos.magnitude, 2)); } }

        private Spine() { }

        public Spine(Spine parent, float size, float position, float toughness, float force, Dictionary<float, float> actuatorConnections = null) :
            base(parent, size, position, toughness)
        {
            ChildNodes = new List<Node>();
            this.actuatorConnections = actuatorConnections;
            this.force = force;
            normalPosition = position;
        }

        /// <summary>
        /// Adds a new Spinal node to this node and returns it.
        /// </summary>
        public Spine AddNode(float size, float position)
        {
            Spine newNode = new Spine(this, size, position, this.toughness, this.force);
            ChildNodes.Add(newNode);
            return newNode;
        }

        /// <summary>
        /// Adds a new node to this node and returns it. Check the constructor of each node type for the correct arguments.
        /// Thruster: float force, Dictionary<float, float> actuatorConnections
        /// Grabber:  float pierce, Dictionary<float, float> actuatorConnections
        /// </summary>
        public T AddNode<T>(float size, float position, object[] otherArgs = null) where T : Node
        {
            otherArgs = otherArgs ?? new object[0]; // Check if null
            object[] args = new object[] { this, size, position, toughness };
            args = args.Concat(otherArgs).ToArray();
            T newNode = (T)Activator.CreateInstance(typeof(T), args);
            ChildNodes.Add(newNode);
            return newNode;
        }


        public bool HasChilderen() { return ChildNodes.Count != 0; }
    }

    [DataContract(IsReference = true)]
    public class Thruster : Node
    {
        [DataMember]
        public float force { get; protected set; }

        public override Brain.ActuatorResponse.ActuatorType actuatorType { get { return Brain.ActuatorResponse.ActuatorType.force; } }

        private Thruster() { }
        public Thruster(Spine parent, float size, float position, float toughness, float force, Dictionary<float, float> actuatorConnections) :
            base(parent, size, position, toughness)
        {
            this.force = force;
            this.actuatorConnections = actuatorConnections;
        }
    }

    [DataContract(IsReference = true)]
    public class Grabber : Node
    {
        [DataMember]
        public float pierce { get; protected set; }

        public override Brain.ActuatorResponse.ActuatorType actuatorType { get { return Brain.ActuatorResponse.ActuatorType.bite; } }

        private Grabber() { }
        public Grabber(Spine parent, float size, float position, float toughness, float pierce, Dictionary<float, float> actuatorConnections) :
            base(parent, size, position, toughness)
        {
            this.pierce = pierce;
            this.actuatorConnections = actuatorConnections;
        }
    }

    /// <summary>
    /// A photosynthethic unit
    /// </summary>
    [DataContract(IsReference = true)]
    public class PhSyU : Node
    {
        public override Brain.ActuatorResponse.ActuatorType actuatorType { get { return Brain.ActuatorResponse.ActuatorType.none; } }
        private PhSyU() { }

        public PhSyU(Spine parent, float size, float position, float toughness) :
            base(parent, size, position, toughness)
        {
        }
    }


    [DataContract(IsReference = true)]
    public struct Sensor
    {
        public enum SensorType { OtherBody, light, pain }
        [DataMember]
        public SensorType type { get; private set; }
        [DataMember]
        public Node parent { get; private set; }

        public Sensor(Node parent, SensorType type)
        {
            this.parent = parent; this.type = type;
        }
    }
}

