namespace Cs2Toolkit.Models;

public static class PlayerBones
{
    // animgraph_2_beta indices (runtime logs 2026-06-18).
    public const int Origin = 0;
    public const int Pelvis = 1;
    public const int Neck = 5;
    public const int Head = 6;
    public const int Chest = 23;
    public const int ShoulderA = 9;
    public const int ElbowA = 10;
    public const int HandA = 11;
    public const int ShoulderB = 13;
    public const int ElbowB = 14;
    public const int HandB = 15;
    public const int HipA = 17;
    public const int KneeA = 18;
    public const int AnkleA = 19;
    public const int HipB = 20;
    public const int KneeB = 21;
    public const int AnkleB = 22;

    public const int Count = 28;
    public const int MatrixStride = 32;
    public const float MaxConnectionWorldDistance = 120f;

    public static readonly int[] RequiredIndices =
    [
        Pelvis, Neck, Head, Chest,
        ShoulderA, ElbowA, HandA,
        ShoulderB, ElbowB, HandB,
        HipA, KneeA, AnkleA,
        HipB, KneeB, AnkleB
    ];

    public static readonly (int From, int To)[] Connections =
    [
        (Neck, Head),
        (Neck, ShoulderA),
        (Neck, ShoulderB),
        (ElbowA, ShoulderA),
        (ElbowB, ShoulderB),
        (HandA, ElbowA),
        (HandB, ElbowB),
        (Neck, Chest),
        (Chest, Pelvis),
        (HipA, Pelvis),
        (KneeA, HipA),
        (AnkleA, KneeA),
        (HipB, Pelvis),
        (KneeB, HipB),
        (AnkleB, KneeB)
    ];
}
