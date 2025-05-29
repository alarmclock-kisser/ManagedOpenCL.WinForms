__kernel void normalizeOutOfPlace01(
    __global const float* input,
    __global float* output,
    long inputLen,
    long outputLen,
    float maxValue)
{
    int gid = get_global_id(0);
    if (gid >= inputLen || gid >= outputLen) return;

    // 1. Finde das maximale absolute Sample (wie oben)
    float maxSample = 0.0f;
    for (int i = 0; i < inputLen; i++) {
        float absVal = fabs(input[i]);
        if (absVal > maxSample) {
            maxSample = absVal;
        }
    }

    // 2. Schreibe normalisierte Werte in den Output
    if (maxSample > 0.0f) {
        float scale = maxValue / maxSample;
        output[gid] = input[gid] * scale;
    } else {
        output[gid] = input[gid]; // Keine Skalierung nötig
    }
}