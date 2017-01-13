using UnityEngine;
using System.Collections;

public class InfoObjectScript : MonoBehaviour {

    public bool isAi;
    public int weightCase;

	void Start () {
        DontDestroyOnLoad(gameObject);
	}
	
}
