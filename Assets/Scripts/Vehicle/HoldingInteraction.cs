﻿using System.ComponentModel;
using UnityEngine.InputSystem.Controls;
using UnityEngine.Scripting;
using UnityEngine.InputSystem;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

/// <summary>
/// Performs the action if the control is pressed and held for at least the
/// set duration (which defaults to <see cref="InputSettings.defaultHoldTime"/>).
/// </summary>
/// <remarks>
/// The action is started when the control is pressed. If the control is released before the
/// set <see cref="duration"/>, the action is canceled. As soon as the hold time is reached,
/// the action performs. The action then stays performed until the control is released, at
/// which point the action cancels.
///
/// <example>
/// <code>
/// // Action that requires A button on gamepad to be held for half a second.
/// var action = new InputAction(binding: "&lt;Gamepad&gt;/buttonSouth", interactions: "hold(duration=0.5)");
/// </code>
/// </example>
/// </remarks>
///

[Preserve]
[DisplayName("Holding")]
#if UNITY_EDITOR
[InitializeOnLoad]
#endif
public class HoldingInteraction : IInputInteraction
{
    static HoldingInteraction()
    {
        InputSystem.RegisterInteraction<HoldingInteraction>();
    }

    public static void Initialize()
    {
        // Calls the static constructor consequently
    }

    /// <summary>
    /// Duration in seconds that the control must be pressed for the hold to register.
    /// </summary>
    /// <remarks>
    /// If this is less than or equal to 0 (the default), <see cref="InputSettings.defaultHoldTime"/> is used.
    ///
    /// Duration is expressed in real time and measured against the timestamps of input events
    /// (<see cref="LowLevel.InputEvent.time"/>) not against game time (<see cref="Time.time"/>).
    /// </remarks>
    public float duration;

    /// <summary>
    /// Magnitude threshold that must be crossed by an actuated control for the control to
    /// be considered pressed.
    /// </summary>
    /// <remarks>
    /// If this is less than or equal to 0 (the default), <see cref="InputSettings.defaultButtonPressPoint"/> is used instead.
    /// </remarks>
    /// <seealso cref="InputControl.EvaluateMagnitude()"/>
    public float pressPoint;

    private float durationOrDefault => duration > 0.0 ? duration : InputSystem.settings.defaultHoldTime;
    private float pressPointOrDefault => pressPoint > 0.0 ? pressPoint : InputSystem.settings.defaultButtonPressPoint;

    private double m_TimePressed;

    /// <inheritdoc />
    public void Process(ref InputInteractionContext context)
    {
        if (context.timerHasExpired)
        {
            context.Performed();
            return;
        }

        switch (context.phase)
        {
            case InputActionPhase.Waiting:
                if (context.ControlIsActuated(pressPointOrDefault))
                {
                    m_TimePressed = context.time;

                    context.Started();
                    context.SetTimeout(durationOrDefault);
                }
                break;

            case InputActionPhase.Started:
                // If we've reached our hold time threshold, perform the hold.
                // We do this regardless of what state the control changed to.
                if (context.time - m_TimePressed >= durationOrDefault)
                {
                    context.Performed();
                }
                else if (!context.ControlIsActuated())
                {
                    // Control is no longer actuated and we haven't performed a hold yet,
                    // so cancel.
                    context.Canceled();
                }
                break;
        }
    }

    /// <inheritdoc />
    public void Reset()
    {
        m_TimePressed = 0;
    }
}