using ProjNet.CoordinateSystems;
using ProjNet.CoordinateSystems.Projections;
using ProjNet.CoordinateSystems.Transformations;
using ProjNet;

public static class CoordinateTransformHelper
{
    private static readonly CoordinateSystemFactory CoordinateSystemFactory = new CoordinateSystemFactory();
    private static readonly CoordinateTransformationFactory TransformationFactory = new CoordinateTransformationFactory();

    private static readonly GeographicCoordinateSystem Wgs84 = CoordinateSystemFactory.CreateGeographicCoordinateSystem(
        "WGS84",
        AngularUnit.Degrees,
        HorizontalDatum.WGS84,
        PrimeMeridian.Greenwich,
        new AxisInfo("Latitude", AxisOrientationEnum.North),
        new AxisInfo("Longitude", AxisOrientationEnum.East));

    private static readonly ProjectedCoordinateSystem ITM = CoordinateSystemFactory.CreateProjectedCoordinateSystem(
        "ITM",
        Wgs84,
        CoordinateSystemFactory.CreateProjection(
            "Transverse Mercator",
            "Transverse_Mercator",
         new List<ProjectionParameter>() {  
                new ProjectionParameter("latitude_of_origin", 31.7343936111),
                new ProjectionParameter("central_meridian", 35.2045169444),
                new ProjectionParameter("scale_factor", 1.0000067),
                new ProjectionParameter("false_easting", 219529.584),
                new ProjectionParameter("false_northing", 626907.39)
            }),
        LinearUnit.Metre,
        new AxisInfo("East", AxisOrientationEnum.East),
        new AxisInfo("North", AxisOrientationEnum.North));

    private static readonly ICoordinateTransformation Wgs84ToItmTransformation = TransformationFactory.CreateFromCoordinateSystems(Wgs84, ITM);

    public static (double Easting, double Northing) TransformToItm(double latitude, double longitude)
    {
        var transformed = Wgs84ToItmTransformation.MathTransform.Transform(new[] { longitude, latitude });
        return (transformed[0], transformed[1]);
    }
}
