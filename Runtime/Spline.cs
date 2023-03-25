using System.Collections;
using System.Collections.Generic;

using UnityEngine;

using SevenGame.Utility;

namespace EasySplines {
    
    [RequireComponent(typeof(MeshFilter))]
    [RequireComponent(typeof(MeshCollider))]
    [DisallowMultipleComponent]
    public sealed class Spline : MonoBehaviour {

        private Mesh mesh;
        private MeshFilter _meshFilter;
        private MeshCollider _meshCollider;
        
        public SegmentType segmentType = SegmentType.BezierCubic;
        [SerializeField] private LineSegment _linearSegment;
        [SerializeField] private BezierCubic _bezierCubic;
        [SerializeField] private BezierQuadratic _bezierQuadratic;

        public Spline previousSpline;
        public Spline nextSpline;

        
        public RepeatableMesh mesh2D;
        [Range(3, 128)]
        public int ringCount = 4;
        public float scale = 1f;

        public bool hasStoppingPoint = false;
        public float stoppingPoint = 0.5f;




        public Segment segment {
            get {
                switch (segmentType) {
                    default:
                        return _linearSegment;
                    case SegmentType.BezierCubic:
                        return _bezierCubic;
                    case SegmentType.BezierQuadratic:
                        return _bezierQuadratic;
                }
            }
        }

        public MeshFilter meshFilter {
            get {
                if (_meshFilter == null) 
                    _meshFilter = GetComponent<MeshFilter>();
                return _meshFilter;
            }
        }
        public MeshCollider meshCollider {
            get {
                if (_meshCollider == null) 
                    _meshCollider = GetComponent<MeshCollider>();
                return _meshCollider;
            }
        }
        
        public float length => segment.length;
        public float[] arcLengths {
            get {
                if (segment is Curve curve)
                    return curve.arcLengths;
                Debug.Log("Spline.arcLengths: segment is not a curve");
                return null;
            }
        }




        public OrientedPoint GetPoint(float tVal) => segment.GetPoint(tVal);
        public OrientedPoint GetPointUniform(float tVal) => segment.GetPoint( segment.GetUniformT(tVal) );
        public OrientedPoint GetPointWithDistance(float distance) => segment.GetPoint( segment.GetTOfDistance(distance) );
        public Vector3 GetTangent(float tVal) => segment.GetTangent(tVal);

        public void UpdateMesh() {
            
            segment.UpdateLength();
            

            if (mesh2D == null) {
                meshFilter.sharedMesh = null;
                meshCollider.sharedMesh = null;
                return;
            }

            if (mesh != null) {
                mesh.Clear();
            } else {
                mesh = new Mesh();
                mesh.name = $"Procedural {mesh2D.name} mesh";
            }

            List<Vector3> vertices = new List<Vector3>();
            List<Vector3> normals = new List<Vector3>();
            List<Vector2> uvs = new List<Vector2>();
            List<int> triangles = new List<int>();

            for (int ring = 0; ring < ringCount; ring++){

                float t = ring / (ringCount-1f);
                OrientedPoint op = transform.InverseTransform(segment.GetPoint( segment.GetUniformT(t) ));

                for (int j = 0; j < mesh2D.vertexCount; j++){
                    vertices.Add(op.position + (op.rotation * mesh2D.vertices[j].point)*scale);
                    normals.Add(op.rotation * mesh2D.vertices[j].normal);
                    uvs.Add(new Vector2(mesh2D.vertices[j].UCoord, t));

                }
            }

            for (int ring = 0; ring < (ringCount-1f); ring++){
                int rootIndex = ring * mesh2D.vertexCount;
                int rootIndexNext = (ring+1) * mesh2D.vertexCount;

                for (int line = 0; line < mesh2D.lineCount; line++){
                    int lineIndexA = mesh2D.segmentIndices[line].vert1;
                    int lineIndexB = mesh2D.segmentIndices[line].vert2;

                    int currentA = rootIndex + lineIndexA;
                    int currentB = rootIndex + lineIndexB;
                    int nextA = rootIndexNext + lineIndexA;
                    int nextB = rootIndexNext + lineIndexB;

                    triangles.Add(currentA);
                    triangles.Add(nextA);
                    triangles.Add(nextB);
                    triangles.Add(currentA);
                    triangles.Add(nextB);
                    triangles.Add(currentB);
                }

            }

            triangles.Add(0);
            triangles.Add(6);
            triangles.Add(5);

            mesh.SetVertices(vertices);
            mesh.SetNormals(normals);
            mesh.SetUVs(0, uvs);

            mesh.SetTriangles(triangles, 0);

            meshFilter.sharedMesh = mesh;
            meshCollider.sharedMesh = mesh;
        }

        public void UpdateOtherSegments(){
            UpdateMesh();
            UpdateNextSegment();
            UpdatePreviousSegment();
        }

        public void UpdateNextSegment() {
            if (nextSpline == null) return;
            
            Segment thisSegment = this.segment;
            Segment nextSegment = nextSpline.segment;

            if (nextSegment.GetType() == thisSegment.GetType()) {
                thisSegment.UpdateNextSegment(nextSegment);
            }

            nextSpline.UpdateMesh();

        }

        public void UpdatePreviousSegment() {
            if (previousSpline == null) return;

            Segment thisSegment = this.segment;
            Segment previousSegment = previousSpline.segment;

            if (previousSegment.GetType() == thisSegment.GetType()) {
                thisSegment.UpdatePreviousSegment(previousSegment);
            }

            previousSpline.UpdateMesh();
            
        }


        public void AddNext(){
            Vector3 displacement = segment.controlPoint2.position - segment.controlPoint1.position;
            nextSpline = Instantiate(gameObject, transform.position + displacement, transform.rotation, transform.parent).GetComponent<Spline>();
            nextSpline.previousSpline = this;

            nextSpline.gameObject.name = "Segment";

            UpdateOtherSegments();
        }
        public void RemoveNext() {
            if (nextSpline.nextSpline != null)
                nextSpline.nextSpline.previousSpline = null;
            GameUtility.SafeDestroy(nextSpline.gameObject);
            nextSpline = null;
        }


        public void AddPrev(){
            Vector3 displacement = segment.controlPoint1.position - segment.controlPoint2.position;
            previousSpline = Instantiate(gameObject, transform.position + displacement, transform.rotation, transform.parent).GetComponent<Spline>();
            previousSpline.nextSpline = this;

            previousSpline.gameObject.name = "Segment";
            
            UpdateOtherSegments();
        }
        public void RemovePrev(){
            if (previousSpline.previousSpline != null)
                previousSpline.previousSpline.nextSpline = null;
            GameUtility.SafeDestroy(previousSpline.gameObject);
            previousSpline = null;
        }



        private void Reset(){
            _linearSegment = new LineSegment();
            _bezierQuadratic = new BezierQuadratic();
            _bezierCubic = new BezierCubic();
            UpdateOtherSegments();
        }



        public enum SegmentType {
            Linear,
            BezierCubic,
            BezierQuadratic
        }

    }
}