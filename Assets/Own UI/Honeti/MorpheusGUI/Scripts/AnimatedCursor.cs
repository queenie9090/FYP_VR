using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimatedCursor : MonoBehaviour
{
    [SerializeField] private Texture2D[] _cursorTexturesArray;
    [SerializeField] private float _frameRate;
    [SerializeField] private int _heatPointX = 0;
    [SerializeField] private int _heatPointY = 0;

    private int _currentFrame;
    private float _frameTimer;

    bool animate = false;

    public void ChangeCursor()
    {
        Cursor.SetCursor(_cursorTexturesArray[0], new Vector2(_heatPointX, _heatPointY), CursorMode.Auto);
        animate = true;
    }
    public void ResetCursor()
    {
        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
        animate = false;
    }

    void Update()
    {
        if (!animate) return;

        _frameTimer -= Time.deltaTime;
        if (_frameTimer <= 0f)
        {
            _frameTimer += _frameRate;
            _currentFrame = (_currentFrame + 1) % _cursorTexturesArray.Length;
            Cursor.SetCursor(_cursorTexturesArray[_currentFrame], new Vector2(_heatPointX, _heatPointY), CursorMode.Auto);
        }
    }
}
