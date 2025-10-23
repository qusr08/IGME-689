using System.Collections.Generic;
using UnityEngine;

public class PassengerContainer : MonoBehaviour
{
	[SerializeField] private GameObject passengerPrefab;
	[SerializeField] private Material passengerMaterial;
	[SerializeField] private float passengerSize;
	[SerializeField] private float passengerSpacing;
	[SerializeField] private int columns;
	[Space]
	[SerializeField] private Queue<Shape> passengerQueue;

	public int MaxPassengerCount => 2 * columns;

	public bool IsAtCapacity => passengerQueue.Count == MaxPassengerCount;

	private void Awake()
	{
		passengerQueue = new Queue<Shape>();
	}

	private void Start()
	{
		for (int i = 0; i < MaxPassengerCount; i++)
		{
			AddRandomPassenger();
		}
	}

	public void AddRandomPassenger()
	{
		if (IsAtCapacity)
		{
			return;
		}

		Shape passenger = Instantiate(passengerPrefab, transform).GetComponent<Shape>();
		passenger.Material = passengerMaterial;
		passengerQueue.Enqueue(passenger);
		UpdatePassengerPositions();
	}

	public void UpdatePassengerPositions()
	{
		int i = 0;
		foreach (Shape passenger in passengerQueue)
		{
			float rowOffset = i / columns == 0 ? 1 : -1;
			float columnOffset = i % columns;
			float x = 0.5f + ((columnOffset + 1) * passengerSpacing) + ((columnOffset * passengerSize) + (passengerSize / 2f));
			float z = (passengerSize + passengerSpacing) / 2f * rowOffset;

			passenger.transform.localPosition = new Vector3(x, passengerSize, z);
			passenger.transform.localScale = passengerSize * Vector3.one;

			i++;
		}
	}

	public Shape RemoveNextPassenger()
	{
		Shape nextPassenger = passengerQueue.Dequeue();
		UpdatePassengerPositions();

		return nextPassenger;
	}
}
