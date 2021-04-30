using UnityEngine;
using System.Collections.Generic;

// Punkte 	Aktion
// 10 	Schildkröte/Krebs oder Hüpfer umwerfen.
// 500 	Vereiser zerschmettern.
// 800 	Schildkröte/Krebs oder Hüpfer einsammeln.
// 800 	Münze
// 5000 	alle 10 Münzen in Bonuslevel 3 eingesammelt.
// 8000 	alle 10 Münzen in Bonuslevel 8, 15 ,22 oder im K.O.-Modus eingesammelt. 

[CreateAssetMenu(fileName="GameData", menuName="Data/Game Data")]
public class GameData : ScriptableObject
{
    public float SpawnDelay = 20f;
    public Vector2 GravityVector = new Vector2(0, -2);
    public List<LevelData> Levels;
}