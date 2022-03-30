using System;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Security.AccessControl;
using UnityEditor;
using UnityEngine;

namespace ycdivfx.ProbeGenerator.Editor
{
    [CustomEditor(typeof(ProbeGenerator))]
    public class ProbeCreatorEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            var obj = (ProbeGenerator)target;
            var prob = obj.GetComponent<LightProbeGroup>();

            var refineProps = new[] { "KeepAllSamples", "SamplesPerPoint", "IgnoreBounds", "OffsetSamples", "UseHitNormal", "OffsetDistance", "SamplesMinDistance", "AnalyzeStaticObjectsOnly" };
            var randomProps = new[] { "Seed", "NumberOfProbes", "RandomSeed", "Iterations" };

            EditorGUI.BeginChangeCheck();
            serializedObject.Update();
            var iterator = serializedObject.GetIterator();
            for (bool enterChildren = true; iterator.NextVisible(enterChildren); enterChildren = false)
            {
                var enabled = "m_Script" == iterator.propertyPath ||
                              (iterator.propertyPath == "Seed" && obj.RandomSeed);
                if (obj.UseColliderVolume && iterator.propertyPath == "MeshVolume") continue;
                //if (obj.UseColliderVolume && iterator.propertyPath == "RespectMeshBounds") continue;
                if (!obj.UseColliderVolume && iterator.propertyPath == "ColliderVolume") continue;
                if (!obj.UseColliderVolume && !obj.Refine && iterator.propertyPath == "Mask") continue;
                enabled |= randomProps.Contains(iterator.propertyPath) && obj.Generator == ProbeGenerator.RandomGenerators.Grid;
                enabled |= iterator.propertyPath == "ShowGrid" && obj.Generator != ProbeGenerator.RandomGenerators.Grid;
                enabled |= (iterator.propertyPath == "UseHitNormal" || iterator.propertyPath == "OffsetDistance") && !obj.OffsetSamples;
                enabled |= refineProps.Contains(iterator.propertyPath) && !obj.Refine;
                enabled |= iterator.propertyPath == "ShowSampleRays" && !obj.ShowBasePoints;
                enabled |= iterator.propertyPath == "RespectMeshBounds" && !obj.UseColliderVolume;
                using (new EditorGUI.DisabledScope(enabled))
                    EditorGUILayout.PropertyField(iterator, true);
            }
            serializedObject.ApplyModifiedProperties();
            EditorGUI.EndChangeCheck();

            var bounds = obj.UseColliderVolume ? (obj.ColliderVolume != null ? obj.ColliderVolume.bounds : (Bounds?)null) : (obj.MeshVolume != null ? obj.MeshVolume.bounds : (Bounds?)null);
            var extents = bounds != null ? bounds.Value.extents : Vector3.zero;
            GUILayout.BeginVertical("box");
            var oldColor = GUI.color;
            var msg = !obj.UseColliderVolume && obj.MeshVolume == null ? "Missing mesh renderer." : string.Empty;
            msg += obj.UseColliderVolume && obj.ColliderVolume == null ? "Missing collider." : string.Empty;
            if (obj.UseColliderVolume && obj.ColliderVolume == null || !obj.UseColliderVolume && obj.MeshVolume == null)
                GUI.color = Color.yellow;
            else
                GUI.color = Color.green;
            if (string.IsNullOrEmpty(msg)) msg = "None";
            GUILayout.Label("Warnings: " + msg);
            GUI.color = oldColor;
            var xx = 0;
            var yy = 0;
            var zz = 0;
            var center = bounds != null ? bounds.Value.center : Vector3.zero;
            var total = obj.NumberOfProbes;
            if (bounds != null && obj.Generator == ProbeGenerator.RandomGenerators.Grid)
            {
                var extends = bounds.Value.extents;
                var spacing = obj.MinDistance;
                var min = center - extends;
                var max = center + extends;
                var ex = (max.x - min.x);
                var ey = (max.y - min.y);
                var ez = (max.z - min.z);
                xx = Mathf.CeilToInt(ex / spacing);
                yy = Mathf.CeilToInt(ey / spacing);
                zz = Mathf.CeilToInt(ez / spacing);
                total = xx * yy * zz;
                GUILayout.Label(string.Format("Cell count X: {0} Y:{1} Z:{2} Total:{3}", xx, yy, zz, total));
            }
            else GUILayout.Label("Selected generator: " + obj.Generator);
            GUILayout.Label(string.Format("Refine with {0} probes", obj.Refine ? obj.SamplesPerPoint * total : 0));
            GUILayout.BeginHorizontal();
            GUILayout.Label(string.Format("Center X: {0:##0.00} Y:{1:###0.00} Z:{2:##0.00}", center.x,
                center.y, center.z));
            GUILayout.Label(string.Format("Object bounds X: {0:##0.00} Y:{1:###0.00} Z:{2:##0.00}", extents.x,
                                          extents.y, extents.z));
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();

            GUILayout.BeginHorizontal();

            if (GUILayout.Button("Generate Probes"))
                EditorApplication.delayCall += () => GenerateProbes(obj, xx, yy, zz);
            GUILayout.Space(30);
            if (GUILayout.Button("Clear Probes"))
            {
                obj.GetComponent<LightProbeGroup>().probePositions = null;
                UpdateTetrahedraOnProbes();
            }
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            GUILayout.Label(string.Format("Probe count: {0:######}", prob.probePositions.Length));
            GUILayout.Label(string.Format("Took: {0:######}ms", obj.GenerationTime));
            GUILayout.EndHorizontal();
        }

        private static void GenerateProbes(ProbeGenerator obj, int xx, int yy, int zz)
        {
            var calculate = true;
            var total = xx * yy * zz;
            if (total > 16000)
            {
                EditorUtility.DisplayDialog(
                    "Error", "We have a 32000 probe count limit, Unity Editor performance for safety.", "Ok");
                calculate = false;
            }
            else if (total > 6000)
                calculate = EditorUtility.DisplayDialogComplex("Warning", string.Format("You are about to create {0} probes. Do you want to continue?", total), "Yes", "No", "Cancel") == 0;
            if (calculate)
                obj.Calculate();
            UpdateTetrahedraOnProbes();
        }

        private static void UpdateTetrahedraOnProbes()
        {
            SceneView.RepaintAll();
            var inspectorType = Assembly.GetAssembly(typeof(UnityEditor.Editor)).GetTypes()
                .FirstOrDefault(x => x.Name == "LightProbeGroupInspector");
            if (inspectorType == null) return;

            var lightProbeGroupInspector = Resources.FindObjectsOfTypeAll(inspectorType).FirstOrDefault();
            if (lightProbeGroupInspector == null) return;

            var f = lightProbeGroupInspector.GetType().GetField("m_Editor", BindingFlags.NonPublic | BindingFlags.Instance);
            if (f == null) return;

            var o = f.GetValue(lightProbeGroupInspector);
            var m = o.GetType().GetMethod("MarkTetrahedraDirty");
            if (m != null) m.Invoke(o, null);
        }
    }
}