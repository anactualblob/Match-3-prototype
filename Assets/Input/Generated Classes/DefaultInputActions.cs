// GENERATED AUTOMATICALLY FROM 'Assets/Input/DefaultInputActions.inputactions'

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;

public class @DefaultInputActions : IInputActionCollection, IDisposable
{
    public InputActionAsset asset { get; }
    public @DefaultInputActions()
    {
        asset = InputActionAsset.FromJson(@"{
    ""name"": ""DefaultInputActions"",
    ""maps"": [
        {
            ""name"": ""DefaultMap"",
            ""id"": ""b978a709-cac5-4a69-b5ed-6bf463ae208b"",
            ""actions"": [
                {
                    ""name"": ""Touch"",
                    ""type"": ""Value"",
                    ""id"": ""5a11ab97-9d06-490a-b8f2-ab32615b42bc"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""TouchPosition"",
                    ""type"": ""Value"",
                    ""id"": ""d2fc7b62-749c-4691-995a-4cc285ad3568"",
                    ""expectedControlType"": ""Vector2"",
                    ""processors"": """",
                    ""interactions"": """"
                }
            ],
            ""bindings"": [
                {
                    ""name"": """",
                    ""id"": ""f902ce6e-07a0-4975-a2be-c76854ad20f1"",
                    ""path"": ""<Touchscreen>/primaryTouch/press"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Touch"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""02b14391-f85a-434e-b132-7f88fed2ea93"",
                    ""path"": ""<Touchscreen>/primaryTouch/position"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""TouchPosition"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                }
            ]
        }
    ],
    ""controlSchemes"": []
}");
        // DefaultMap
        m_DefaultMap = asset.FindActionMap("DefaultMap", throwIfNotFound: true);
        m_DefaultMap_Touch = m_DefaultMap.FindAction("Touch", throwIfNotFound: true);
        m_DefaultMap_TouchPosition = m_DefaultMap.FindAction("TouchPosition", throwIfNotFound: true);
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

    // DefaultMap
    private readonly InputActionMap m_DefaultMap;
    private IDefaultMapActions m_DefaultMapActionsCallbackInterface;
    private readonly InputAction m_DefaultMap_Touch;
    private readonly InputAction m_DefaultMap_TouchPosition;
    public struct DefaultMapActions
    {
        private @DefaultInputActions m_Wrapper;
        public DefaultMapActions(@DefaultInputActions wrapper) { m_Wrapper = wrapper; }
        public InputAction @Touch => m_Wrapper.m_DefaultMap_Touch;
        public InputAction @TouchPosition => m_Wrapper.m_DefaultMap_TouchPosition;
        public InputActionMap Get() { return m_Wrapper.m_DefaultMap; }
        public void Enable() { Get().Enable(); }
        public void Disable() { Get().Disable(); }
        public bool enabled => Get().enabled;
        public static implicit operator InputActionMap(DefaultMapActions set) { return set.Get(); }
        public void SetCallbacks(IDefaultMapActions instance)
        {
            if (m_Wrapper.m_DefaultMapActionsCallbackInterface != null)
            {
                @Touch.started -= m_Wrapper.m_DefaultMapActionsCallbackInterface.OnTouch;
                @Touch.performed -= m_Wrapper.m_DefaultMapActionsCallbackInterface.OnTouch;
                @Touch.canceled -= m_Wrapper.m_DefaultMapActionsCallbackInterface.OnTouch;
                @TouchPosition.started -= m_Wrapper.m_DefaultMapActionsCallbackInterface.OnTouchPosition;
                @TouchPosition.performed -= m_Wrapper.m_DefaultMapActionsCallbackInterface.OnTouchPosition;
                @TouchPosition.canceled -= m_Wrapper.m_DefaultMapActionsCallbackInterface.OnTouchPosition;
            }
            m_Wrapper.m_DefaultMapActionsCallbackInterface = instance;
            if (instance != null)
            {
                @Touch.started += instance.OnTouch;
                @Touch.performed += instance.OnTouch;
                @Touch.canceled += instance.OnTouch;
                @TouchPosition.started += instance.OnTouchPosition;
                @TouchPosition.performed += instance.OnTouchPosition;
                @TouchPosition.canceled += instance.OnTouchPosition;
            }
        }
    }
    public DefaultMapActions @DefaultMap => new DefaultMapActions(this);
    public interface IDefaultMapActions
    {
        void OnTouch(InputAction.CallbackContext context);
        void OnTouchPosition(InputAction.CallbackContext context);
    }
}
