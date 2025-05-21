using System;
using System.Collections.Generic;

[Serializable]
public class WaveElement
{
    public Enemy enemy;
    public int count;
    public float timeBetweenSpawn;
}

[Serializable]
public class Wave  
{
    public List<WaveElement> elements = new List<WaveElement>();
}
