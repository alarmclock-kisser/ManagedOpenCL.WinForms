// Optional: Wenn Vector2 nicht nativ unterstützt wird, explizit definieren:
typedef struct {
    float x;
    float y;
} Vector2;

__kernel void stretch01(
    __global const Vector2* input,
    __global Vector2* output,
    int inputLen,
    int outputLen,
    float factor)
{
    int gid = get_global_id(0);
    if (gid >= outputLen) return;

    // Position im Input
    float srcPos = gid / factor;
    int i0 = (int)floor(srcPos);
    int i1 = min(i0 + 1, inputLen - 1);
    float frac = srcPos - i0;

    // Input-Werte lesen
    Vector2 a = input[i0];
    Vector2 b = input[i1];

    // Interpolation im Betrag
    Vector2 interp;
    interp.x = a.x * (1.0f - frac) + b.x * frac;
    interp.y = a.y * (1.0f - frac) + b.y * frac;

    // Phase berechnen
    float angleA = atan2(a.y, a.x);
    float angleB = atan2(b.y, b.x);

    float dAngle = angleB - angleA;
    if (dAngle > M_PI_F) dAngle -= 2.0f * M_PI_F;
    if (dAngle < -M_PI_F) dAngle += 2.0f * M_PI_F;

    float interpAngle = angleA + frac * dAngle;

    // Betrag berechnen
    float mag = hypot(interp.x, interp.y);

    // Rekonstruktion in kartesischer Form
    output[gid].x = mag * cos(interpAngle);
    output[gid].y = mag * sin(interpAngle);
}
