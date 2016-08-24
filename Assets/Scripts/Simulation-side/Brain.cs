using System;
using System.Linq;
using System.Collections.Generic;
using System.Runtime.Serialization;

[DataContract]
public class Brain
{
    [DataMember]
    List<Node.Sensor> sensors;
    [DataMember]
    List<Node> actuators;
    [DataMember]
    List<Neuron> neurons;
    public int size { get { return neurons.Count; } }

    public Brain(Node.Spine mainNode)
    {
        sensors = mainNode.AllSensors;
        actuators = mainNode.AllChildNodes.FindAll(n => n.GetActuatorConnections() != null);
        neurons = new List<Neuron>();
        sensors.ForEach(s => neurons.Add(new Neuron.Sensor(s)));
        actuators.ForEach(a => neurons.Add(new Neuron.Actuator(a)));
    }

    /// <summary>
    /// Adds A neuron to the brain
    /// </summary>
    /// <param name="connections">The key is the location of the connection, the value the strength</param>
    public void AddNeuron(Dictionary<float,float> connections, int cluster)
    {
        neurons.Add(new Neuron(connections, cluster));
    }

    public List<Node.Sensor> GetSensorList() { return sensors; }

    internal void Process(float[] sensorAnswer)
    {
        // Set sensor state
        if (sensors.Count != sensorAnswer.Length) throw new Exception("Invalid sensor answer");
        List<Neuron.Sensor> nSensors = neurons.FindAll(n => n.GetType() == typeof(Neuron.Sensor)).ConvertAll(n=>(Neuron.Sensor)n);
        for (int i = 0; i < sensors.Count; i++)
        {
            if (nSensors[i].IsSensor(sensors[i]) == false) throw new Exception("Sensors not alligned");
            nSensors[i].setState(sensorAnswer[i]);
        }
        // process all neurons
        neurons.ForEach(n => n.ComputeState());
        neurons.ForEach(n => n.ConfirmState());

    }

    internal List<ActuatorResponse> GetActuatorResponse()
    {
        List<ActuatorResponse> ret = new List<ActuatorResponse>();
        foreach (Neuron.Actuator act in neurons.FindAll(n=>n is Neuron.Actuator).ConvertAll(n => (Neuron.Actuator)n))
        {
            ret.Add(act.GenerateResponse());
        }
        return ret;
    }

    public void Finalise()
    {
        neurons.ForEach(n =>n.Finalise(neurons));
    }

    static List<Neuron> GetCluster(List<Neuron> neurons, int cluster)
    {
        return neurons.FindAll(n => n.cluster == cluster);
    }

    public override string ToString()
    {
        return string.Join("|", neurons.ConvertAll(n => n.ToString()).OrderBy(s => s).ToArray());
    }

    [DataContract(IsReference = true)]
    class Neuron
    {
        [DataMember]
        private float state;
        float nextState;
        [DataMember]
        public readonly int cluster;
        /// <summary>
        /// incoming connections
        /// </summary>
        [DataMember]
        Dictionary<Neuron, float> conections;
        Dictionary<float, float> templateConnections;
        string ID;

        public Neuron(Dictionary<float, float> conections, int cluster)
        {
            templateConnections = conections;
            this.cluster = cluster;
        }

        internal void Finalise(List<Neuron> neurons)
        {
            ID = "" + this.cluster + "," + GetCluster(neurons, this.cluster).IndexOf(this);
            if (templateConnections == null) return;
            conections = new Dictionary<Neuron, float>();
            foreach (KeyValuePair<float, float> cnct in templateConnections)
            {
                int cluster = (int)cnct.Key;
                List<Neuron> neurClust = GetCluster(neurons, cluster);
                if (neurClust.Count == 0) continue;
                Neuron source = neurClust[(int)(Math.Abs(cnct.Key - cluster) * neurClust.Count)];
                if (conections.ContainsKey(source)) { conections[source] += cnct.Value; }
                else { conections.Add(source, cnct.Value); }
            }
        }

        public void ComputeState()
        {
            nextState = 0;
            if (conections == null) return;
            foreach (KeyValuePair<Neuron,float> cnct in conections)
            {
                nextState += cnct.Key.state * cnct.Value;
            }
        }

        /// <summary>
        /// Loads state into newstate and multiplys by the sigmund function
        /// </summary>
        public  void ConfirmState() { state = (float)(1/(1+ Math.Exp( nextState))); }

        public override string ToString()
        {
            if (conections == null || conections.Count == 0) return ID;
            return ID + ";" + string.Join(";", conections.ToList().ConvertAll(c => "" + c.Key.ID + "," + c.Value.ToString("n3")).OrderBy(s => s).ToArray());
        }

        /// <summary>
        /// A neuron connected to a sensor
        /// </summary>
        [DataContract(IsReference = true, Name = "SensorNeuron")]
        public class Sensor : Neuron
        {
            [DataMember]
            Node.Sensor sensor;

            public Sensor(Node.Sensor sensor) : base(null, 0)
            {
                this.sensor = sensor;
            }

            public new void ConfirmState() { return; }

            public void setState(float newState) { state = newState; }
            public bool IsSensor(Node.Sensor test) { return sensor.Equals(test); }
        }

        /// <summary>
        /// A neuron connected to an actuator
        /// </summary>
        [DataContract(IsReference = true)]
        public class Actuator : Neuron
        {
            [DataMember]
            Node actuator;
            public Actuator(Node actuator) : base(actuator.GetActuatorConnections(), 1)
            {
                this.actuator = actuator;
            }

            internal ActuatorResponse GenerateResponse()
            {
                var pos = actuator.RealPos;
                return new ActuatorResponse(actuator.actuatorType, pos.x, pos.y, actuator.realOriantation, state, actuator);
            }
        }
    }

    public struct ActuatorResponse
    {
        public enum ActuatorType {none, force, bite, internalForce }
        public readonly ActuatorType type;
        // The position and orientation relative to the creature
        public readonly float x; public readonly float y; public readonly float theta;
        public readonly float value;
        public readonly Node node;


        public ActuatorResponse(ActuatorType type, float x, float y, float theta, float value, Node node)
        {
            this.type = type; this.x = x; this.y = y; this.theta = theta; this.value = value; this.node = node;
        }
    }
}
