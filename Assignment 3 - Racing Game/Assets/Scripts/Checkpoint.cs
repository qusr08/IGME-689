using Esri.ArcGISMapsSDK.Components;
using Esri.GameEngine.Elevation;
using Esri.GameEngine.Geometry;
using Esri.HPFramework;
using UnityEngine;
using UnityEngine.Splines;

public class Checkpoint : MonoBehaviour
{
    public int SplineIndex { get; private set; }

    private RacingFeatureLayerComponent racingFeatureLayerComponent;
    private SplineContainer splineContainer;
    private ArcGISMapComponent mapComponent;

    public void Initialize(RacingFeatureLayerComponent racingFeatureLayerComponent, SplineContainer splineContainer, ArcGISMapComponent mapComponent, ArcGISPoint point)
    {
        this.racingFeatureLayerComponent = racingFeatureLayerComponent;
        this.splineContainer = splineContainer;
        this.mapComponent = mapComponent;

        // Add the knot to the spline
        SplineIndex = splineContainer.Splines[0].Count;
        BezierKnot bezierKnot = new BezierKnot();
        bezierKnot.Position = mapComponent.GeographicToEngine(point);
        transform.SetPositionAndRotation(bezierKnot.Position, Quaternion.identity);
        splineContainer.Splines[0].Add(bezierKnot);

        // Set up an elevation monitor to make sure the spline knot sits on the ground
        ArcGISElevationMonitor elevationMonitor = new ArcGISElevationMonitor(point, ArcGISElevationMode.RelativeToGround, 0);
        elevationMonitor.PositionChanged += UpdateSplinePosition;
        mapComponent.View.ElevationProvider.RegisterMonitor(elevationMonitor);
    }

    private void UpdateSplinePosition(ArcGISPoint point)
    {
        // Update the spline knot at the specified index for this checkpoint
        BezierKnot bezierKnot = splineContainer.Splines[0][SplineIndex];
        bezierKnot.Position = mapComponent.GeographicToEngine(point);
        transform.SetPositionAndRotation(bezierKnot.Position, racingFeatureLayerComponent.GetSplineKnotRotation(SplineIndex + 1, SplineIndex));
        splineContainer.Splines[0][SplineIndex] = bezierKnot;
    }
}
