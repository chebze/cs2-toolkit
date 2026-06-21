namespace CS2Toolkit.Input.Abstractions;

public sealed record KeybindDefinition(string ActionId, string KeyName);

public static class ToolkitKeybindActions
{
    public const string Inject = "inject";
    public const string MenuToggle = "menu-toggle";
    public const string Panic = "panic";
    public const string SaveSettings = "save-settings";
    public const string RcsToggle = "rcs-toggle";
    public const string TriggerbotToggle = "triggerbot-toggle";
    public const string EnemyEspToggle = "enemy-esp-toggle";
    public const string SoundEspToggle = "sound-esp-toggle";
    public const string AimHelperToggle = "aimhelper-toggle";
    public const string AimHelperActivation = "aimhelper-activation";
    public const string TriggerbotAutoStrafe = "triggerbot-auto-strafe";
    public const string BulletTracersToggle = "bullet-tracers-toggle";
}
