using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class Launcher : MonoBehaviour 
{
	private bool newGame = false;
	private enum Game
	{
		ChaosHead,
		SteinsGate
	}
	
	private void Start() 
	{
		Game selectedGame = Game.SteinsGate; // temporary

		switch (selectedGame)
		{
			case Game.ChaosHead:
				// Launch Chaos;Head
				break;
			case Game.SteinsGate:
				// Launch Steins;Gate
				break;
		}
	}
}
