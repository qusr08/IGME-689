using System.Collections.Generic;
using System.IO;
using UnityEditor.ShaderGraph.Serialization;
using UnityEngine;

public class GenerateStateGeometry : MonoBehaviour
{
	private void Start ( )
	{
		LoadStateBoundaries( );
	}

	private void LoadStateBoundaries ( )
	{
		Debug.Log(Application.persistentDataPath + "/Data/us-state-boundaries.json");
	}
}