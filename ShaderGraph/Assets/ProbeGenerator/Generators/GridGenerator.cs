using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace ycdivfx.ProbeGenerator.Generators
{
    public class GridGenerator : UnityRnd
    {
        private Vector3 _center;
        private Vector3 _extends;
        private float _spacing;

        public GridGenerator(int seed) : base(seed)
        {
            
        }

        public GridGenerator(Vector3 center, Vector3 extends, float spacing)
        {
            _center = center;
            _extends = extends;
            _spacing = spacing;
        }

        public void Init(Vector3 center, Vector3 extends, float spacing)
        {
            _center = center;
            _extends = extends;
            _spacing = spacing;
        }


        public IList<Vector3> GenerateGrid()
        {
            var min = _center - _extends;
            var max = _center + _extends;
            var ex = (max.x - min.x);
            var ey = (max.y - min.y);
            var ez = (max.z - min.z);
            var xx = Mathf.CeilToInt(ex / _spacing);
            var yy = Mathf.CeilToInt(ey / _spacing);
            var zz = Mathf.CeilToInt(ez / _spacing);

            Debug.LogFormat("Distance: {0} {1} {2}", ex, ey, ez);

            Debug.LogFormat("Grid cells: {0} {1} {2}", xx, yy, zz);

            var points = new List<Vector3>();

            var total = xx * yy * zz;
            if (total > 32000)
            {
                Debug.LogErrorFormat("Too many grid cells to calculate. {0} cells", total);
                return points;
            }

#if UNITY_EDITOR
            UnityEditor.EditorUtility.DisplayCancelableProgressBar("Grid Generator", "Probe generation", 0f);
            var progress = 0f;
            var lastP = 0;
#endif
            for (var x = 0; x < xx; x++)
            {
                for (var y = 0; y < yy; y++)
                {
                    for (var z = 0; z < zz; z++)
                    {
                        points.Add(min + new Vector3(x, y, z) * _spacing);
#if UNITY_EDITOR
                        var f = (progress) /total;
                        var p = Mathf.RoundToInt(f * 100);
                        if (p != lastP && UnityEditor.EditorUtility.DisplayCancelableProgressBar(
                            "Grid Generator", "Probe generation", f))
                            goto End;
                        lastP = p;
                        progress++;
#endif
                    }
                }
            }
            Debug.LogFormat("Progress: {0}, Total: {1}", progress, total);
            End:
#if UNITY_EDITOR
            UnityEditor.EditorUtility.ClearProgressBar();
#endif

            return points;
        }
    }
}
