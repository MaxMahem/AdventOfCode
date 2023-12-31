namespace AdventOfCode.Helpers;

public class GridPolygon<T>(IEnumerable<T> polygonPoints) where T : IGridPoint<int> {
    public IEnumerable<T> Points { get; } = polygonPoints?.ToImmutableArray() ?? throw new ArgumentNullException(nameof(polygonPoints));

    public double CalculatePerimeter() => this.Points.PairwiseLoop().Sum(pair => GridPoint<int>.Distance(pair.First, pair.Second));

    /// <summary>Calculates the area of the polygon.</summary>
    /// <remarks>Uses the Shoelace formula.</remarks>
    public double CalculateArea() => 
        Math.Abs(this.Points.PairwiseLoop().Select(pair => (pair.First.X * pair.Second.Y) - (pair.Second.X * pair.First.Y)).Sum()) / 2.0;

    /// <summary>Counts the number of interior grid points using Pickets Theorem.</summary>
    public int CountInteriorGridPoints() => (int)(CalculateArea() - Points.Count() / 2 + 1);

    public bool IsPointInsidePolygon(T point) {
        int intersectionCount = 0;
        foreach((T currentPoint, T nextPoint) in this.Points.PairwiseLoop()) { 
            if ((currentPoint.Y > point.Y && nextPoint.Y <= point.Y) || (nextPoint.Y > point.Y && currentPoint.Y <= point.Y)) {
                double intersectionX = (nextPoint.X - currentPoint.X) * (point.Y - currentPoint.Y) / (nextPoint.Y - currentPoint.Y) + currentPoint.X;

                if (point.X <= intersectionX) intersectionCount++;
            }
        }

        return intersectionCount % 2 != 0;
    }
}