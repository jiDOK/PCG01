using System;
using System.Collections.Generic;
using Random = UnityEngine.Random;

public class ShapeGrammar
{
    Shape axiom;
    Shape sentence;
    int floors;

    // Generate soll eine callback-Action auslösen, die übergeben wird
    public void Generate(Action<HouseData> buildHouse)
    {
        // fürs erste die Floor-Daten
        float width = Random.Range(3f, 20f);
        float length = Random.Range(3f, 20f);
        float height = 1f;
        // Wir beginnen mit einem Basefloor als Start
        axiom = new BaseFloor(width, length, height);
        sentence = axiom;
        // und bringen die Sache ins Rollen
        sentence.Expand();
        // wir erstellen ein HouseData-Objekt
        HouseData houseData = new HouseData
        {
            Floors = new List<Floor>()
        };
        // am Ende durch die verlinkten Shapes durchsteppen und houseData entsprechend füllen
        while (sentence != null)
        {
            switch (sentence.ShapeType)
            {
                case Shape.ST.Default:
                    break;
                case Shape.ST.Base:
                    houseData.BaseFloor = (BaseFloor)sentence;
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
        // und das Haus bauen!
        buildHouse(houseData);
    }
}

public class BaseFloor : Shape
{
    float mySpecialHeight;

    public override float Height { get { return mySpecialHeight; } }

    // der normale Constructor
    public BaseFloor(float width, float length, float height) : base(width, length, height)
    {
        shapeType = ST.Base;
        mySpecialHeight = Random.Range(1f, 3f);
    }

    // parameterloser Constructor
    public BaseFloor() : base()
    {
        shapeType = ST.Base;
    }

    public override void Expand()
    {
        nextShape = new Floor(width, length, height);
        nextShape.Expand();
    }
}


public class Floor : Shape
{
    public Floor(float width, float length, float height) : base(width, length, height)
    {
        shapeType = ST.Floor;
    }

    public Floor() : base()
    {
        shapeType = ST.Floor;
    }

    // die Floor Shapes entscheiden von Fall zu Fall, ob es weitergeht, oder das Dach folgt
    public override void Expand()
    {
        float decide = Random.value;
        if (decide > 0.1f)
        {
            nextShape = new Floor(width, length, height);
            nextShape.Expand();
        }
        else
        {
            nextShape = new Roof(width, length, height);
        }
    }

}

public class Roof : Shape
{
    public override float Height { get { return 0.5f; } }

    public Roof(float width, float height, float length) : base(width, height, length)
    {
        shapeType = ST.Roof;
    }

    public Roof() : base()
    {
        shapeType = ST.Roof;
    }

    // nach dem Dach passiert momentan nichts mehr, nextShape bleibt null.
    public override void Expand()
    {
    }
}

// abstrakte Klasse als Vorbild
public abstract class Shape
{
    protected float width = 1f;
    public float Width { get { return width; } }
    protected float length = 1f;
    public float Length { get { return length; } }
    protected float height = 1f;
    public virtual float Height { get { return height; } }

    protected ST shapeType;
    public ST ShapeType { get { return shapeType; } }
    protected Shape nextShape;
    public Shape NextShape { get { return nextShape; } }

    public Shape(float width, float length, float height)
    {
        this.width = width;
        this.length = length;
        this.height = height;
    }

    public Shape()
    {
    }

    public abstract void Expand();

    // die momentanen Arten von Shape. Default ist nicht umgesetzt
    public enum ST
    {
        Default, Base, Floor, Roof,
    }
}

// ein Erdgeschoß, ggf. mehrere Stockwerke, ein Dach
public class HouseData
{
    public BaseFloor BaseFloor { get; set; }
    public List<Floor> Floors { get; set; }
    public Roof Roof { get; set; }
}
