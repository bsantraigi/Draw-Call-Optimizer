using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DrawCallOptimizer : MonoBehaviour {
    public GameObject EmptyCombinedPrefab;
    delegate T Action<T>(T item);

    void Start()
    {
        Transform[] children = GetComponentsInChildren<Transform>();
        List<Transform> transformsList = new List<Transform>();
        List<Mesh> meshes = new List<Mesh>();

        for(int i = 0; i<children.Length; i++)
        {
            MeshFilter m = children[i].GetComponent<MeshFilter>();
            if(m != null)
            {
                transformsList.Add(children[i]);
                meshes.Add(m.mesh);
            }
        }
        GameObject go = Instantiate(EmptyCombinedPrefab, Vector3.zero, Quaternion.identity) as GameObject;
        MeshFilter mf = go.GetComponent<MeshFilter>();
        Mesh combinedMesh = mergeMeshes(ref mf, meshes, transformsList);

        for (int i = 0; i < transformsList.Count; i++)
        {
            transformsList[i].gameObject.SetActive(false);
        }
    }

    Mesh mergeMeshes(ref MeshFilter mf, List<Mesh> meshes, List<Transform> childrens)
    {
        Mesh combinedMesh = new Mesh();

        int vertCount = 0;
        int triCount = 0;

        for (int i = 0; i < meshes.Count; i++)
        {
            Mesh m = meshes[i];
            vertCount += m.vertexCount;
            triCount += m.triangles.Length;
        }

        Vector3[] vertices = new Vector3[vertCount];
        int[] triangles = new int[triCount];
        Vector2[] uv = new Vector2[vertCount];

        int vertCurrentSize = 0;
        int triCurrentSize = 0;
        for (int i = 0; i < meshes.Count; i++)
        {
            Mesh m = meshes[i];

            Transform t = childrens[i];

            Copy<Vector3>(ref vertices, m.vertices, vertCurrentSize, m.vertexCount, 
                delegate(Vector3 v)
                {
                    return t.TransformPoint(v);
                }
                );
            Copy<int>(ref triangles, m.triangles, triCurrentSize, m.triangles.Length, 
                delegate(int k)
                {
                    return k + vertCurrentSize;
                }
                );

            vertCurrentSize += m.vertexCount;
            triCurrentSize += m.triangles.Length;

        }

        combinedMesh.vertices = vertices;
        combinedMesh.triangles = triangles;
        combinedMesh.RecalculateNormals();

        mf.mesh = combinedMesh;

        return combinedMesh;
    }

    static void Copy<T>(ref T[] dest, T[] source, int destStart, int size, Action<T> action = null)
    {
        if(dest == null || source == null)
        {
            Debug.LogError("Null ref error");
            return;
        }
        int j = 0;
        if (action == null)
        {
            for (int i = destStart; i < destStart + size; i++)
            {
                dest[i] = source[j++];
            }
        }
        else {
            for (int i = destStart; i < destStart + size; i++)
            {
                dest[i] = action(source[j++]);
            }
        }
    }
}
