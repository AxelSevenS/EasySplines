using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EasySplines {
    
    [CreateAssetMenu(fileName = "new 2D Mesh", menuName = "2D Mesh")]
    public class RepeatableMesh : ScriptableObject{
        
        [System.Serializable] public class Vertex{
            public Vector2 point;
            public Vector2 normal;
            public float UCoord;
        }
        [System.Serializable] public class Segment{
            public int vert1;
            public int vert2;
        }
        
        public Vertex[] vertices;
        public Segment[] segmentIndices;


        public int firstUVIndex;

        public int vertexCount => vertices.Length;
        public int lineCount => segmentIndices.Length;

        [ContextMenu("Auto-Calculate UVs")]
        public void CalculateUVs(){
            if (firstUVIndex > vertexCount) firstUVIndex = 0;
            List<Vector2> uniqueVerts = new List<Vector2>{};
            for ( int i = 0; i < vertexCount; i++ ) {
                Vector2 newCoord = vertices[i].point;
                if ( !uniqueVerts.Exists(element => element == newCoord) ) uniqueVerts.Add(newCoord);
            }
            for ( int i = 0; i < vertexCount; i++ ) {
                int affectedIndex = (i + firstUVIndex) % vertexCount;
                int part = uniqueVerts.FindIndex(element => element == vertices[affectedIndex].point);
                float fraction = (float)part / (uniqueVerts.Count - 1);
                Debug.Log(i + " " + fraction);
                vertices[i].UCoord = fraction;
            }

        }
    }
}