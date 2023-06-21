// using System.Linq;
// using System.Collections.Generic;
// using UnityEngine;

// using SevenGame.Utility;
// using EasySplines;
// using UnityEngine.Splines;
// using Unity.Mathematics;

// namespace SeleneGame.Core {
    
//     [ExecuteInEditMode]
//     [RequireComponent(typeof(SplineContainer))]
//     [RequireComponent(typeof(MeshFilter))]
//     [RequireComponent(typeof(MeshCollider))]
//     [DisallowMultipleComponent]
//     public class SplineMesh : MonoBehaviour {

//         private SplineContainer _splineContainer;
//         private Mesh _mesh;
//         private MeshFilter _meshFilter;
//         private MeshCollider _meshCollider;

//         public RepeatableMesh mesh2D;
//         [Range(3, 128)]
//         public int ringCount = 4;
//         public float scale = 1f;


//         public SplineContainer splineContainer {
//             get {
//                 if (_splineContainer == null) 
//                     _splineContainer = GetComponent<SplineContainer>();
//                 return _splineContainer;
//             }
//         }
//         public MeshFilter meshFilter {
//             get {
//                 if (_meshFilter == null) 
//                     _meshFilter = GetComponent<MeshFilter>();
//                 return _meshFilter;
//             }
//         }
//         public MeshCollider meshCollider {
//             get {
//                 if (_meshCollider == null) 
//                     _meshCollider = GetComponent<MeshCollider>();
//                 return _meshCollider;
//             }
//         }


//         private void OnEnable() {
//             UnityEngine.Splines.Spline.Changed += OnSplineChanged;
//         }

//         private void OnDisable() {
//             UnityEngine.Splines.Spline.Changed -= OnSplineChanged;
//         }
        


//         private void OnSplineChanged(UnityEngine.Splines.Spline spline, int index, SplineModification modif) {

//             if (splineContainer.Splines.Contains(spline)) {
//                 UpdateMesh();
//             }
//         }

//         private void OnValidate() {
//             UpdateMesh();
//         }

//         private void BuildCurveMesh(UnityEngine.Splines.BezierCurve curve, List<Vector3> vertices, List<Vector3> normals, List<Vector2> uvs, List<int> triangles, UnityEngine.Splines.Spline spline, int ringCount = 4, float scale = 1f) {

//             int vertexOffset = vertices.Count;

//             for (int ring = 0; ring < ringCount; ring++){

//                 float curveT = ring / (ringCount-1f);
//                 Vector3 position = CurveUtility.EvaluatePosition(curve, curveT);
//                 Vector3 tangent = CurveUtility.EvaluateTangent(curve, curveT);

//                 float splineT = spline.CurveToSplineT(curveT);

//                 int startCurveIndex = spline.SplineToCurveT(splineT, out _);
//                 Vector3 up = SplineUtility.EvaluateUpVector(spline, splineT);

//                 // startCurveIndex = spline.PreviousIndex(startCurveIndex);
//                 // int endCurveIndex = spline.NextIndex(startCurveIndex);
//                 // Vector3 startUp = math.rotate(spline[startCurveIndex].Rotation, math.up());
//                 // Vector3 endUp = math.rotate(spline[endCurveIndex].Rotation, math.up());
//                 // Vector3 up = Vector3.Slerp(startUp, endUp, curveT);
                
//                 Quaternion rotation = Quaternion.LookRotation(tangent, up);

//                 for (int j = 0; j < mesh2D.vertexCount; j++){
//                     vertices.Add(position + (rotation * mesh2D.vertices[j].point)*scale);
//                     normals.Add(rotation * mesh2D.vertices[j].normal);
//                     uvs.Add(new Vector2(mesh2D.vertices[j].UCoord, curveT));

//                 }
//             }

//             for (int ring = 0; ring < (ringCount-1f); ring++){
//                 int rootIndex = ring * mesh2D.vertexCount;
//                 int rootIndexNext = (ring+1) * mesh2D.vertexCount;

//                 for (int line = 0; line < mesh2D.lineCount; line++){
//                     int lineIndexA = mesh2D.segmentIndices[line].vert1;
//                     int lineIndexB = mesh2D.segmentIndices[line].vert2;

//                     int currentA = vertexOffset + rootIndex + lineIndexA;
//                     int currentB = vertexOffset + rootIndex + lineIndexB;
//                     int nextA = vertexOffset + rootIndexNext + lineIndexA;
//                     int nextB = vertexOffset + rootIndexNext + lineIndexB;

//                     triangles.Add(currentA);
//                     triangles.Add(nextA);
//                     triangles.Add(nextB);
//                     triangles.Add(currentA);
//                     triangles.Add(nextB);
//                     triangles.Add(currentB);
//                 }

//             }
//         }

//         private void BuildSplineMesh(UnityEngine.Splines.Spline spline, List<Vector3> vertices, List<Vector3> normals, List<Vector2> uvs, List<int> triangles, int ringCount = 4, float scale = 1f) {

//             int curveCount = spline.Closed ? spline.Count + 1 : spline.Count;

//             if (curveCount < 1) return;

//             for (int i = 1; i < curveCount; i++) {
//                 UnityEngine.Splines.BezierCurve curve = spline.GetCurve(i - 1);
//                 BuildCurveMesh(curve, vertices, normals, uvs, triangles, spline, ringCount, scale);
//             }
//         }

//         public void UpdateMesh() {

//             if (mesh2D == null) {
//                 meshFilter.sharedMesh = null;
//                 meshCollider.sharedMesh = null;
//                 return;
//             }

//             if (_mesh != null) {
//                 _mesh.Clear();
//             } else {
//                 _mesh = new Mesh();
//                 _mesh.name = $"Procedural {mesh2D.name} mesh";
//             }

//             List<Vector3> vertices = new List<Vector3>();
//             List<Vector3> normals = new List<Vector3>();
//             List<Vector2> uvs = new List<Vector2>();
//             List<int> triangles = new List<int>();

//             foreach (UnityEngine.Splines.Spline spline in splineContainer.Splines) {
//                 BuildSplineMesh(spline, vertices, normals, uvs, triangles, ringCount, scale);
//             }

//             _mesh.SetVertices(vertices);
//             _mesh.SetNormals(normals);
//             _mesh.SetUVs(0, uvs);

//             _mesh.SetTriangles(triangles, 0);

//             meshFilter.sharedMesh = _mesh;
//             meshCollider.sharedMesh = _mesh;
//         }
//     }
// }
