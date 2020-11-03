// GENERATED AUTOMATICALLY FROM 'Assets/Misc/Input System/ControlActions.inputactions'

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;

public class @ControlActions : IInputActionCollection, IDisposable
{
    public InputActionAsset asset { get; }
    public @ControlActions()
    {
        asset = InputActionAsset.FromJson(@"{
    ""name"": ""ControlActions"",
    ""maps"": [
        {
            ""name"": ""Player"",
            ""id"": ""2881a8e0-f359-4cca-a9e5-d82318b67ec7"",
            ""actions"": [
                {
                    ""name"": ""Accelerate"",
                    ""type"": ""Value"",
                    ""id"": ""9fc88983-29b3-44cd-8b91-f2014e81b493"",
                    ""expectedControlType"": ""Analog"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""Break"",
                    ""type"": ""Value"",
                    ""id"": ""05d0f103-9ec8-4838-9e99-bdb731622f05"",
                    ""expectedControlType"": ""Analog"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""Steer"",
                    ""type"": ""Value"",
                    ""id"": ""4710ec03-fee2-461d-8c2f-5a71c35a8562"",
                    ""expectedControlType"": ""Axis"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""TankStrafe"",
                    ""type"": ""Value"",
                    ""id"": ""055cf113-3c51-49a7-bd87-ccc80c442d18"",
                    ""expectedControlType"": ""Axis"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""TankAccelerate"",
                    ""type"": ""Value"",
                    ""id"": ""3a143e59-c738-4fd2-9694-5596757f4a76"",
                    ""expectedControlType"": ""Axis"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""TankTurn"",
                    ""type"": ""Value"",
                    ""id"": ""443643dc-2134-4529-8a42-f516298c78b9"",
                    ""expectedControlType"": ""Axis"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""Ebreak"",
                    ""type"": ""Button"",
                    ""id"": ""c985acd9-636c-4c29-b8cc-02e3334e1572"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""Pitch"",
                    ""type"": ""Value"",
                    ""id"": ""f2dbe73f-e3ef-4f95-9415-f4dab7383240"",
                    ""expectedControlType"": ""Axis"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""Yaw"",
                    ""type"": ""Value"",
                    ""id"": ""c4d6ff17-e0af-4e32-a38d-0677f3aab1d7"",
                    ""expectedControlType"": ""Axis"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""Roll"",
                    ""type"": ""Value"",
                    ""id"": ""e721c547-8a1a-4fce-bc80-79bde509cd3b"",
                    ""expectedControlType"": ""Axis"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""Camera"",
                    ""type"": ""Value"",
                    ""id"": ""24015662-4113-43e9-b038-de7084bb4037"",
                    ""expectedControlType"": ""Stick"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""Fire"",
                    ""type"": ""Button"",
                    ""id"": ""50de3c2d-bc0a-4440-b1d8-542f687e543d"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""Boost"",
                    ""type"": ""Button"",
                    ""id"": ""2dac79c1-1eeb-435e-af0c-ae80057479d9"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""Switch"",
                    ""type"": ""Button"",
                    ""id"": ""197eb7da-7fc3-4492-b874-87552db25a53"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                }
            ],
            ""bindings"": [
                {
                    ""name"": """",
                    ""id"": ""b1bd2fa6-98bd-47aa-a63b-198ed5b05d23"",
                    ""path"": ""<Gamepad>/rightTrigger"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Main"",
                    ""action"": ""Accelerate"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""07935217-73c5-4aea-a6b2-5ec341963db7"",
                    ""path"": ""<Gamepad>/leftTrigger"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Main"",
                    ""action"": ""Break"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": ""1D Axis"",
                    ""id"": ""c01651e3-2cbf-4366-943f-08e09edd1271"",
                    ""path"": ""1DAxis"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Steer"",
                    ""isComposite"": true,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": ""negative"",
                    ""id"": ""04d4e1b6-116f-4a83-8b46-348c9762a74a"",
                    ""path"": ""<Gamepad>/leftStick/left"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Main"",
                    ""action"": ""Steer"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""positive"",
                    ""id"": ""d3712b26-1fcf-449c-a338-088ca2bdb7fc"",
                    ""path"": ""<Gamepad>/leftStick/right"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Main"",
                    ""action"": ""Steer"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": """",
                    ""id"": ""8f9aa567-c652-41a1-b4c2-43058e35687f"",
                    ""path"": ""<Gamepad>/buttonWest"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Main"",
                    ""action"": ""Ebreak"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": ""1D Axis"",
                    ""id"": ""17d3737b-a1c3-4157-ab0d-5eba45a00358"",
                    ""path"": ""1DAxis"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Pitch"",
                    ""isComposite"": true,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": ""negative"",
                    ""id"": ""187380ff-b986-4025-a5cc-b8e469da5f99"",
                    ""path"": ""<Gamepad>/leftStick/up"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Main"",
                    ""action"": ""Pitch"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""positive"",
                    ""id"": ""7e3250ec-6800-4c70-a761-2485faf2b19c"",
                    ""path"": ""<Gamepad>/leftStick/down"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Main"",
                    ""action"": ""Pitch"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""1D Axis"",
                    ""id"": ""b2666131-453c-463b-916e-85ea72f16ee9"",
                    ""path"": ""1DAxis"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Yaw"",
                    ""isComposite"": true,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": ""negative"",
                    ""id"": ""e760160d-5385-4da5-b09f-7611726d32f3"",
                    ""path"": ""<Gamepad>/leftStick/left"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Main"",
                    ""action"": ""Yaw"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""positive"",
                    ""id"": ""0e36f0e5-1b2b-469a-bb39-9e92d4570101"",
                    ""path"": ""<Gamepad>/leftStick/right"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Main"",
                    ""action"": ""Yaw"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""1D Axis"",
                    ""id"": ""15113cee-c3e4-4de6-b2a4-2c7760246bba"",
                    ""path"": ""1DAxis"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Roll"",
                    ""isComposite"": true,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": ""negative"",
                    ""id"": ""26529ca8-6e3b-4424-8ac7-aab198ee7de3"",
                    ""path"": ""<Gamepad>/dpad/left"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Main"",
                    ""action"": ""Roll"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""positive"",
                    ""id"": ""bb7752d8-8f5f-4b57-bb77-bc49c084e193"",
                    ""path"": ""<Gamepad>/dpad/right"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Main"",
                    ""action"": ""Roll"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": """",
                    ""id"": ""b78be3c3-dd9b-465b-b224-972dce4b0af9"",
                    ""path"": ""<Gamepad>/rightStick"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Main"",
                    ""action"": ""Camera"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""0f0b3720-96fd-4fe5-9da4-dfe9fd90547e"",
                    ""path"": ""<Gamepad>/rightShoulder"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Main"",
                    ""action"": ""Fire"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""f6ca051e-2423-474e-93d1-6fafa36b205b"",
                    ""path"": ""<Gamepad>/buttonSouth"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Main"",
                    ""action"": ""Boost"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""cb49f34d-dff2-4df7-97ba-9951715afa51"",
                    ""path"": ""<Gamepad>/buttonEast"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Main"",
                    ""action"": ""Switch"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": ""1D Axis"",
                    ""id"": ""4ffb4fc8-2126-4c6d-82e0-57af5eef8d29"",
                    ""path"": ""1DAxis"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Main"",
                    ""action"": ""TankStrafe"",
                    ""isComposite"": true,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": ""negative"",
                    ""id"": ""c375963f-181c-4d0e-978a-8c5d4c4b4640"",
                    ""path"": ""<Gamepad>/leftStick/left"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Main"",
                    ""action"": ""TankStrafe"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""positive"",
                    ""id"": ""1182c998-126c-4a77-858d-c80186b7bc7a"",
                    ""path"": ""<Gamepad>/leftStick/right"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Main"",
                    ""action"": ""TankStrafe"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""1D Axis"",
                    ""id"": ""6745c8f2-3a7e-41d6-b484-60a0a90e9afe"",
                    ""path"": ""1DAxis"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Main"",
                    ""action"": ""TankAccelerate"",
                    ""isComposite"": true,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": ""negative"",
                    ""id"": ""f4b612e4-2e74-4ffe-9edc-35cb7bc6752c"",
                    ""path"": ""<Gamepad>/leftStick/down"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Main"",
                    ""action"": ""TankAccelerate"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""positive"",
                    ""id"": ""91837812-8e8f-41ef-bb96-66e7a7adfb25"",
                    ""path"": ""<Gamepad>/leftStick/up"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Main"",
                    ""action"": ""TankAccelerate"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""1D Axis"",
                    ""id"": ""bb3d4479-7bc6-4e0e-afaf-d07a262bc76c"",
                    ""path"": ""1DAxis"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""TankTurn"",
                    ""isComposite"": true,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": ""negative"",
                    ""id"": ""25afd327-0e0c-4151-a175-37fcacc9e551"",
                    ""path"": ""<Gamepad>/rightStick/left"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Main"",
                    ""action"": ""TankTurn"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""positive"",
                    ""id"": ""298c6915-f60d-47f6-a731-82c3e460d7e3"",
                    ""path"": ""<Gamepad>/rightStick/right"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Main"",
                    ""action"": ""TankTurn"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                }
            ]
        }
    ],
    ""controlSchemes"": [
        {
            ""name"": ""Main"",
            ""bindingGroup"": ""Main"",
            ""devices"": [
                {
                    ""devicePath"": ""<Gamepad>"",
                    ""isOptional"": false,
                    ""isOR"": false
                }
            ]
        }
    ]
}");
        // Player
        m_Player = asset.FindActionMap("Player", throwIfNotFound: true);
        m_Player_Accelerate = m_Player.FindAction("Accelerate", throwIfNotFound: true);
        m_Player_Break = m_Player.FindAction("Break", throwIfNotFound: true);
        m_Player_Steer = m_Player.FindAction("Steer", throwIfNotFound: true);
        m_Player_TankStrafe = m_Player.FindAction("TankStrafe", throwIfNotFound: true);
        m_Player_TankAccelerate = m_Player.FindAction("TankAccelerate", throwIfNotFound: true);
        m_Player_TankTurn = m_Player.FindAction("TankTurn", throwIfNotFound: true);
        m_Player_Ebreak = m_Player.FindAction("Ebreak", throwIfNotFound: true);
        m_Player_Pitch = m_Player.FindAction("Pitch", throwIfNotFound: true);
        m_Player_Yaw = m_Player.FindAction("Yaw", throwIfNotFound: true);
        m_Player_Roll = m_Player.FindAction("Roll", throwIfNotFound: true);
        m_Player_Camera = m_Player.FindAction("Camera", throwIfNotFound: true);
        m_Player_Fire = m_Player.FindAction("Fire", throwIfNotFound: true);
        m_Player_Boost = m_Player.FindAction("Boost", throwIfNotFound: true);
        m_Player_Switch = m_Player.FindAction("Switch", throwIfNotFound: true);
    }

    public void Dispose()
    {
        UnityEngine.Object.Destroy(asset);
    }

    public InputBinding? bindingMask
    {
        get => asset.bindingMask;
        set => asset.bindingMask = value;
    }

    public ReadOnlyArray<InputDevice>? devices
    {
        get => asset.devices;
        set => asset.devices = value;
    }

    public ReadOnlyArray<InputControlScheme> controlSchemes => asset.controlSchemes;

    public bool Contains(InputAction action)
    {
        return asset.Contains(action);
    }

    public IEnumerator<InputAction> GetEnumerator()
    {
        return asset.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public void Enable()
    {
        asset.Enable();
    }

    public void Disable()
    {
        asset.Disable();
    }

    // Player
    private readonly InputActionMap m_Player;
    private IPlayerActions m_PlayerActionsCallbackInterface;
    private readonly InputAction m_Player_Accelerate;
    private readonly InputAction m_Player_Break;
    private readonly InputAction m_Player_Steer;
    private readonly InputAction m_Player_TankStrafe;
    private readonly InputAction m_Player_TankAccelerate;
    private readonly InputAction m_Player_TankTurn;
    private readonly InputAction m_Player_Ebreak;
    private readonly InputAction m_Player_Pitch;
    private readonly InputAction m_Player_Yaw;
    private readonly InputAction m_Player_Roll;
    private readonly InputAction m_Player_Camera;
    private readonly InputAction m_Player_Fire;
    private readonly InputAction m_Player_Boost;
    private readonly InputAction m_Player_Switch;
    public struct PlayerActions
    {
        private @ControlActions m_Wrapper;
        public PlayerActions(@ControlActions wrapper) { m_Wrapper = wrapper; }
        public InputAction @Accelerate => m_Wrapper.m_Player_Accelerate;
        public InputAction @Break => m_Wrapper.m_Player_Break;
        public InputAction @Steer => m_Wrapper.m_Player_Steer;
        public InputAction @TankStrafe => m_Wrapper.m_Player_TankStrafe;
        public InputAction @TankAccelerate => m_Wrapper.m_Player_TankAccelerate;
        public InputAction @TankTurn => m_Wrapper.m_Player_TankTurn;
        public InputAction @Ebreak => m_Wrapper.m_Player_Ebreak;
        public InputAction @Pitch => m_Wrapper.m_Player_Pitch;
        public InputAction @Yaw => m_Wrapper.m_Player_Yaw;
        public InputAction @Roll => m_Wrapper.m_Player_Roll;
        public InputAction @Camera => m_Wrapper.m_Player_Camera;
        public InputAction @Fire => m_Wrapper.m_Player_Fire;
        public InputAction @Boost => m_Wrapper.m_Player_Boost;
        public InputAction @Switch => m_Wrapper.m_Player_Switch;
        public InputActionMap Get() { return m_Wrapper.m_Player; }
        public void Enable() { Get().Enable(); }
        public void Disable() { Get().Disable(); }
        public bool enabled => Get().enabled;
        public static implicit operator InputActionMap(PlayerActions set) { return set.Get(); }
        public void SetCallbacks(IPlayerActions instance)
        {
            if (m_Wrapper.m_PlayerActionsCallbackInterface != null)
            {
                @Accelerate.started -= m_Wrapper.m_PlayerActionsCallbackInterface.OnAccelerate;
                @Accelerate.performed -= m_Wrapper.m_PlayerActionsCallbackInterface.OnAccelerate;
                @Accelerate.canceled -= m_Wrapper.m_PlayerActionsCallbackInterface.OnAccelerate;
                @Break.started -= m_Wrapper.m_PlayerActionsCallbackInterface.OnBreak;
                @Break.performed -= m_Wrapper.m_PlayerActionsCallbackInterface.OnBreak;
                @Break.canceled -= m_Wrapper.m_PlayerActionsCallbackInterface.OnBreak;
                @Steer.started -= m_Wrapper.m_PlayerActionsCallbackInterface.OnSteer;
                @Steer.performed -= m_Wrapper.m_PlayerActionsCallbackInterface.OnSteer;
                @Steer.canceled -= m_Wrapper.m_PlayerActionsCallbackInterface.OnSteer;
                @TankStrafe.started -= m_Wrapper.m_PlayerActionsCallbackInterface.OnTankStrafe;
                @TankStrafe.performed -= m_Wrapper.m_PlayerActionsCallbackInterface.OnTankStrafe;
                @TankStrafe.canceled -= m_Wrapper.m_PlayerActionsCallbackInterface.OnTankStrafe;
                @TankAccelerate.started -= m_Wrapper.m_PlayerActionsCallbackInterface.OnTankAccelerate;
                @TankAccelerate.performed -= m_Wrapper.m_PlayerActionsCallbackInterface.OnTankAccelerate;
                @TankAccelerate.canceled -= m_Wrapper.m_PlayerActionsCallbackInterface.OnTankAccelerate;
                @TankTurn.started -= m_Wrapper.m_PlayerActionsCallbackInterface.OnTankTurn;
                @TankTurn.performed -= m_Wrapper.m_PlayerActionsCallbackInterface.OnTankTurn;
                @TankTurn.canceled -= m_Wrapper.m_PlayerActionsCallbackInterface.OnTankTurn;
                @Ebreak.started -= m_Wrapper.m_PlayerActionsCallbackInterface.OnEbreak;
                @Ebreak.performed -= m_Wrapper.m_PlayerActionsCallbackInterface.OnEbreak;
                @Ebreak.canceled -= m_Wrapper.m_PlayerActionsCallbackInterface.OnEbreak;
                @Pitch.started -= m_Wrapper.m_PlayerActionsCallbackInterface.OnPitch;
                @Pitch.performed -= m_Wrapper.m_PlayerActionsCallbackInterface.OnPitch;
                @Pitch.canceled -= m_Wrapper.m_PlayerActionsCallbackInterface.OnPitch;
                @Yaw.started -= m_Wrapper.m_PlayerActionsCallbackInterface.OnYaw;
                @Yaw.performed -= m_Wrapper.m_PlayerActionsCallbackInterface.OnYaw;
                @Yaw.canceled -= m_Wrapper.m_PlayerActionsCallbackInterface.OnYaw;
                @Roll.started -= m_Wrapper.m_PlayerActionsCallbackInterface.OnRoll;
                @Roll.performed -= m_Wrapper.m_PlayerActionsCallbackInterface.OnRoll;
                @Roll.canceled -= m_Wrapper.m_PlayerActionsCallbackInterface.OnRoll;
                @Camera.started -= m_Wrapper.m_PlayerActionsCallbackInterface.OnCamera;
                @Camera.performed -= m_Wrapper.m_PlayerActionsCallbackInterface.OnCamera;
                @Camera.canceled -= m_Wrapper.m_PlayerActionsCallbackInterface.OnCamera;
                @Fire.started -= m_Wrapper.m_PlayerActionsCallbackInterface.OnFire;
                @Fire.performed -= m_Wrapper.m_PlayerActionsCallbackInterface.OnFire;
                @Fire.canceled -= m_Wrapper.m_PlayerActionsCallbackInterface.OnFire;
                @Boost.started -= m_Wrapper.m_PlayerActionsCallbackInterface.OnBoost;
                @Boost.performed -= m_Wrapper.m_PlayerActionsCallbackInterface.OnBoost;
                @Boost.canceled -= m_Wrapper.m_PlayerActionsCallbackInterface.OnBoost;
                @Switch.started -= m_Wrapper.m_PlayerActionsCallbackInterface.OnSwitch;
                @Switch.performed -= m_Wrapper.m_PlayerActionsCallbackInterface.OnSwitch;
                @Switch.canceled -= m_Wrapper.m_PlayerActionsCallbackInterface.OnSwitch;
            }
            m_Wrapper.m_PlayerActionsCallbackInterface = instance;
            if (instance != null)
            {
                @Accelerate.started += instance.OnAccelerate;
                @Accelerate.performed += instance.OnAccelerate;
                @Accelerate.canceled += instance.OnAccelerate;
                @Break.started += instance.OnBreak;
                @Break.performed += instance.OnBreak;
                @Break.canceled += instance.OnBreak;
                @Steer.started += instance.OnSteer;
                @Steer.performed += instance.OnSteer;
                @Steer.canceled += instance.OnSteer;
                @TankStrafe.started += instance.OnTankStrafe;
                @TankStrafe.performed += instance.OnTankStrafe;
                @TankStrafe.canceled += instance.OnTankStrafe;
                @TankAccelerate.started += instance.OnTankAccelerate;
                @TankAccelerate.performed += instance.OnTankAccelerate;
                @TankAccelerate.canceled += instance.OnTankAccelerate;
                @TankTurn.started += instance.OnTankTurn;
                @TankTurn.performed += instance.OnTankTurn;
                @TankTurn.canceled += instance.OnTankTurn;
                @Ebreak.started += instance.OnEbreak;
                @Ebreak.performed += instance.OnEbreak;
                @Ebreak.canceled += instance.OnEbreak;
                @Pitch.started += instance.OnPitch;
                @Pitch.performed += instance.OnPitch;
                @Pitch.canceled += instance.OnPitch;
                @Yaw.started += instance.OnYaw;
                @Yaw.performed += instance.OnYaw;
                @Yaw.canceled += instance.OnYaw;
                @Roll.started += instance.OnRoll;
                @Roll.performed += instance.OnRoll;
                @Roll.canceled += instance.OnRoll;
                @Camera.started += instance.OnCamera;
                @Camera.performed += instance.OnCamera;
                @Camera.canceled += instance.OnCamera;
                @Fire.started += instance.OnFire;
                @Fire.performed += instance.OnFire;
                @Fire.canceled += instance.OnFire;
                @Boost.started += instance.OnBoost;
                @Boost.performed += instance.OnBoost;
                @Boost.canceled += instance.OnBoost;
                @Switch.started += instance.OnSwitch;
                @Switch.performed += instance.OnSwitch;
                @Switch.canceled += instance.OnSwitch;
            }
        }
    }
    public PlayerActions @Player => new PlayerActions(this);
    private int m_MainSchemeIndex = -1;
    public InputControlScheme MainScheme
    {
        get
        {
            if (m_MainSchemeIndex == -1) m_MainSchemeIndex = asset.FindControlSchemeIndex("Main");
            return asset.controlSchemes[m_MainSchemeIndex];
        }
    }
    public interface IPlayerActions
    {
        void OnAccelerate(InputAction.CallbackContext context);
        void OnBreak(InputAction.CallbackContext context);
        void OnSteer(InputAction.CallbackContext context);
        void OnTankStrafe(InputAction.CallbackContext context);
        void OnTankAccelerate(InputAction.CallbackContext context);
        void OnTankTurn(InputAction.CallbackContext context);
        void OnEbreak(InputAction.CallbackContext context);
        void OnPitch(InputAction.CallbackContext context);
        void OnYaw(InputAction.CallbackContext context);
        void OnRoll(InputAction.CallbackContext context);
        void OnCamera(InputAction.CallbackContext context);
        void OnFire(InputAction.CallbackContext context);
        void OnBoost(InputAction.CallbackContext context);
        void OnSwitch(InputAction.CallbackContext context);
    }
}
