using CNA.Interop;

namespace Microsoft.Xna.Framework.GamerServices;

/// <summary>A gamer's game-wide preferences, read once from native.</summary>
public sealed class GameDefaults
{
    internal GameDefaults(CnaGameDefaults native)
    {
        GameDifficulty = (GameDifficulty)native.GameDifficulty;
        ControllerSensitivity = (ControllerSensitivity)native.ControllerSensitivity;
        RacingCameraAngle = (RacingCameraAngle)native.RacingCameraAngle;
        PrimaryColor = native.HasPrimaryColor != 0
            ? new Color(native.PrimaryColor.R, native.PrimaryColor.G, native.PrimaryColor.B, native.PrimaryColor.A)
            : null;
        SecondaryColor = native.HasSecondaryColor != 0
            ? new Color(native.SecondaryColor.R, native.SecondaryColor.G, native.SecondaryColor.B, native.SecondaryColor.A)
            : null;
        AutoAim = native.AutoAim != 0;
        AutoCenter = native.AutoCenter != 0;
        MoveWithRightThumbStick = native.MoveWithRightThumbStick != 0;
        InvertYAxis = native.InvertYAxis != 0;
        ManualTransmission = native.ManualTransmission != 0;
        AccelerateWithButtons = native.AccelerateWithButtons != 0;
        BrakeWithButtons = native.BrakeWithButtons != 0;
    }

    public GameDifficulty GameDifficulty { get; }

    public ControllerSensitivity ControllerSensitivity { get; }

    public Color? PrimaryColor { get; }

    public Color? SecondaryColor { get; }

    public bool AutoAim { get; }

    public bool AutoCenter { get; }

    public bool MoveWithRightThumbStick { get; }

    public bool InvertYAxis { get; }

    public bool ManualTransmission { get; }

    public RacingCameraAngle RacingCameraAngle { get; }

    public bool AccelerateWithButtons { get; }

    public bool BrakeWithButtons { get; }
}
