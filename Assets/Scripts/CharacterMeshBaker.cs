using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class CharacterMeshBaker : MonoBehaviour
{
    [SerializeField] private SkinnedMeshRenderer[] meshRenderers;
    private readonly List<Vector3> _vertices = new List<Vector3>();
    private Mesh _mesh;

    [SerializeField] private VisualEffect visualEffect;
    [SerializeField] private MeshFilter test;
    

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.B))
        {
            BakeMesh();
        }
    }

    private void BakeMesh()
    {
        _mesh = new Mesh();
        // foreach (var mesh in meshRenderers)
        // {
        //     var m = new Mesh();
        //     mesh.BakeMesh(m);
        //     _vertices.AddRange(m.vertices);
        // }
        // _mesh.vertices = _vertices.ToArray();

        var combine = new CombineInstance[meshRenderers.Length];
        for (var i = 0; i < meshRenderers.Length; i++)
        {
            var m = new Mesh();
            meshRenderers[i].BakeMesh(m);
            combine[i].mesh = m;
            combine[i].transform = meshRenderers[i].transform.localToWorldMatrix;
        }
        _mesh.CombineMeshes(combine, false);
        test.mesh = _mesh;
        visualEffect.SetMesh("Mesh", _mesh);
        visualEffect.SendEvent("Play");
    }
}
