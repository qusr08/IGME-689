using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class PassengerContainer : MonoBehaviour
{
	[SerializeField] private GameObject passengerPrefab;
	[SerializeField] private Material passengerMaterial;
	[SerializeField] private float passengerSize;
	[SerializeField] private float passengerSpacing;
	[SerializeField] private int columns;
	[Space]
	[SerializeField] private List<Shape> passengerList;

	public List<ShapeType> PassengerTypeList { get; private set; }
	public int MaxPassengerCount => 2 * columns;
	public int CurrentPassengers => passengerList.Count;
	public bool IsAtCapacity => CurrentPassengers == MaxPassengerCount;
	public Material LocalMaterial { get; private set; }

	private void Awake()
	{
		passengerList = new List<Shape>();
		PassengerTypeList = new List<ShapeType>();
		LocalMaterial = new Material(passengerMaterial);
	}

	public void AddPassenger(ShapeType shapeType)
	{
		if (IsAtCapacity)
		{
			return;
		}

		Shape passenger = Instantiate(passengerPrefab, transform).GetComponent<Shape>();
		passenger.Material = LocalMaterial;
		passenger.Type = shapeType;
		passengerList.Add(passenger);
		PassengerTypeList.Add(shapeType);

		UpdatePassengerPositions();
	}

	public void AddRandomPassenger(ShapeType excludedType = ShapeType.NONE)
	{
		List<ShapeType> availableTypes = new List<ShapeType>() {
			ShapeType.CIRCLE, ShapeType.TRIANGLE, ShapeType.SQUARE
		}.Where(x => x != excludedType).ToList();
		AddPassenger(availableTypes[Random.Range(0, availableTypes.Count)]);
	}

	public void UpdatePassengerPositions()
	{
		for (int i = 0; i < passengerList.Count; i++)
		{
			float rowOffset = i / columns == 0 ? 1 : -1;
			float columnOffset = i % columns;
			float x = 0.5f + ((columnOffset + 1) * passengerSpacing) + ((columnOffset * passengerSize) + (passengerSize / 2f));
			float z = (passengerSize + passengerSpacing) / 2f * rowOffset;

			passengerList[i].transform.localPosition = new Vector3(x, passengerSize, z);
			passengerList[i].transform.localScale = passengerSize * Vector3.one;
		}
	}

	public ShapeType RemoveNextPassenger()
	{
		if (passengerList.Count == 0)
		{
			return ShapeType.NONE;
		}

		return RemovePassengerAtIndex(0);
	}

	public ShapeType GetNextPassengerType()
	{
		if (passengerList.Count == 0)
		{
			return ShapeType.NONE;
		}

		return passengerList[0].Type;
	}

	public ShapeType RemovePassengerAtIndex(int index)
	{
		ShapeType nextPassengerType = passengerList[index].Type;
		Destroy(passengerList[index].gameObject);
		passengerList.RemoveAt(index);
		PassengerTypeList.RemoveAt(index);

		UpdatePassengerPositions();

		return nextPassengerType;
	}

	public void RemoveAllPassengers()
	{
		for (int i = CurrentPassengers - 1; i >= 0; i--)
		{
			RemovePassengerAtIndex(i);
		}
	}

	public int RemovePassengersOfType(ShapeType type)
	{
		if (passengerList.Count == 0)
		{
			return 0;
		}

		int count = 0;

		for (int i = CurrentPassengers - 1; i >= 0; i--)
		{
			if (passengerList[i].Type == type)
			{
				count++;
				RemovePassengerAtIndex(i);
			}
		}

		return count;
	}
}
