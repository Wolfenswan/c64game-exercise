using UnityEngine;
using UnityEngine.InputSystem;
using NEINGames.Debugging;

//! UNUSED

// InputStateContainer serves as a custom container to pass input states around
// A dictionary would serve the same purpose, the custom container mostly does improve readability, as there's no need to juggle dict-keys

public class InputStateContainer {

    public InputStateContainer() {}

    public float RawInputX = 0f;
    public float RawInputY = 0f;
    public bool IsJumpPressed = false;
    public bool IsJumpHeld = false;
    public bool IsWalkHeld = false;
    public bool IsFire1Pressed = false;
    public bool IsCancelPressed = false;
}

public class InputController : MonoBehaviour
{   
    [SerializeField] bool _debug = false;

    InputStateContainer _inputContainer = new InputStateContainer();

    public InputStateContainer UpdateInput() 
    {
        var keyboard = Keyboard.current;

        // TODO Update to new input System & allow distinguishing between two players; maybe Serialize more Fields? Or just Serialize a switch which player it is?
        // https://docs.unity3d.com/Packages/com.unity.inputsystem@1.0/manual/Migration.html#getAxis

        if (_debug)
            Logging.ListFields("Current inputs:", _inputContainer);

        return _inputContainer;
    }
}