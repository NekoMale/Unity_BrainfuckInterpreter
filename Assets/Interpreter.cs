using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class Interpreter : MonoBehaviour
{
    [SerializeField] int _bufferSize = 30;
    [SerializeField] float _delaySeconds = 0.5f;
    [SerializeField] TMP_InputField _input = null;
    [SerializeField] TMP_Text _output = null;
    [SerializeField] RectTransform _memoryCellsContainer = null;
    [SerializeField] RectTransform _memoryCellPrefab = null;
    [SerializeField] RectTransform _head = null;
    [SerializeField] Vector2 _headOffset = new Vector2(0, -28.3f);

    [SerializeField] int[] _buffer = new int[0];
    TMP_Text[] _memoryCells = new TMP_Text[0];

    public UnityEvent OnStartDeconding = new UnityEvent();
    public UnityEvent OnEndDecoding = new UnityEvent();

    private void Awake() {
        _memoryCells = new TMP_Text[_bufferSize];
        for (int i = 0; i < _bufferSize; i++) {
            _memoryCells[i] = Instantiate(_memoryCellPrefab, _memoryCellsContainer).GetComponentInChildren<TMP_Text>();
            _memoryCells[i].text = "0";
        }
    }

    private void Start() {
        StartCoroutine(SetStartHeadPosition());
    }

    private IEnumerator SetStartHeadPosition() {
        yield return new WaitForSeconds(0.1f);
        SetHeadPosition(0);
    }

    public void StartDecode() {
        StartCoroutine(Decode());
    }

    private IEnumerator Decode() {
        OnStartDeconding.Invoke();
        string code = _input.text;
        for (int i = 0; i < _bufferSize; i++) {
            _memoryCells[i].text = "0";
        }
        _buffer = new int[_bufferSize];
        int codeLength = code.Length;
        int pos = 0;
        SetHeadPosition(pos);
        _output.text = "Output: ";
        for (int i = 0; i < codeLength; i++) {
            if (_delaySeconds > 0f) yield return new WaitForSecondsRealtime(_delaySeconds);
            char sign = code[i];
            switch (sign) {
                case '>':
                    pos++;
                    if (!SetHeadPosition(pos)) yield break;
                    break;
                case '<':
                    pos--;
                    if (!SetHeadPosition(pos)) yield break;
                    break;
                case '+':
                    _buffer[pos]++;
                    _buffer[pos] %= 256;
                    ChangeMemoryCell(pos);
                    break;
                case '-':
                    _buffer[pos]--;
                    if (_buffer[pos] < 0) _buffer[pos] += 256;
                    ChangeMemoryCell(pos);
                    break;
                case '.':
                    _output.text += (char)_buffer[pos];
                    break;
                case '[':
                    if (_buffer[pos] == 0) {
                        int nRB = 0;
                        i++;
                        while (code[i] != ']' || nRB > 0) {
                            if (code[i] == '[') nRB++;
                            if (code[i] == ']') nRB--;
                            i++;
                        }
                    }
                    break;
                case ']':
                    if (_buffer[pos] > 0) {
                        int nLB = 0; 
                        i--;
                        while (code[i] != '[' || nLB > 0) {
                            if (code[i] == ']') nLB++;
                            if (code[i] == '[') nLB--;
                            i--;
                        }
                    }
                    break;
            }
        }
        OnEndDecoding.Invoke();
    }

    private void ChangeMemoryCell(int pos)
    {
        _memoryCells[pos].text = _buffer[pos].ToString();
    }

    private bool SetHeadPosition(int pos)
    {
        _head.position = _memoryCells[pos].transform.parent.position;
        _head.anchoredPosition += _headOffset;
        if (pos >= _bufferSize)
        {
            ErrorHandle($"Buffer max size ({_bufferSize}) overflow", pos);
            return false;
        }
        else if (pos < 0)
        {
            ErrorHandle($"Buffer min size underflow", pos);
            return false;
        }
        return true;
    }

    private void ErrorHandle(string message, int index) {
        message += $" ({index})";
        OnEndDecoding.Invoke();
        Debug.LogError(message);
    }

    public void StopDecoding()
    {
        StopAllCoroutines();
        OnEndDecoding.Invoke();
    }
}