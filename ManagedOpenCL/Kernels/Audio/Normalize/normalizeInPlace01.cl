__kernel void normalizeInPlace01(
    __global float* input,
    long inputLen,
    float maxValue)
{
    int gid = get_global_id(0);
    if (gid >= inputLen) return;

    // 1. Finde das maximale absolute Sample (reduzierter Beispielcode)
    float maxSample = 0.0f;
    for (int i = 0; i < inputLen; i++) {
        float absVal = fabs(input[i]);
        if (absVal > maxSample) {
            maxSample = absVal;
        }
    }

    // 2. Normalisiere alle Samples (wenn maxSample > 0)
    if (maxSample > 0.0f) {
        float scale = maxValue / maxSample;
        input[gid] *= scale;
    }
}