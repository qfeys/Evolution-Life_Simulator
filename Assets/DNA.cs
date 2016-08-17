using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class DNA {

    bool[] Dna;
    public int Count { get { return Dna.Count(); } }
    static Random rand = new Random();

    public DNA()
    {
        int length = 16 * 16;   // size * number of elements
        Dna = new bool[length];
        for (int i = 0; i < length; i++)
        {
            double r = rand.NextDouble();
            Dna[i] = r > 0.5;
        }

    }

    public DNA(DNA dna, float mutations = 5)
    {
        //UnityEngine.Debug.Log("dna count: " + dna.Count);
        Dna = dna.Dna;
        int c2 = Count;
        if (dna.Count != Count) UnityEngine.Debug.Log("DNA length bad copy.");
        for (int i = 0; i < mutations; i++)
        {
            int c1 = Count;
            int pos = rand.Next(Dna.Count());
            Dna[pos] = !Dna[pos];
            if (c1 != Count) UnityEngine.Debug.Log("DNA length changed.");
        }
        if (c2 != Count) UnityEngine.Debug.Log("DNA length changed a lot.");
    }

    DNA(bool[] Dna)
    {
        this.Dna = Dna;
    }

    //enum Codex { none, node, sensor, grabber, thruster, armor, photoSS, neuron, reproduction  }

    //Dictionary<byte, Codex> codeBook = new Dictionary<byte, Codex> {
    //    {0xf0, Codex.node },
    //    {0xf1, Codex.node },
    //    {0xf2, Codex.node },
    //    {0xf3, Codex.node },
    //    {0xf4, Codex.node },
    //    {0xf5, Codex.node },
    //    {0xf6, Codex.node },
    //    {0xf7, Codex.node },
    //    {0xf8, Codex.sensor },
    //    {0xf9, Codex.grabber },
    //    {0xfa, Codex.thruster },
    //    {0xfb, Codex.armor },
    //    {0xfc, Codex.photoSS },
    //    {0xfd, Codex.neuron },
    //    {0xfe, Codex.reproduction },
    //    {0xff, Codex.reproduction },
    //};



    //public Creature Read()
    //{
    //    Creature org = new Creature();
    //    Node.Spine activeNode = null;

    //    for (int i = 0; i < Count; i++)
    //    {
    //        byte nextSnippet = ToByte(GetRange(i, 8).ToArray());
    //        if (codeBook.ContainsKey(nextSnippet)){
    //            switch(codeBook[nextSnippet])
    //            {
    //            case Codex.node:
    //                Node newNode = new Node.Spine(null,1,0,1,0);
    //                if (activeNode != null) activeNode.AddNode(1,0);
    //                break;
    //            case Codex.sensor:

    //                break;
    //            }
    //        }
    //    }

    //    return org;
    //}

    byte GetByte(int pos)
    {
        return ToByte(Dna.Skip(pos).Take(8).ToArray());
    }

    byte[] GetBytes(int pos, int number)
    {
        byte[] ret = new byte[number];
        for (int i = 0; i < number; i++)
        {
            ret[i] = GetByte(pos + 8 * i);
        }
        return ret;
    }

    byte ToByte(bool[] array)
    {
        if (array.Length != 8) throw new System.ArgumentException("Array not of valid size. Actual size is: " + array.Length);
        byte ret = 0;
        for (int i = 0; i < 8; i++) if (array[i]) ret += (byte)(1 << i);
        return ret;
    }

    byte ToInt(bool[] array)
    {
        if (array.Length != 16) throw new System.ArgumentException("Array not of valid size. Actual size is: " + array.Length);
        byte ret = 0;
        for (int i = 0; i < 16; i++) if (array[i]) ret += (byte)(1 << i);
        return ret;
    }

    /// <summary>
    /// The angle is always coded as a sInt8, and results in a number between -180 and +180.
    /// </summary>
    float readAngle(sbyte code)
    {
        return (float)code / 128 * 180;
    }

    public float[] GetValues()
    {
        float[] ret = new float[16];
        for (int i = 0; i < 16; i++)
        {
            ret[i] = BitConverter.ToUInt16(GetBytes(i * 8, 4), 0) / 4096.0f;
        }
        return ret;
    }

    public override string ToString()
    {
        List<string> ret = new List<string>();
        for (int i = 0; i < Dna.Count()/8; i++)
        {
            ret.Add(GetByte(i * 8).ToString("X2"));
        }
        return string.Join("-", ret.ToArray());
    }

    public static DNA FromString(string hexadecimalCode)
    {
        if (hexadecimalCode.Length % 3 != 2) return null;
        string[] hexadecimalNumbers = hexadecimalCode.Split('-');
        List<bool> ret = new List<bool>();
        for (int i = 0; i < hexadecimalNumbers.Length; i++)
        {
            byte b = Convert.ToByte(hexadecimalNumbers[i],16);
            ret.AddRange(Convert.ToString(b, 2).PadLeft(8,'0').Select(s => s.Equals('1')).ToArray());  // Convert to binary and add.
        }
        return new DNA(ret.ToArray());
    }

    public void Save(string url = "DNA.txt")
    {
        IOHandler.ListSaving("DNA/" + url, new List<object>() { this });
    }
}
