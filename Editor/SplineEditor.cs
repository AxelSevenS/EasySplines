using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace EasySplines.Editor {

    [CanEditMultipleObjects]
    [CustomEditor(typeof(Spline))]
    public class SplineEditor : UnityEditor.Editor {

        private static Dictionary<Spline, Vector3> oldPositions = new Dictionary<Spline, Vector3>();

        private Spline targetSpline;
        private SerializedObject so;
        private SerializedProperty propSegmentType;

        private SerializedProperty propLinearSegment;
        private SerializedProperty propBezierCubic;
        private SerializedProperty propBezierQuadratic;

        private SerializedProperty propLength;
        private SerializedProperty propArcLengths;

        private SerializedProperty propStoppingPointCheck;
        private SerializedProperty propStoppingPoint;
        
        private SerializedProperty propNextSegment;
        private SerializedProperty propPrevSegment;
        
        private SerializedProperty propMesh2D;
        private SerializedProperty propCount;
        private SerializedProperty propScale;

        private int selectedSegment = 0;



        private Spline.SegmentType segmentType => targetSpline.segmentType;
        private SerializedProperty propSegment {
            get {
                switch (segmentType) {
                    default:
                        return propLinearSegment;
                    case Spline.SegmentType.BezierCubic:
                        return propBezierCubic;
                    case Spline.SegmentType.BezierQuadratic:
                        return propBezierQuadratic;
                }
            }
        }



        private void OnUndoRedo(){
            targetSpline.UpdateMesh();
        }
        
        public override void OnInspectorGUI(){

            Spline targetSpline = (Spline)target;


		    EditorGUI.BeginChangeCheck();

            EditorGUILayout.PropertyField( propSegmentType, new GUIContent("Segment Type") );

            if (EditorGUI.EndChangeCheck()) {
                targetSpline.segmentType = (Spline.SegmentType)propSegmentType.enumValueIndex;

                Undo.RecordObject(targetSpline, "Change Segment Type");
                targetSpline.UpdateMesh();
            }

		    EditorGUI.BeginChangeCheck();

            EditorGUILayout.PropertyField( propSegment, new GUIContent("Segment") );

            // Don't display if multiple objects are selected
            if (targets.Length == 1) {

                GUILayout.Space( 15 );

                GUILayout.Label( "Spline Segments", EditorStyles.boldLabel );
                using ( new GUILayout.HorizontalScope( EditorStyles.helpBox ) ){
                    EditorGUIUtility.labelWidth = 150;
                    using ( new GUILayout.VerticalScope( EditorStyles.label ) ){
                        GUILayout.Label( "Previous Segment", EditorStyles.boldLabel );
                        EditorGUILayout.PropertyField( propPrevSegment, GUIContent.none );

                        GUILayout.Space( 5 );

                        if (targetSpline.previousSpline == null){
                            if (GUILayout.Button( "Add Previous Segment" )) targetSpline.AddPrev();
                        }else{
                            if (GUILayout.Button( "Remove Previous Segment" )) targetSpline.RemovePrev();
                            if (GUILayout.Button( "Link Previous Segment" )) targetSpline.UpdatePreviousSegment();
                        }
                    }
                    using ( new GUILayout.VerticalScope( EditorStyles.label ) ){
                        GUILayout.Label( "Next Segment", EditorStyles.boldLabel );
                        EditorGUILayout.PropertyField( propNextSegment, GUIContent.none );

                        GUILayout.Space( 5 );

                        if (targetSpline.nextSpline == null){
                            if (GUILayout.Button( "Add Next Segment" )) targetSpline.AddNext();
                        }else {
                            if (GUILayout.Button( "Remove Next Segment" )) targetSpline.RemoveNext();
                            if (GUILayout.Button( "Link Next Segment" )) targetSpline.UpdateNextSegment();
                        }
                    }
                }

            }

            
            
            GUILayout.Space( 15 );
            
            EditorGUILayout.PropertyField( propStoppingPointCheck );
            if ( propStoppingPointCheck.boolValue ) {
                EditorGUILayout.PropertyField( propStoppingPoint );
            }

            GUILayout.Space( 15 );

            GUILayout.Label( "Procedural Mesh", EditorStyles.boldLabel );
            using ( new GUILayout.VerticalScope( EditorStyles.helpBox ) ){
                EditorGUILayout.PropertyField( propMesh2D );

                if (targetSpline.mesh2D != null){

                    EditorGUILayout.PropertyField( propCount, new GUIContent("Repeat Count") );
                    EditorGUILayout.PropertyField( propScale, new GUIContent("Scale of Mesh") );
                }
            }

            if ( EditorGUI.EndChangeCheck() ){

                Undo.RecordObject( target, "Edited Spline Values" ); 
                so.ApplyModifiedProperties();

                targetSpline.UpdateMesh();

            }
        }

        public void OnSceneGUI(){

            Spline targetSpline = (Spline)target;

            ControlPoint controlPoint1 = targetSpline.segment.controlPoint1;
            Vector3 tangent1 = targetSpline.GetTangent(0f);
            if ( DrawControlPointGUI( ref controlPoint1, tangent1, 0.25f, Color.red, targetSpline.GetInstanceID() ) ) {
                Undo.RecordObject(targetSpline, "Edited Spline");
                targetSpline.segment.controlPoint1.Set(controlPoint1);
                targetSpline.UpdateMesh();
            }

            ControlPoint controlPoint2 = targetSpline.segment.controlPoint2;
            Vector3 tangent2 = targetSpline.GetTangent(1f);
            if ( DrawControlPointGUI( ref controlPoint2, tangent2, 0.25f, Color.red, targetSpline.GetInstanceID() + 1 ) ) {
                Undo.RecordObject(targetSpline, "Edited Spline");
                targetSpline.segment.controlPoint2.Set(controlPoint2);
                targetSpline.UpdateMesh();
            }

            // Vector3 centerPosition = (controlPoint1.position + controlPoint2.position) / 2f;

            if (targetSpline.segment is BezierCubic bezierCubic) {

                Vector3 handle1 = bezierCubic.handle1;
                if ( DrawHandleGUI( ref handle1, 0.2f, Color.blue, targetSpline.GetInstanceID() + 2 ) ) {
                    Undo.RecordObject(targetSpline, "Edited Cubic Spline");
                    bezierCubic.handle1 = handle1;
                    targetSpline.UpdateMesh();
                }

                Vector3 handle2 = bezierCubic.handle2;
                if ( DrawHandleGUI( ref handle2, 0.2f, Color.blue, targetSpline.GetInstanceID() + 3 ) ) {
                    Undo.RecordObject(targetSpline, "Edited Cubic Spline");
                    bezierCubic.handle2 = handle2;
                    targetSpline.UpdateMesh();
                }

                // centerPosition = (centerPosition + handle1 + handle2) / 3f;

            }

            if (targetSpline.segment is BezierQuadratic bezierQuadratic) {

                Vector3 handle = bezierQuadratic.handle;
                if ( DrawHandleGUI( ref handle, 0.2f, Color.blue, targetSpline.GetInstanceID() + 4 ) ) {
                    Undo.RecordObject(targetSpline, "Edited Quadratic Spline");
                    bezierQuadratic.handle = handle;
                    targetSpline.UpdateMesh();
                }

                // centerPosition = (centerPosition + handle) / 2f;

            }
            
            // EditorGUI.BeginChangeCheck();

            // Vector3 newCenterPosition = Handles.PositionHandle(centerPosition, Quaternion.identity);

            // if (EditorGUI.EndChangeCheck()) {
            //     Undo.RecordObject(targetSpline, "Edited Spline");
            //     targetSpline.segment.Move( newCenterPosition - centerPosition );
            //     targetSpline.UpdateMesh();
            // }

            if ( targetSpline.transform.hasChanged ){
                targetSpline.transform.hasChanged = false; 

                Vector3 movement = targetSpline.transform.position - oldPositions[targetSpline];
                oldPositions[targetSpline] = targetSpline.transform.position;
                
                targetSpline.segment.Move( movement );
                targetSpline.UpdateMesh();
            }

        }

        [DrawGizmo(GizmoType.InSelectionHierarchy | GizmoType.Active)]
        private static void OnDrawGizmosSelected(Spline scr, GizmoType gizmoType) {

            Segment segment = scr.segment;

            if (segment is LineSegment lineSegment) {
                Gizmos.DrawLine( segment.controlPoint1.position, segment.controlPoint2.position );
            }
            if (segment is BezierCubic bezierCubic) {
                Handles.DrawBezier( segment.controlPoint1.position, segment.controlPoint2.position, bezierCubic.handle1, bezierCubic.handle2, Color.white, EditorGUIUtility.whiteTexture, 1f );
                Gizmos.DrawLine( segment.controlPoint1.position, bezierCubic.handle1 );
                Gizmos.DrawLine( segment.controlPoint2.position, bezierCubic.handle2 );
            }
            if (segment is BezierQuadratic bezierQuadratic) {
                // draw a Quadratic Bezier
                const int steps = 15;
                for (int i = 0; i < steps; i++)
                    Gizmos.DrawLine( segment.GetPoint( (float)i/(float)steps ).position, segment.GetPoint( (float)(i+1)/(float)steps ).position );
                    
                Gizmos.DrawLine( segment.controlPoint1.position, bezierQuadratic.handle );
                Gizmos.DrawLine( segment.controlPoint2.position, bezierQuadratic.handle );
            }

            for (int i = 0; i < scr.ringCount; i++){
                OrientedPoint pointAlongTessel = scr.GetPointUniform( (float)i/((float)scr.ringCount - 1) );

                if (scr.mesh2D == null) continue;
                Gizmos.color = Color.red;

                for (int j = 0; j < scr.mesh2D.vertices.Length-1; j++)
                    Gizmos.DrawLine(pointAlongTessel.position + (pointAlongTessel.rotation * scr.mesh2D.vertices[j].point)*scr.scale, pointAlongTessel.position + (pointAlongTessel.rotation * scr.mesh2D.vertices[j+1].point)*scr.scale);
                Gizmos.DrawLine(pointAlongTessel.position + (pointAlongTessel.rotation * scr.mesh2D.vertices[scr.mesh2D.vertices.Length-1].point)*scr.scale, pointAlongTessel.position + (pointAlongTessel.rotation * scr.mesh2D.vertices[0].point)*scr.scale);
                
            }
        }

        


        private bool DrawControlPointGUI( ref ControlPoint point, Vector3 tangent, float size, Color color, int id ) {

            float handleSize = HandleUtility.GetHandleSize(point.position);
            float moveHandleSize = handleSize * 0.2f;
            Handles.color = color;


            EditorGUI.BeginChangeCheck();


            point.position = Handles.FreeMoveHandle(id, point.position, Quaternion.identity, moveHandleSize, Vector3.zero, Handles.SphereHandleCap);

            if (GUIUtility.hotControl == id || selectedSegment == id) {
                point.position = Handles.PositionHandle(point.position, Quaternion.identity);
                selectedSegment = id;
            }


            Quaternion cpRotation = Quaternion.LookRotation(tangent, Quaternion.AngleAxis(point.upAngle, tangent) * Vector3.up);

            Quaternion newRotation = Handles.Disc(cpRotation, point.position, tangent, handleSize * 0.5f, false, 0f);
            Handles.DrawLine(point.position, point.position + (newRotation * Vector3.up * handleSize * 0.5f));
            point.upAngle = Mathf.Abs(newRotation.eulerAngles.z);


            return EditorGUI.EndChangeCheck();
            
        }

        private bool DrawHandleGUI( ref Vector3 point, float size, Color color, int id ) {

            float handleSize = HandleUtility.GetHandleSize(point) * 0.2f;
            Handles.color = color;


            EditorGUI.BeginChangeCheck();


            point = Handles.FreeMoveHandle(id, point, Quaternion.identity, handleSize, Vector3.zero, Handles.SphereHandleCap);

            if (GUIUtility.hotControl == id || selectedSegment == id) {
                point = Handles.PositionHandle(point, Quaternion.identity);
                selectedSegment = id;
            }
            

            return EditorGUI.EndChangeCheck();
            
        }




        
        private void OnEnable(){

            so = serializedObject;
            propSegmentType = so.FindProperty( "segmentType" );

            propLinearSegment = so.FindProperty( "_linearSegment" );
            propBezierCubic = so.FindProperty( "_bezierCubic" );
            propBezierQuadratic = so.FindProperty( "_bezierQuadratic" );

            propStoppingPointCheck = so.FindProperty( "hasStoppingPoint" );
            propStoppingPoint = so.FindProperty( "stoppingPoint" );

            propNextSegment = so.FindProperty( "nextSpline" );
            propPrevSegment = so.FindProperty( "previousSpline" );

            propMesh2D = so.FindProperty( "mesh2D" );
            propCount = so.FindProperty( "ringCount" );
            propScale = so.FindProperty( "scale" );

            targetSpline = (Spline)target;

            // We have to do this for multi-object editing, otherwise the oldPosition will not be set on the non-first selected objects
            foreach (var item in targets) {
                if (item is Spline spline) {
                    oldPositions[spline] = spline.transform.position;

                    Undo.undoRedoPerformed -= OnUndoRedo;
                    Undo.undoRedoPerformed += OnUndoRedo;
                }
            }
        }

        private void OnDisable(){
            Undo.undoRedoPerformed -= OnUndoRedo;
            Debug.Log(targetSpline.name + " disabled");
        }

    }
}