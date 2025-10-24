using System;
using UnityEngine;

public enum ShapeType
{
    NONE, CIRCLE, TRIANGLE, SQUARE
}

public class Shape : MonoBehaviour
{
    [SerializeField] private ShapeMeshDictionary shapeMeshDictionary;
    [SerializeField] private Material _material;
    [SerializeField] private ShapeType _type;

    public ShapeType Type
    {
        get => _type;
        set
        {
            _type = value;

            // Update active airport mesh
            int airportTypeCount = Enum.GetValues(typeof(ShapeType)).Length;
            for (int i = 1; i < airportTypeCount; i++)
            {
                shapeMeshDictionary[(ShapeType)i].enabled = (_type == (ShapeType)i);
            }
        }
    }

    public bool IsRendererVisible => shapeMeshDictionary[Type].isVisible;

    public Material Material
    {
        get => _material;
        set
        {
            _material = value;

			int airportTypeCount = Enum.GetValues(typeof(ShapeType)).Length;
			for (int i = 1; i < airportTypeCount; i++)
			{
				shapeMeshDictionary[(ShapeType)i].material = _material;
			}
		}
    }

    private void Start()
    {
        Material = _material;
    }

    private void OnMouseDown()
    {
        Debug.Log("CLICK");
    }
}
