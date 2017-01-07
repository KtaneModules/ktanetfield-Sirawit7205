using UnityEngine;
using System.Collections.Generic;
using Newtonsoft.Json;

public class tfield : MonoBehaviour {

    public KMAudio Audio;
    public KMSelectable[] buttons;
    public TextMesh[] info;
    public int[] port, indc, batt, keys, verif;
    public string serl;
    public bool _hasvowels = false, _lightson = false;
    public int select, anscnt = 0;

    public int[,] truth = new int[8, 12] {
        {4,3,6,1,2,5,6,6,2,2,2,3},
        {3,2,5,6,5,2,6,5,4,3,1,1},
        {2,1,2,2,3,4,6,4,4,6,3,5},
        {4,1,2,6,4,6,2,5,3,5,2,1},
        {3,2,4,5,1,6,4,3,2,5,2,4},
        {1,4,3,2,1,3,2,3,1,5,6,1},
        {5,3,6,1,3,6,2,4,6,6,2,3},
        {2,5,1,2,5,4,6,1,2,3,5,3}
    };

    void Start () {
        GetComponent<KMBombModule>().OnActivate += Init;
    }

    void Awake()
    {
        buttons[0].OnInteract += delegate ()
        {
            HandlePress(0);
            return false;
        };
        buttons[1].OnInteract += delegate ()
        {
            HandlePress(1);
            return false;
        };
        buttons[2].OnInteract += delegate ()
        {
            HandlePress(2);
            return false;
        };
        buttons[3].OnInteract += delegate ()
        {
            HandlePress(3);
            return false;
        };
        buttons[4].OnInteract += delegate ()
        {
            HandlePress(4);
            return false;
        };
        buttons[5].OnInteract += delegate ()
        {
            HandlePress(5);
            return false;
        };
        buttons[6].OnInteract += delegate ()
        {
            HandlePress(6);
            return false;
        };
        buttons[7].OnInteract += delegate ()
        {
            HandlePress(7);
            return false;
        };
        buttons[8].OnInteract += delegate ()
        {
            HandlePress(8);
            return false;
        };
        buttons[9].OnInteract += delegate ()
        {
            HandlePress(9);
            return false;
        };
        buttons[10].OnInteract += delegate ()
        {
            HandlePress(10);
            return false;
        };
        buttons[11].OnInteract += delegate ()
        {
            HandlePress(11);
            return false;
        };
    } 

    void Init()
    {
        int temp = Random.Range(0, 6) + 65;
        for (int i = 0; i < 12; i++)
        {
            info[i].text = char.ConvertFromUtf32(temp);
        }
        List<string> responsePORT = GetComponent<KMBombInfo>().QueryWidgets(KMBombInfo.QUERYKEY_GET_PORTS, null);
        foreach (string response in responsePORT)
        {
            Debug.Log(response);
            Dictionary<string, string[]> responseDict = JsonConvert.DeserializeObject<Dictionary<string, string[]>>(response);
            foreach (string s in responseDict["presentPorts"])
            {
                if (string.Compare(s, "DVI", true) == 0) port[0]++;
                else if (string.Compare(s, "Parallel", true) == 0) port[1]++;
                else if (string.Compare(s, "PS2", true) == 0) port[2]++;
                else if (string.Compare(s, "RJ45", true) == 0) port[3]++;
                else if (string.Compare(s, "Serial", true) == 0) port[4]++;
                else if (string.Compare(s, "StereoRCA", true) == 0) port[5]++;
            }
        }
        List<string> responseBATT = GetComponent<KMBombInfo>().QueryWidgets(KMBombInfo.QUERYKEY_GET_BATTERIES, null);
        foreach (string response in responseBATT)
        {
            Debug.Log(response);
            Dictionary<string, int> responseDict = JsonConvert.DeserializeObject<Dictionary<string, int>>(response);
            batt[0] += responseDict["numbatteries"];
        }
        List<string> responseINDC = GetComponent<KMBombInfo>().QueryWidgets(KMBombInfo.QUERYKEY_GET_INDICATOR, null);
        foreach (string response in responseINDC)
        {
            Debug.Log(response);
            Dictionary<string, string> responseDict = JsonConvert.DeserializeObject<Dictionary<string, string>>(response);
            string label = responseDict["label"];
            bool active = responseDict["on"].Equals("True");
            if (active)
            {
                if (string.Compare(label, "CLR") == 0) indc[0]++;
                else if (string.Compare(label, "FRK") == 0) indc[1]++;
                else if (string.Compare(label, "BOB") == 0) indc[2]++;
                else if (string.Compare(label, "CAR") == 0) indc[3]++;
                else if (string.Compare(label, "SIG") == 0) indc[4]++;
                else if (string.Compare(label, "TRN") == 0) indc[5]++;
                else if (string.Compare(label, "IND") == 0) indc[6]++;
                indc[7]++;
            }
        }
        List<string> responseSERL = GetComponent<KMBombInfo>().QueryWidgets(KMBombInfo.QUERYKEY_GET_SERIAL_NUMBER, null);
        foreach (string response in responseSERL)
        {
            Debug.Log(response);
            Dictionary<string, string> responseDict = JsonConvert.DeserializeObject<Dictionary<string, string>>(response);
            serl = responseDict["serial"];
        }
        foreach (char c in serl)
        {
            if (c == 'A' || c == 'E' || c == 'I' || c == 'O' || c == 'U')
                _hasvowels = true;
        }
        generateAns(temp-65);
        for(int i=0; i<12; i++)
        {
            int vt = truth[select, i] + 64;
            char ver = (char) vt;
            if (ver.ToString() == info[i].text) { anscnt++; verif[i] = 1; }
        }
        _lightson = true;
        Debug.Log("[Tfield] ready! Answers generated!");
    }

    void generateAns(int ruleset)
    {
        if (ruleset == 0)
        {
            if (indc[0] != 0) select = 2;
            else if (batt[0] > 2) select = 3;
            else if (batt[0] == 1) select = 5;
            else if (indc[1] != 0) select = 4;
            else select = 6;
        }
        else if (ruleset == 1)
        {
            if (batt[0] == 0) select = 1;
            else if ((serl[5] - 48) % 2 == 1) select = 2;
            else if (port[4] == 0) select = 4;
            else if (indc[5] != 0) select = 6;
            else select = 5;
        }
        else if (ruleset == 2)
        {
            if (port[0] != 0) select = 7;
            else if (batt[0] == 2) select = 0;
            else if (_hasvowels == false) select = 4;
            else if (indc[3] != 0) select = 2;
            else select = 5;
        }
        else if (ruleset == 3)
        {
            if (port[1] != 0) select = 0;
            else if (batt[0] < 2) select = 7;
            else if (indc[4] != 0) select = 3;
            else if (port[2] == 0) select = 1;
            else select = 2;
        }
        else if (ruleset == 4)
        {
            if (batt[0] < 3) select = 5;
            else if (port[5] == 0) select = 7;
            else if (indc[2] != 0) select = 6;
            else if (port[3] != 0) select = 3;
            else select = 4;
        }
        else
        {
            if (port[4] == 0) select = 4;
            else if (_hasvowels == true) select = 6;
            else if (indc[6] != 0) select = 2;
            else if ((serl[5] - 48) % 2 == 0) select = 0;
            else select = 7;
        }
    }

    void HandlePress(int mode)
    {
        Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, buttons[mode].transform);
        buttons[mode].AddInteractionPunch();
        Debug.Log("[Tfield] Button " + mode.ToString() + " was pressed.");
        if (verif[mode] == 1)
        {
            anscnt--;
            Debug.Log("[Tfield] One down! " + anscnt.ToString() + " left!");
            verif[mode] = 0;
			info[mode].text = "✓";
        } 
        else
        {
            Debug.Log("[Tfield] Incorrect or repeat!");
            GetComponent<KMBombModule>().HandleStrike();
            info[mode].text = "X";
        }
        if(anscnt == 0 && _lightson == true)
        {
            Debug.Log("[Tfield] Module disarmed.");
            GetComponent<KMBombModule>().HandlePass();
        }
    }
}
