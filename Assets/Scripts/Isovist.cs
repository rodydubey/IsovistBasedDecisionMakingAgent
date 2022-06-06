using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DefaultNamespace {
    [CreateAssetMenu(fileName = "Isovist", menuName = "Isovist", order = 0)]
    public class Isovist : ScriptableObject {

        private List<Vector3> isovistPolygon = new List<Vector3>();
        private Dictionary<IsovistMeasures, float> cachedMeasures = new Dictionary<IsovistMeasures, float>();

        public void AddPointFromWorld(Vector3 point, Vector3 worldPos) {
            isovistPolygon.Add(point - worldPos);
        }

        public void ClearPoints() {
            isovistPolygon.Clear();
        }

        public List<Vector3> GetPoints() {
            return isovistPolygon;
        }

        private float CalculatePerimeter() {
            var perimeter = 0f;
            for (var i = 0; i < isovistPolygon.Count - 1; i++)
            {
                perimeter += 
                    Mathf.Sqrt(
                        Mathf.Pow(isovistPolygon[i + 1].x - isovistPolygon[i].x,2) + 
                        Mathf.Pow(isovistPolygon[i + 1].z - isovistPolygon[i].z, 2)
                    );
            }
            return perimeter;
        }
        
        private float CalculateMaxRadialLength() {
            var magnitudes = isovistPolygon.Select(x => x.magnitude);
            return magnitudes.Any() ? isovistPolygon.Select(x => x.magnitude).Max() : 0.0f;
        }

        private float CalculateMinRadialLength() {
            return isovistPolygon.Where(x => x != Vector3.zero).Select(x => x.magnitude).Min();
        }

        private float SignedPolygonArea() {
            var area = 0f;
            
            for (int i = 0; i < isovistPolygon.Count - 1; i++) {
                area +=
                    (isovistPolygon[i + 1].x - isovistPolygon[i].x) *
                    (isovistPolygon[i + 1].z - isovistPolygon[i].z) / 2;
            }

            return area;
        }

        public Vector3 CalculateSumOfRays()
        {
            Vector3 sum = new Vector3();
            for (int i = 0; i < isovistPolygon.Count - 1; i++)
            {
                sum += isovistPolygon[i];
            }
            sum.Normalize();

            return sum;
        }

        private Vector2 CalculateCentroid() {
            var sumY = 0f;
            var sumX = 0f;
            var pSum = 0f;
            var sum = 0f;
            var centroid = new Vector2();

            for (int i = 0; i < isovistPolygon.Count - 1; i++) {
                pSum = Mat2Determinant(
                    isovistPolygon[i].x,
                    isovistPolygon[i].z,
                    isovistPolygon[i + 1].x,
                    isovistPolygon[i + 1].z
                );
                sum += pSum;
                sumX += (isovistPolygon[i].x + isovistPolygon[i + 1].x) * pSum;
                sumY += (isovistPolygon[i].z + isovistPolygon[i + 1].z) * pSum;
            }

            var area = .5f * sum;
            centroid.x = (sumX / 6) / area;
            centroid.y = (sumY / 6) / area;
            return centroid;
        }

        private float CalculateArea() {
            if (isovistPolygon.Count < 3) return 0f;

            var area = 0f;
            for (int i = 0; i < isovistPolygon.Count; i++) {
                area += Mat2Determinant(
                    isovistPolygon[i].x,
                    isovistPolygon[i].z,
                    isovistPolygon[(i + 1) % isovistPolygon.Count].x,
                    isovistPolygon[(i + 1) % isovistPolygon.Count].z
                );
            }
            
            return Mathf.Abs(area / 2);
        }

        private float CalculateRealSurfaceLength() {
            var temp = 0f;
            for (int i = 0; i < isovistPolygon.Count - 1; i++) {
                var dX = isovistPolygon[i + 1].x - isovistPolygon[i].x;
                var dZ = isovistPolygon[i + 1].z - isovistPolygon[i].z;
                if (Mathf.Abs(dX) < 3 && Mathf.Abs(dZ) < 3) {
                    temp += Mathf.Sqrt(dX * dX + dZ * dZ);
                }
            }

            return temp;
        }

        private float CalculateDrift() {
            var centroid = CalculateCentroid();
            return Mathf.Sqrt(centroid.x * centroid.x + centroid.y * centroid.y);
        }

        private float CalculateOpenness() {
            var perimeter = CalculatePerimeter();
            var surfaceLength = CalculateRealSurfaceLength();
            return (perimeter - surfaceLength) / surfaceLength;
        }
        
        private float CalculateJaggedness() {
            var perimeter = CalculatePerimeter();
            var area = CalculateArea();
            return (perimeter * perimeter) / area;
        }
        
        private float CalculateOcclusivity() {
            var perimeter = CalculatePerimeter();
            var surfaceLength = CalculateRealSurfaceLength();
            return (perimeter - surfaceLength) / perimeter;
        }

        public Dictionary<IsovistMeasures, float> CalculateIsovistMeasures() {
            cachedMeasures[IsovistMeasures.Perimeter] = CalculatePerimeter();
            cachedMeasures[IsovistMeasures.MaxRadialLength] = CalculateMaxRadialLength();
            cachedMeasures[IsovistMeasures.MinRadialLength] = CalculateMinRadialLength();
            cachedMeasures[IsovistMeasures.SignedPolygonArea] = SignedPolygonArea();
            cachedMeasures[IsovistMeasures.CentroidX] = CalculateCentroid().x;
            cachedMeasures[IsovistMeasures.CentroidY] = CalculateCentroid().y;
            cachedMeasures[IsovistMeasures.Area] = CalculateArea();
            cachedMeasures[IsovistMeasures.RealSurfaceLength] = CalculateRealSurfaceLength();
            cachedMeasures[IsovistMeasures.Drift] = CalculateDrift();
            cachedMeasures[IsovistMeasures.Openness] = CalculateOpenness();
            cachedMeasures[IsovistMeasures.Jaggedness] = CalculateJaggedness();
            cachedMeasures[IsovistMeasures.Occlusivity] = CalculateOcclusivity();
            cachedMeasures[IsovistMeasures.WallSumX] = CalculateSumOfRays().x;
            cachedMeasures[IsovistMeasures.WallSumZ] = CalculateSumOfRays().z;

            return cachedMeasures;
        }
        
        private static float Mat2Determinant(float x1, float y1, float x2, float y2) {
            return x1 * y2 - x2 * y1;
        }
        
        public enum IsovistMeasures {
            Perimeter,
            MaxRadialLength,
            MinRadialLength,
            SignedPolygonArea,
            CentroidX,
            CentroidY,
            Area,
            RealSurfaceLength,
            Drift,
            Openness,
            Jaggedness,
            Occlusivity,
            WallSumX,
            WallSumZ
        }
    }
}