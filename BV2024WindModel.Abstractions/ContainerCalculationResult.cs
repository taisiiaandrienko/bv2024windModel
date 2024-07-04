using Clipper2Lib;

namespace BV2024WindModel.Abstractions
{
    public class ContainerCalculationResult
    {
        public string ContainerId;
        public PathsD WindExposedPolygon;
        public double FullArea;
        public double ExposedArea; 
        public double FullHeight;
        public double FullWidth;
        public double VolumetricCenter;
        public double WindForceFull;
        public double WindForceForArea;
    } 
}
