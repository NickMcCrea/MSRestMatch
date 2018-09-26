using UnityEngine;

public class AnimateSkinnedMeshUvPosition : MonoBehaviour
{
    public Vector2 TextureMoveSpeed;
    private Mesh _mesh;
    private Vector2[] _uvs;

    private void Start()
    {
        _mesh = GetComponent<SkinnedMeshRenderer>().sharedMesh;
    }

    private void Update()
    {
        _uvs = _mesh.uv;
        for (int i = 0; i < _uvs.Length; i++)
        {
            _uvs[i] = new Vector2(_uvs[i].x + (TextureMoveSpeed.x * Time.deltaTime), _uvs[i].y + (TextureMoveSpeed.y * Time.deltaTime));
        }
        _mesh.uv = _uvs;
    }
}
