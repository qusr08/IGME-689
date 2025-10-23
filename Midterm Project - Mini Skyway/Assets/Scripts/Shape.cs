using System;
using UnityEngine;
using Random = UnityEngine.Random;

public enum ShapeType
{
    CIRCLE, TRIANGLE, SQUARE
}

public class Shape : MonoBehaviour
{
    [SerializeField] private ShapeMeshDictionary shapeMeshDictionary;
    [SerializeField] private Material shapeMaterial;
    [SerializeField] private ShapeType _type;

    public ShapeType Type
    {
        get => _type;
        set
        {
            _type = value;

            // Update active airport mesh
            int airportTypeCount = Enum.GetValues(typeof(ShapeType)).Length;
            for (int i = 0; i < airportTypeCount; i++)
            {
                shapeMeshDictionary[(ShapeType)i].enabled = (_type == (ShapeType)i);
            }
        }
    }

    public bool IsRendererVisible => shapeMeshDictionary[Type].isVisible;

    private void OnValidate()
    {
        int airportTypeCount = Enum.GetValues(typeof(ShapeType)).Length;
        for (int i = 0; i < airportTypeCount; i++)
        {
            shapeMeshDictionary[(ShapeType)i].material = shapeMaterial;
        }
    }

    private void Awake()
    {
        OnValidate();
    }

    private void Start()
    {
        Type = (ShapeType)Random.Range(0, 3);
    }

    private void OnMouseDown()
    {
        Debug.Log("CLICK");
    }
}
