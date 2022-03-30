using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using ycdivfx.ProbeGenerator.Generators;
using UnityEngine;

namespace ycdivfx.ProbeGenerator
{
    [ExecuteInEditMode]
    [RequireComponent(typeof(LightProbeGroup))]
    public class ProbeGenerator : MonoBehaviour
    {
        private LightProbeGroup _probes;
        [Header("Setup")]
        public bool UseColliderVolume = true;
        public Collider ColliderVolume;
        public MeshRenderer MeshVolume;
        public bool RespectMeshBounds = true;
        [Tooltip("This is the raycasting mask. It works in both physics and octree refine modes.")]
        public LayerMask Mask = -1;

        [Header("Random Options")]
        public int Seed = 123456;

        public bool RandomSeed = true;
        public RandomGenerators Generator = RandomGenerators.Unity;

        [Header("Probe Generation")]
        [Range(1, 32000)]
        public int NumberOfProbes = 300;

        [Range(0.001f, 10f)]
        public float MinDistance = 0.1f;

        [Range(1, 500)]
        public int Iterations = 100;

        [Header("Refine")]
        [Tooltip("Refines the probe distribution using collider information.")]
        public bool Refine = true;

        public RefineMethods RefineMethod = RefineMethods.PhysicsRaycast;

        [Tooltip("Ignores any object that is not set to static.")]
        public bool AnalyzeStaticObjectsOnly = true;
        [Tooltip("Ignores the probe generator bounds.")]
        public bool IgnoreBounds = false;
        [Tooltip("Keeps all the samples. This causes more probes to be generated than the specified.")]
        public bool KeepAllSamples = true;

        [Tooltip("Samples to send from each random point in volume. One sample or below causes samples to use the center a origin.")]
        public int SamplesPerPoint = 2;

        [Range(0.001f, 10f)]
        public float SamplesMinDistance = 0.1f;
        [Space]
        public bool OffsetSamples = true;

        [Tooltip("Uses the collider hit normal, or the original sample point - hit point as a normal for offset.")]
        public bool UseHitNormal = true;

        [Tooltip("Offset distance.")]
        public float OffsetDistance = 0.1f;

#if UNITY_EDITOR
        [Header("Visualization")]
        public bool ShowBasePoints;
        public bool ShowSampleRays;
        public bool ShowGrid;
        [Tooltip("Maximum number of visible points before disabling.")]
        public int HardLimit = 60000;
        [Tooltip("Maximum computation time in ms before disabling.")]
        public float ResponseTime = 500;
#endif

        [HideInInspector]
        [NonSerialized]
        public long GenerationTime;

        private MeshFilter _meshFilter;
        private Func<Vector3, bool> _isPointInsideObject;
#if UNITY_EDITOR
        private bool _isDirty;
        private List<Vector3> _cachePoints = new List<Vector3>();
        private readonly List<Ray> _cacheRays = new List<Ray>();
        private readonly List<Vector3> _cacheLines = new List<Vector3>();
        private Vector3 _lastPosition;
        private Vector3 _lastScale;
        private Quaternion _lastRotation;
#endif

        protected Func<Vector3, bool> IsPointInsideObject
        {
            get
            {
                if (_isPointInsideObject != null) return _isPointInsideObject;
                if (UseColliderVolume)
                    _isPointInsideObject = IsPointInColliders;
                else
                    _isPointInsideObject = IsPoinInsideMesh;
                return _isPointInsideObject;
            }
        }

        public enum RandomGenerators
        {
            Unity,
            System,
            Grid,
            MultiplicativeCongruential31,
            MultiplicativeCongruential59,
            MersenneTwister,
            CombinedMultipleRecursive,
            WH1982,
            WH2006,
            XorShift
        }

        public enum RefineMethods
        {
            PhysicsRaycast,
            PhysicsSphereCast,
            OctreeRaycast
        }

        // Use this for initialization
        protected void Start()
        {
            _probes = GetComponent<LightProbeGroup>();
        }

        private RandomSource GetGenerator()
        {
            switch (Generator)
            {
                case RandomGenerators.Unity:
                    return new UnityRnd(Seed);
                case RandomGenerators.System:
                    return new RandomSystemSource(Seed);
                case RandomGenerators.Grid:
                    return new GridGenerator(Seed);
                case RandomGenerators.MultiplicativeCongruential31:
                    return new Mcg31m1(Seed);
                case RandomGenerators.MultiplicativeCongruential59:
                    return new Mcg59(Seed);
                case RandomGenerators.MersenneTwister:
                    return new MersenneTwister(Seed);
                case RandomGenerators.CombinedMultipleRecursive:
                    return new Mrg32k3a(Seed);
                case RandomGenerators.WH1982:
                    return new WH1982(Seed);
                case RandomGenerators.WH2006:
                    return new WH2006(Seed);
                case RandomGenerators.XorShift:
                    return new Xorshift(Seed);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        // Update is called once per frame
        public void Calculate()
        {
            if (!MeshVolume && !UseColliderVolume) return;
            if (!ColliderVolume && UseColliderVolume) return;

            if (RandomSeed && Generator != RandomGenerators.Grid)
            {
                Seed = Generators.RandomSeed.Robust();
            }

            var generator = GetGenerator();

            Bounds bounds;
            if (UseColliderVolume)
            {
                bounds = ColliderVolume.bounds;
            }
            else
            {
                _meshFilter = MeshVolume.GetComponent<MeshFilter>();
                bounds = MeshVolume.bounds;
            }
            
            var center = bounds.center;
            var maxRadius = bounds.extents.magnitude;
            var extents = bounds.extents;
            var tm = Stopwatch.StartNew();

            var gridGenerator = generator as GridGenerator;

            if (UseColliderVolume)
                _isPointInsideObject = IsPointInColliders;
            else
                _isPointInsideObject = IsPoinInsideMesh;


            var initialPoints = new List<Vector3>();
            var points = new List<Vector3>();

            Action<Vector3, List<Vector3>, Ray, float> shootRays;
            BoundsOctree<TriMesh> boundsOctree;
            switch (RefineMethod)
            {
                case RefineMethods.PhysicsRaycast:
                    shootRays = PhysicsRaycast;
                    break;
                case RefineMethods.PhysicsSphereCast:
                    shootRays = PhysicsSphereCast;
                    break;
                case RefineMethods.OctreeRaycast:
                    boundsOctree = OctreeRaycast.BuildOctree(bounds);
                    shootRays = (origin, pts, r, maxDistance) =>
                        BruteOctreeRaycast(origin, pts, r, maxDistance, boundsOctree);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            if (gridGenerator != null)
            {
                gridGenerator.Init(center, extents, MinDistance);
                var basePoints = gridGenerator.GenerateGrid();
                initialPoints.AddRange(from point in basePoints where _isPointInsideObject(point) select transform.InverseTransformPoint(point));
            }
            else
            {
#if UNITY_EDITOR
                UnityEditor.EditorUtility.DisplayCancelableProgressBar("Grid Generator", "Probe generation", 0f);
                var progress = 0f;
                var lastPr = 0;
#endif
                var maxProbes = NumberOfProbes;
                var i = 0;
                while (maxProbes > 0)
                {
                    var iterations = Iterations;
                    var p = Vector3.zero;
#if UNITY_EDITOR
                    var pr = Mathf.RoundToInt((float)i / NumberOfProbes * 100f);
                    if (pr != lastPr)
                        if (UnityEditor.EditorUtility.DisplayCancelableProgressBar(
                            "Point generation phase", "Point generation", progress / NumberOfProbes))
                            break;
                    lastPr = pr;
                    progress++;
#endif
                    i++;
                    while (iterations > 0)
                    {

                        p = center + generator.NextVector3(-maxRadius, maxRadius);
                        if (IsAboveDistance(p, initialPoints, MinDistance) && _isPointInsideObject(p))
                            break;
                        iterations--;

                    }
                    if (iterations == 0)
                    {
                        maxProbes--;
                        //Debug.LogWarning("Got to the limit of iterations.");
                        continue;
                    }
                    // If we are going to refine we need to use world space point, if not use the local space point.
                    initialPoints.Add(p);
                    maxProbes--;
                }
#if UNITY_EDITOR
                _cachePoints = initialPoints;
#endif
                if (!Refine)
                    initialPoints = initialPoints.Select(x => transform.InverseTransformPoint(x)).ToList();
#if UNITY_EDITOR
                UnityEditor.EditorUtility.ClearProgressBar();
#endif
            }

            if (Refine)
            {
                UnityEngine.Random.InitState(Seed);
                var progress = 0f;
                var lastPr = 0;
                var i = 0;
                var total = KeepAllSamples ? (initialPoints.Count * SamplesPerPoint) : NumberOfProbes;
                foreach (var point in initialPoints)
                {
#if UNITY_EDITOR
                    var pr = Mathf.RoundToInt((float)i / total * 100f);
                    if (pr != lastPr)
                        if (UnityEditor.EditorUtility.DisplayCancelableProgressBar("Refinement phase", "Refinement phase", (progress) / total))
                            break;
                    lastPr = pr;
                    progress++;
#endif
                    i++;
                    if (SamplesPerPoint <= 1)
                    {
                        var r = point.ToRay(center - point);
                        shootRays(point, points, r, maxRadius * 2);
                    }
                    else
                    {
                        var samples = SamplesPerPoint;
                        while (samples > 0)
                        {
                            samples--;
                            var r = point.ToRay(UnityEngine.Random.insideUnitSphere);
                            shootRays(point, points, r, maxRadius * 2);
                        }
                    }
                    if (!KeepAllSamples && points.Count > NumberOfProbes)
                        break;
                }
#if UNITY_EDITOR
                UnityEditor.EditorUtility.ClearProgressBar();
#endif
            }

            _probes.probePositions = !Refine ? initialPoints.ToArray() : points.ToArray();

            tm.Stop();
            GenerationTime = tm.ElapsedMilliseconds;

            print(string.Format("Random probe generation took: {0}ms", GenerationTime));
        }

        private bool IsPointInColliders(Vector3 point)
        {
            return Physics.OverlapSphere(point, 0).Any(x => x == ColliderVolume);
        }

        private bool IsPoinInsideMesh(Vector3 point)
        {
            var invP = transform.InverseTransformPoint(point);
            var result = false;

            if (!RespectMeshBounds)
                return true;

            float distance;

            if (IsPointInside(_meshFilter.mesh, invP, out distance))
                result = true;

            return result;
        }

        private void PhysicsRaycast(Vector3 origin, List<Vector3> points, Ray r, float maxDistance)
        {
            var hits = Physics.RaycastAll(r, maxDistance, Mask, QueryTriggerInteraction.Collide);
            var raycastHits = hits.Where(x => x.collider != ColliderVolume);
            foreach (var hit in raycastHits)
            {
                if (!hit.collider.gameObject.isStatic && AnalyzeStaticObjectsOnly) continue;
                var invPoint = hit.point;
                if (OffsetSamples)
                {
                    var n = UseHitNormal ? hit.normal.normalized : (origin - hit.point).normalized;
                    invPoint = invPoint + (n * OffsetDistance);
                }
                invPoint = transform.InverseTransformPoint(invPoint);
                if (!IgnoreBounds && !_isPointInsideObject(transform.TransformPoint(invPoint))) continue;

                if (!IsAboveDistance(invPoint, points.ToArray(), SamplesMinDistance)) continue;

                points.Add(invPoint);
            }
        }

        private void PhysicsSphereCast(Vector3 origin, List<Vector3> points, Ray r, float maxDistance)
        {
            var hits = Physics.SphereCastAll(r, 0.01f, maxDistance, Mask, QueryTriggerInteraction.Collide);
            var raycastHits = hits.Where(x => x.collider != ColliderVolume);
            foreach (var hit in raycastHits)
            {
                if (!hit.collider.gameObject.isStatic && AnalyzeStaticObjectsOnly) continue;
                var invPoint = hit.point;
                if (OffsetSamples)
                {
                    var n = UseHitNormal ? hit.normal.normalized : (origin - hit.point).normalized;
                    invPoint = invPoint + (n * OffsetDistance);
                }
                invPoint = transform.InverseTransformPoint(invPoint);
                if (!IgnoreBounds && !_isPointInsideObject(transform.TransformPoint(invPoint))) continue;

                if (!IsAboveDistance(invPoint, points.ToArray(), SamplesMinDistance)) continue;

                points.Add(invPoint);
            }
        }

        private void BruteOctreeRaycast(Vector3 origin, List<Vector3> points, Ray r, float maxDistance, BoundsOctree<TriMesh> o)
        {
            var raycastHits = OctreeRaycast.RaycastAll(o, r, maxDistance, Mask);
            foreach (var hit in raycastHits)
            {
                if (!hit.Transform.gameObject.isStatic && AnalyzeStaticObjectsOnly) continue;
                var invPoint = hit.Point;
                if (OffsetSamples)
                {
                    var n = UseHitNormal ? hit.Normal.normalized : (origin - hit.Point).normalized;
                    invPoint = invPoint + (n * OffsetDistance);
                }
                invPoint = transform.InverseTransformPoint(invPoint);

                if (!IgnoreBounds && !_isPointInsideObject(transform.TransformPoint(invPoint))) continue;

                if (!IsAboveDistance(invPoint, points.ToArray(), SamplesMinDistance)) continue;

                points.Add(invPoint);
            }
        }

        private static bool IsAboveDistance(Vector3 point, IEnumerable<Vector3> points, float distance)
        {
            var result = true;
            foreach (var p in points)
                if (Mathf.Abs(Vector3.Distance(p, point)) < distance) result = false;
            return result;
        }

        private static bool IsPointInside(Mesh mesh, Vector3 point, out float distance)
        {
            var verts = mesh.vertices;
            var tris = mesh.triangles;
            var triangleCount = tris.Length / 3;
            distance = float.MaxValue;
            for (var i = 0; i < triangleCount; i++)
            {
                var v1 = verts[tris[i * 3]];
                var v2 = verts[tris[i * 3 + 1]];
                var v3 = verts[tris[i * 3 + 2]];
                var p = new Plane(v1, v2, v3);

                if (p.GetSide(point))
                    return false;

                var d = Mathf.Abs(p.GetDistanceToPoint(point));
                if (d < distance)
                    distance = d;
            }
            return true;
        }

        private void Reset()
        {
            ColliderVolume = GetComponent<Collider>();
            MeshVolume = GetComponent<MeshRenderer>();
        }

        protected void OnValidate()
        {
#if UNITY_EDITOR
            _isDirty = true;
#endif
        }

        protected void Update()
        {
#if UNITY_EDITOR
            if (transform.position == _lastPosition && transform.localScale == _lastScale &&
                transform.rotation == _lastRotation) return;
            _lastPosition = transform.position;
            _lastScale = transform.localScale;
            _lastRotation = transform.rotation;
            _isDirty = true;
#endif
        }

        protected void OnDrawGizmosSelected()
        {
#if UNITY_EDITOR
            var oldColor = UnityEditor.Handles.color;
            if (!_isDirty)
            {
                if (ShowGrid && Generator == RandomGenerators.Grid)
                {
                    UnityEditor.Handles.color = Color.yellow;
                    for (var i = 0; i < _cacheLines.Count; i += 2)
                    {
                        var p1 = _cacheLines[i];
                        var p2 = _cacheLines[i + 1];
                        UnityEditor.Handles.DrawLine(p1, p2);
                    }
                }
                if (!ShowBasePoints) return;
                UnityEditor.Handles.color = Color.green;
                foreach (var cachePoint in _cachePoints)
                    UnityEditor.Handles.SphereHandleCap(0, cachePoint, Quaternion.identity, 0.4f,
                        EventType.Repaint);
                if (ShowSampleRays)
                {
                    var oldGizmoColor = Gizmos.color;
                    Gizmos.color = Color.red;

                    foreach (var cacheRay in _cacheRays)
                    {
                        Gizmos.DrawRay(cacheRay);
                        //UnityEditor.Handles.DrawLine(cacheRay.origin, cacheRay.origin + cacheRay.direction);
                    }
                    Gizmos.color = oldGizmoColor;

                }
                UnityEditor.Handles.color = oldColor;
                return;
            }
            _isDirty = false;
            _cachePoints.Clear();
            _cacheLines.Clear();
            var bounds = UseColliderVolume ? (ColliderVolume != null ? ColliderVolume.bounds : (Bounds?)null) : (MeshVolume != null ? MeshVolume.bounds : (Bounds?)null);
            var extents = bounds != null ? bounds.Value.extents : Vector3.zero;

            var tm = Stopwatch.StartNew();

            if (ShowGrid || (ShowBasePoints && Generator == RandomGenerators.Grid))
            {
                if (Generator != RandomGenerators.Grid) return;

                if (bounds == null) return;

                var min = bounds.Value.center - extents;
                var max = bounds.Value.center + extents;
                var ex = max.x - min.x;
                var ey = max.y - min.y;
                var ez = max.z - min.z;
                var xx = Mathf.CeilToInt(ex / MinDistance);
                var yy = Mathf.CeilToInt(ey / MinDistance);
                var zz = Mathf.CeilToInt(ez / MinDistance);
                var total = xx * yy * zz;
                if (total > HardLimit)
                    return;
                if (ShowGrid)
                {
                    UnityEditor.Handles.color = Color.yellow;
                    for (var x = 0; x <= xx; x++)
                    {
                        for (var z = 0; z <= zz; z++)
                        {
                            var p1 = min + new Vector3(x, 0, z) * MinDistance;
                            var p2 = min + new Vector3(x, yy, z) * MinDistance;
                            //UnityEditor.Handles.DrawLine(p1, p2);
                            _cacheLines.Add(p1);
                            _cacheLines.Add(p2);
                            if (tm.ElapsedMilliseconds > ResponseTime) goto End;
                        }
                    }
                    for (var y = 0; y <= yy; y++)
                    {
                        for (var z = 0; z <= zz; z++)
                        {
                            var p1 = min + new Vector3(0, y, z) * MinDistance;
                            var p2 = min + new Vector3(xx, y, z) * MinDistance;
                            //UnityEditor.Handles.DrawLine(p1, p2);
                            _cacheLines.Add(p1);
                            _cacheLines.Add(p2);
                            if (tm.ElapsedMilliseconds > ResponseTime) goto End;
                        }
                    }
                    for (var x = 0; x <= xx; x++)
                    {
                        for (var y = 0; y <= yy; y++)
                        {
                            var p1 = min + new Vector3(x, y, 0) * MinDistance;
                            var p2 = min + new Vector3(x, y, zz) * MinDistance;
                            //UnityEditor.Handles.DrawLine(p1, p2);
                            _cacheLines.Add(p1);
                            _cacheLines.Add(p2);
                            if (tm.ElapsedMilliseconds > ResponseTime) goto End;
                        }
                    }
                }
                if (ShowBasePoints)
                {
                    UnityEditor.Handles.color = Color.green;
                    for (var x = 0; x < xx; x++)
                    {
                        for (var y = 0; y < yy; y++)
                        {
                            for (var z = 0; z < zz; z++)
                            {
                                var p = min + new Vector3(x, y, z) * MinDistance;
                                //UnityEditor.Handles.SphereHandleCap(0, p, Quaternion.identity, 0.4f, EventType.Repaint);
                                _cachePoints.Add(p);
                                if (tm.ElapsedMilliseconds > ResponseTime) goto End;
                            }
                        }
                    }
                }
                End:;
                //print("Aborted");
            }
            else if (ShowBasePoints && Generator != RandomGenerators.Grid && bounds != null)
            {
                var generator = GetGenerator();
                var maxProbes = NumberOfProbes > HardLimit ? HardLimit : NumberOfProbes;
                var maxRadius = extents.magnitude;
                var initialPoints = new List<Vector3>();
                UnityEditor.Handles.color = Color.green;
                while (maxProbes > 0)
                {
                    var iterations = Iterations;
                    var p = Vector3.zero;
                    while (iterations > 0)
                    {

                        if (tm.ElapsedMilliseconds > ResponseTime) break;
                        p = bounds.Value.center + generator.NextVector3(-maxRadius, maxRadius);
                        if (IsAboveDistance(p, initialPoints, MinDistance) && IsPointInsideObject(p))
                            break;
                        iterations--;

                    }
                    if (iterations == 0)
                    {
                        maxProbes--;
                        continue;
                    }
                    if (tm.ElapsedMilliseconds > ResponseTime) break;
                    //UnityEditor.Handles.SphereHandleCap(0, p, Quaternion.identity, 0.4f, EventType.Repaint);
                    _cachePoints.Add(p);
                    initialPoints.Add(p);
                    maxProbes--;
                }
            }
            if (ShowSampleRays && bounds != null)
            {
                _cacheRays.Clear();
                foreach (var point in _cachePoints)
                {
                    if (SamplesPerPoint <= 1)
                    {
                        var r = point.ToRay((bounds.Value.center - point));
                        _cacheRays.Add(r);
                    }
                    else
                    {
                        var samples = SamplesPerPoint;
                        while (samples > 0)
                        {
                            samples--;
                            var r = point.ToRay(UnityEngine.Random.insideUnitSphere);
                            _cacheRays.Add(r);
                            if (tm.ElapsedMilliseconds > ResponseTime) break;
                        }
                    }
                    if (tm.ElapsedMilliseconds > ResponseTime) break;
                }
            }
            UnityEditor.Handles.color = oldColor;
#endif
        }
    }
}