using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using UnityEngine;

public class DebugWindow : SingletonMonoBehaviour<DebugWindow>
{
    [SerializeField]
    public bool DebugMode = false;

    private static readonly Vector2 WindowPos = new Vector2(0, 460);
    private static readonly Vector2 WindowSize = new Vector2(780, 620);
    private static readonly Rect WindowRect = new Rect(WindowPos, WindowSize);

    private Rect _scrollViewPos = new Rect(0, 10, 770, 610);
    private Rect _scrollViewRect = new Rect(0, 0, 1920, 0);
    private Rect _logRowRect = new Rect(20, 0, 770, 30);

    private GUIStyle _normalStyle = null;
    private GUIStyle _errorStyle = null;
    private GUIStyle _exceptionStyle = null;

    private Queue<KeyValuePair<LogType, string>> _logQueue = new Queue<KeyValuePair<LogType, string>>();
    private Vector2 _scrollPosition = Vector2.zero;

    // Start is called before the first frame update
    protected override void Awake()
    {
        base.Awake();

        var _normalState = new GUIStyleState() { textColor = new Color(255, 255, 255, 255) };
        _normalStyle = new GUIStyle() { fontSize = 20, normal = _normalState };
        var _errorState = new GUIStyleState() { textColor = new Color(255, 0, 0, 255) };
        _errorStyle = new GUIStyle() { fontSize = 20, normal = _errorState };
        var _exceptionState = new GUIStyleState() { textColor = new Color(0, 0, 255, 255) };
        _exceptionStyle = new GUIStyle() { fontSize = 20, normal = _exceptionState };

        Application.logMessageReceived += ReceivedLogMessage;
    }

    void OnGUI()
    {
        if (!DebugMode) return;

        GUI.Window(0, WindowRect, CreateWindow, "Log");
    }

    public void Add(LogType logType, string msg)
    {
        if (!DebugMode) return;

        _logQueue.Enqueue(new KeyValuePair<LogType, string>(logType, msg));
    }

    private void CreateWindow(int id)
    {
        if (!DebugMode) return;

        var logQueue = new Queue<KeyValuePair<LogType, string>>(_logQueue);

        // 行数に応じてスクロール領域を増やす
        _scrollViewRect.height = 20 + logQueue.Count * 30;
        _scrollPosition = GUI.BeginScrollView(_scrollViewPos, _scrollPosition, _scrollViewRect);

        var row = 0;
        while (logQueue.Count > 0)
        {
            var log = logQueue.Dequeue();
            _logRowRect.y = 20 + row * 30;
            switch (log.Key)
            {
                case LogType.Log:
                    GUI.Label(_logRowRect, log.Value, _normalStyle);
                    break;
                case LogType.Error:
                    GUI.Label(_logRowRect, log.Value, _errorStyle);
                    break;
                case LogType.Exception:
                    GUI.Label(_logRowRect, log.Value, _exceptionStyle);
                    break;
            }
            row++;
        }

        GUI.EndScrollView();
    }

    private void ReceivedLogMessage(string msg, string stackTrace, LogType logType)
    {
        if (!DebugMode) return;

        if (logType == LogType.Log || logType == LogType.Error || logType == LogType.Exception)
        {
            Add(logType, msg);
        }

        // スクロールバー調整
        if (_logQueue.Count > 19)
        {
            _scrollPosition = new Vector2(_scrollPosition.x, _scrollPosition.y + 30);
        }
    }
}
