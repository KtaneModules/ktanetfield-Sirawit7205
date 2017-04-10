using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;

public class TextField : MonoBehaviour
{
    public KMAudio Audio;
    public KMSelectable[] Buttons;
    public TextMesh[] ButtonLabels;
    public KMBombInfo BombInfo;

    private bool _hasvowels = false, _lightson = false;
    private int _answerCount = 0;

    private Dictionary<string, int> _portCounts = new Dictionary<string, int> { { "Serial", 0 }, { "DVI", 0 }, { "Parallel", 0 }, { "PS2", 0 }, { "StereoRCA", 0 }, { "RJ45", 0 } };
    private HashSet<string> _litIndicators = new HashSet<string>();
    private int _numBatteries;
    private bool[] _mustPressButton = new bool[12], _pressedButton = new bool[12];

    private char[,] _solutionTable = new char[8, 12] {
        { 'D', 'C', 'F', 'A', 'B', 'E', 'F', 'F', 'B', 'B', 'B', 'C' },
        { 'C', 'B', 'E', 'F', 'E', 'B', 'F', 'E', 'D', 'C', 'A', 'A' },
        { 'B', 'A', 'B', 'B', 'C', 'D', 'F', 'D', 'D', 'F', 'C', 'E' },
        { 'D', 'A', 'B', 'F', 'D', 'F', 'B', 'E', 'C', 'E', 'B', 'A' },
        { 'C', 'B', 'D', 'E', 'A', 'F', 'D', 'C', 'B', 'E', 'B', 'D' },
        { 'A', 'D', 'C', 'B', 'A', 'C', 'B', 'C', 'A', 'E', 'F', 'A' },
        { 'E', 'C', 'F', 'A', 'C', 'F', 'B', 'D', 'F', 'F', 'B', 'C' },
        { 'B', 'E', 'A', 'B', 'E', 'D', 'F', 'A', 'B', 'C', 'E', 'C' }
    };

    private static int _moduleIdCounter = 1;
    private int _moduleId;

    void Start()
    {
        _moduleId = _moduleIdCounter++;
        GetComponent<KMBombModule>().OnActivate += Init;
    }

    void Awake()
    {
        Buttons[0].OnInteract += delegate ()
        {
            HandlePress(0);
            return false;
        };
        Buttons[1].OnInteract += delegate ()
        {
            HandlePress(1);
            return false;
        };
        Buttons[2].OnInteract += delegate ()
        {
            HandlePress(2);
            return false;
        };
        Buttons[3].OnInteract += delegate ()
        {
            HandlePress(3);
            return false;
        };
        Buttons[4].OnInteract += delegate ()
        {
            HandlePress(4);
            return false;
        };
        Buttons[5].OnInteract += delegate ()
        {
            HandlePress(5);
            return false;
        };
        Buttons[6].OnInteract += delegate ()
        {
            HandlePress(6);
            return false;
        };
        Buttons[7].OnInteract += delegate ()
        {
            HandlePress(7);
            return false;
        };
        Buttons[8].OnInteract += delegate ()
        {
            HandlePress(8);
            return false;
        };
        Buttons[9].OnInteract += delegate ()
        {
            HandlePress(9);
            return false;
        };
        Buttons[10].OnInteract += delegate ()
        {
            HandlePress(10);
            return false;
        };
        Buttons[11].OnInteract += delegate ()
        {
            HandlePress(11);
            return false;
        };
    }

    void Init()
    {
        int letter = Random.Range(0, 6) + 65;
        for (int i = 0; i < 12; i++)
            ButtonLabels[i].text = char.ConvertFromUtf32(letter);

        foreach (string portInfo in BombInfo.QueryWidgets(KMBombInfo.QUERYKEY_GET_PORTS, null))
            foreach (string portName in JsonConvert.DeserializeObject<Dictionary<string, string[]>>(portInfo)["presentPorts"])
                _portCounts[portName] = _portCounts[portName] + 1;

        foreach (string batteryInfo in BombInfo.QueryWidgets(KMBombInfo.QUERYKEY_GET_BATTERIES, null))
            _numBatteries += JsonConvert.DeserializeObject<Dictionary<string, int>>(batteryInfo)["numbatteries"];

        foreach (string indicatorInfo in BombInfo.QueryWidgets(KMBombInfo.QUERYKEY_GET_INDICATOR, null))
        {
            var responseDict = JsonConvert.DeserializeObject<Dictionary<string, string>>(indicatorInfo);
            if (responseDict["on"].Equals("True"))
                _litIndicators.Add(responseDict["label"]);
        }

        string serialNumber = null;
        foreach (string serialNumberInfo in BombInfo.QueryWidgets(KMBombInfo.QUERYKEY_GET_SERIAL_NUMBER, null))
            serialNumber = JsonConvert.DeserializeObject<Dictionary<string, string>>(serialNumberInfo)["serial"];

        _hasvowels = serialNumber.Any("AEIOU".Contains);
        var answer = generateAns(serialNumber, letter - 65);
        Debug.LogFormat("[Text Field #{1}] Letter is {0}.", (char) letter, _moduleId);
        for (int i = 0; i < 12; i++)
        {
            if (_solutionTable[answer, i] == letter)
            {
                _answerCount++;
                _mustPressButton[i] = true;
                Debug.LogFormat("[Text Field #{2}] Must press button at ({0}, {1}).", (i % 4) + 1, (i / 4) + 1, _moduleId);
            }
        }
        _lightson = true;
    }

    int generateAns(string serialNumber, int ruleset)
    {
        if (ruleset == 0)
        {
            if (_litIndicators.Contains("CLR")) return 2;
            else if (_numBatteries > 2) return 3;
            else if (_numBatteries == 1) return 5;
            else if (_litIndicators.Contains("FRK")) return 4;
            else return 6;
        }
        else if (ruleset == 1)
        {
            if (_numBatteries == 0) return 1;
            else if ((serialNumber[5] - 48) % 2 == 1) return 2;
            else if (_portCounts["Serial"] == 0) return 4;
            else if (_litIndicators.Contains("TRN")) return 6;
            else return 5;
        }
        else if (ruleset == 2)
        {
            if (_portCounts["DVI"] != 0) return 7;
            else if (_numBatteries == 2) return 0;
            else if (!_hasvowels) return 4;
            else if (_litIndicators.Contains("CAR")) return 2;
            else return 5;
        }
        else if (ruleset == 3)
        {
            if (_portCounts["Parallel"] != 0) return 0;
            else if (_numBatteries < 2) return 7;
            else if (_litIndicators.Contains("SIG")) return 3;
            else if (_portCounts["PS2"] == 0) return 1;
            else return 2;
        }
        else if (ruleset == 4)
        {
            if (_numBatteries < 3) return 5;
            else if (_portCounts["StereoRCA"] == 0) return 7;
            else if (_litIndicators.Contains("BOB")) return 6;
            else if (_portCounts["RJ45"] != 0) return 3;
            else return 4;
        }
        else
        {
            if (_portCounts["Serial"] == 0) return 4;
            else if (_hasvowels) return 6;
            else if (_litIndicators.Contains("IND")) return 2;
            else if ((serialNumber[5] - 48) % 2 == 0) return 0;
            else return 7;
        }
    }

    void HandlePress(int btnIx)
    {
        Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, Buttons[btnIx].transform);
        Buttons[btnIx].AddInteractionPunch();
        Debug.LogFormat("[Text Field #{2}] Button at ({0}, {1}) was pressed.", btnIx % 4 + 1, btnIx / 4 + 1, _moduleId);
        _pressedButton[btnIx] = true;
        if (_mustPressButton[btnIx])
        {
            _answerCount--;
            Debug.LogFormat("[Text Field #{1}] One down, {0} left.", _answerCount, _moduleId);
            _mustPressButton[btnIx] = false;
            ButtonLabels[btnIx].text = "✓";
        }
        else
        {
            Debug.LogFormat("[Text Field #{0}] Incorrect or repeat!", _moduleId);
            GetComponent<KMBombModule>().HandleStrike();
            ButtonLabels[btnIx].text = "✗";
        }
        if (_answerCount == 0 && _lightson)
        {
            Debug.LogFormat("[Text Field #{0}] Module disarmed.", _moduleId);
            GetComponent<KMBombModule>().HandlePass();
        }
    }

    KMSelectable[] ProcessTwitchCommand(string command)
    {
        int pos;
        var btn = new List<KMSelectable>();

        command = command.ToLowerInvariant().Trim();

        if(command.StartsWith("press "))
        {
            command = command.Substring(6);

            foreach (var cell in command.Trim().Split(new[] { ' ' }, System.StringSplitOptions.RemoveEmptyEntries))
            {
                if (Regex.IsMatch(cell, @"^[1-4],[1-3]$"))
                {
                    pos = (4 * int.Parse(cell[2].ToString())) + int.Parse(cell[0].ToString()) - 5;
                    if (_pressedButton[pos] == false)
                    {
                        _pressedButton[pos] = true;
                        btn.Add(Buttons[pos]);
                    }
                }
            }

            if (btn.Count > 0) return btn.ToArray();
        }

        return null;
    }
}
