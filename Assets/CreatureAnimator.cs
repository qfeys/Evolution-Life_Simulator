using UnityEngine;
using System.Collections.Generic;
using System;

public class CreatureAnimator : MonoBehaviour
{
    public static CreatureAnimator TheOne;

    Dictionary<int, DrawData> AllCreatures;

    Dictionary<Type, GameObject> Prototypes;
    public GameObject prototypeSpine;
    public GameObject prototypePhSyU;
    public GameObject prototypeGrabber;
    public GameObject prototypeThruster;

    // Use this for initialization
    void Start()
    {
        if (TheOne == null) TheOne = this;
        AllCreatures = new Dictionary<int, DrawData>();
        Prototypes = new Dictionary<Type, GameObject>(4) { { typeof(Node.Spine), prototypeSpine }, { typeof(Node.PhSyU), prototypePhSyU },
            {typeof(Node.Grabber),prototypeGrabber }, {typeof(Node.Thruster),prototypeThruster } };
        InvokeRepeating("CleanUp", 5, 2);
    }

    // Update is called once per frame
    void Update()
    {

    }

    /// <summary>
    /// Requests the drawing of a creature, given his ID, at a certain position.
    /// </summary>
    /// <param name="pos">The position of the creature. The z coord is the rotation of the creature.</param>
    public void RequestDraw(int CreatureID, Vector3 pos)
    {
        if (AllCreatures.ContainsKey(CreatureID) == false)
        {
            Creature c = Simulation.AllCreatures[CreatureID];
            AddCreatureToDict(CreatureID, c);
        }
        GameObject creature = AllCreatures[CreatureID].CreatureGo;
        creature.transform.position = new Vector3(pos.x, pos.y);
        creature.transform.rotation = Quaternion.Euler(0, 0, pos.z*Mathf.Rad2Deg);
    }

    private void AddCreatureToDict(int creatureID, Creature cData)
    {
        GameObject newCreature = new GameObject("Creature " + creatureID);
        AddNode(newCreature, cData.mainNode);
        AllCreatures.Add(creatureID, new DrawData(newCreature));
    }

    /// <summary>
    /// Adds a node gameobject as a child to "oldNode", with as data "node"
    /// </summary>
    private void AddNode(GameObject oldNode, Node node)
    {
        GameObject newNode = Instantiate(Prototypes[node.GetType()]);
        newNode.transform.SetParent(oldNode.transform);
        newNode.transform.localPosition = new Vector3(Mathf.Cos(node.position), Mathf.Sin(node.position), 0) * node.distanceFromParent;
        newNode.transform.localScale = Vector3.one * node.size;
        newNode.transform.localRotation = Quaternion.Euler(0, 0, node.orientation * Mathf.Rad2Deg);
        if (node is Node.Spine && ((Node.Spine)node).HasChilderen())
        {
            foreach (Node child in ((Node.Spine)node).ChildNodes)
            {
                AddNode(newNode, child);
            }
        }
    }

    void CleanUp()
    {
        foreach (var crea in AllCreatures.Values)
        {
            crea.CleanUp(God.TheOne.presentTime);
        }
    }

    class DrawData
    {
        GameObject creatureGo;
        public GameObject CreatureGo { get { LastUsed = God.TheOne.presentTime; return creatureGo; } set { creatureGo = value; } }
        int LastUsed;
        const int MaxIdleTime = 10;

        public DrawData(GameObject newCreature)
        {
            creatureGo = newCreature;
            LastUsed = God.TheOne.presentTime;
        }

        /// <summary>
        /// Returns whether this Object should be removed and cleans up the assosciated GameObjects
        /// </summary>
        /// <param name="Time"></param>
        /// <returns></returns>
        public bool CleanUp(int Time)
        {
            if (Time - LastUsed >= MaxIdleTime)
            {
                Destroy(creatureGo);
                return true;
            }
            return false;
        }
    }
}
