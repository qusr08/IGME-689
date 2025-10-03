using UnityEngine;
using UnityEngine.Splines;

public class Car : MonoBehaviour
{
    [SerializeField] private RacingFeatureLayerComponent racingFeatureLayerComponent;

    private void Update()
    {
        // When the race has not started, or the checkpoints have not finished loading in, return
        if (racingFeatureLayerComponent.HasRaceStarted)
        {
            return;
        }

        // Set the position and rotation of the car
        int index1 = racingFeatureLayerComponent.SplineContainer.Splines[0].Count - 2;
        int index2 = racingFeatureLayerComponent.SplineContainer.Splines[0].Count - 3;
        Vector3 position = racingFeatureLayerComponent.GetSplineKnotMidpoint(index1, index2) + new Vector3(0f, 2f, 0f);
        Quaternion rotation = racingFeatureLayerComponent.GetSplineKnotRotation(index1, index2);
        transform.SetPositionAndRotation(position, rotation);
    }
}
