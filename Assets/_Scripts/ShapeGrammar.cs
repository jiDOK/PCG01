using System;
using System.Collections.Generic;
using Random = UnityEngine.Random;
using UnityEngine;

public class ShapeGrammar
{
    Shape axiom;
    Shape sentence;
    int floors;
    bool[] randomize = new bool[3];
    float minXZSize = 1f;
    float maxXZSize = 1f;
    float maxPlanarOffset = 0f;
    float maxAngleOffset = 0f;
    int maxFloors = 1;

    public ShapeGrammar(bool[] randomize, float minXZSize, float maxXZSize, float maxPlanarOffset, float maxAngleOffset, int maxFloors)
    {
        this.randomize = randomize;
        this.minXZSize = minXZSize;
        this.maxXZSize = maxXZSize;
        this.maxPlanarOffset = maxPlanarOffset;
        this.maxAngleOffset = maxAngleOffset;
        this.maxFloors = maxFloors;
    }

    public ShapeGrammar()
    {
    }

    public void Generate(Action<HouseData> buildHouse)
    {
        float width = Random.Range(minXZSize, maxXZSize);
        float length = Random.Range(minXZSize, maxXZSize);
        float height = 1f;
        axiom = new GroundFloor(new ShapeData(width, length, height, Vector2.zero, maxPlanarOffset, maxAngleOffset, 0f, maxFloors, randomize));
        sentence = axiom;
        sentence.Expand();
        HouseData houseData = new HouseData
        {
            Floors = new List<Floor>()
        };
        while (sentence != null)
        {
            switch (sentence.ShapeType)
            {
                case Shape.ST.Default:
                    break;
                case Shape.ST.Base:
                    houseData.BaseFloor = (GroundFloor)sentence;
                    break;
                case Shape.ST.Floor:
                    houseData.Floors.Add((Floor)sentence);
                    break;
                case Shape.ST.Roof:
                    houseData.Roof = (Roof)sentence;
                    break;
                default:
                    break;
            }
            sentence = sentence.NextShape;
        }
        buildHouse(houseData);
    }
}

public class GroundFloor : Shape
{
    ShapeData myData;
    public override ShapeData Data => myData;

    public GroundFloor(ShapeData data) : base(data)
    {
        myData = new ShapeData(data.Width, data.Length, Random.Range(1f, 3f), data.PlanarOffset, data.MaxPlanarOffset, data.MaxAngleOffset, data.AngleOffset, data.MaxFloors, data.Randomize);
        shapeType = ST.Base;
    }

    public GroundFloor() : base()
    {
        shapeType = ST.Base;
    }

    public override void Expand()
    {
        nextShape = new Floor(base.Data, 0);
        nextShape.Expand();
    }
}

public class Floor : Shape
{
    ShapeData myData;
    public override ShapeData Data => myData;
    public int FloorIdx { get; }

    public Floor(ShapeData data, int floorIdx) : base(data)
    {
        Vector2 planarOffset;
        float angleOffset;
        FloorIdx = floorIdx;
        if (data.Randomize[0])
        {
            planarOffset = new Vector2(Random.Range(-data.MaxPlanarOffset, data.MaxPlanarOffset), Random.Range(-data.MaxPlanarOffset, data.MaxPlanarOffset));
            angleOffset = Random.Range(0, data.AngleOffset);
        }
        else
        {
            planarOffset = data.PlanarOffset;
            angleOffset = data.AngleOffset;
        }
        myData = new ShapeData(data.Width, data.Length, data.Height, planarOffset, data.MaxPlanarOffset, data.MaxAngleOffset, angleOffset, data.MaxFloors, data.Randomize);
        shapeType = ST.Floor;
    }

    public Floor() : base()
    {
        shapeType = ST.Floor;
    }

    public override void Expand()
    {
        float decide = Random.value;
        if (decide > 0.1f && FloorIdx < myData.MaxFloors - 1)
        {
            nextShape = new Floor(Data, FloorIdx + 1);
            nextShape.Expand();
        }
        else
        {
            nextShape = new Roof(Data);
        }
    }
}

public class Roof : Shape
{
    ShapeData myData;
    public override ShapeData Data => myData;

    public Roof(ShapeData data) : base(data)
    {
        myData = new ShapeData(data.Width, data.Length, 0.5f, data.PlanarOffset, data.MaxPlanarOffset, data.AngleOffset, data.MaxAngleOffset, data.MaxFloors, data.Randomize);
        shapeType = ST.Roof;
    }

    public Roof()
    {
        shapeType = ST.Roof;
    }

    public override void Expand()
    {
    }
}

public abstract class Shape
{
    public virtual ShapeData Data { get; }

    protected ST shapeType;
    public ST ShapeType { get { return shapeType; } }
    protected Shape nextShape;
    public Shape NextShape { get { return nextShape; } }

    public Shape(ShapeData data)
    {
        Data = data;
    }

    public Shape()
    {
    }

    public abstract void Expand();

    public enum ST
    {
        Default, Base, Floor, Roof,
    }
}

public struct ShapeData
{
    public float Width { get; }
    public float Length { get; }
    public float Height { get; }
    public Vector2 PlanarOffset { get; }
    public float AngleOffset { get; }
    public float MaxPlanarOffset { get; }
    public float MaxAngleOffset { get; }
    public int MaxFloors { get; }
    public bool[] Randomize { get; }

    public ShapeData(float width, float length, float height, Vector2 planarOffset, float maxPlanarOffset, float angleOffset, float maxAngleOffset, int maxFloors, bool[] randomize)
    {
        Width = width;
        Length = length;
        Height = height;
        PlanarOffset = planarOffset;
        MaxPlanarOffset = maxPlanarOffset;
        AngleOffset = angleOffset;
        MaxAngleOffset = maxAngleOffset;
        MaxFloors = maxFloors;
        Randomize = randomize;
    }
}

public class HouseData
{
    public GroundFloor BaseFloor { get; set; }
    public List<Floor> Floors { get; set; }
    public Roof Roof { get; set; }
}
