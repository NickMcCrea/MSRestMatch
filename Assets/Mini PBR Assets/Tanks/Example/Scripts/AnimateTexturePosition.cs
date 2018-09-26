using UnityEngine;

public class AnimateTexturePosition : MonoBehaviour
{
    public int MaterialIndex = 0;
    public Vector2 TextureMoveSpeed;
    private Material _material;
    private Vector2 _currentPosition;

    private void Start()
    {
        _material = GetComponent<Renderer>().materials[MaterialIndex];
    }

    private void Update()
    {
        var textureMoveDistance = TextureMoveSpeed * Time.deltaTime;
        _currentPosition += textureMoveDistance;
        _material.SetTextureOffset("_MainTex", _currentPosition);
    }
}
