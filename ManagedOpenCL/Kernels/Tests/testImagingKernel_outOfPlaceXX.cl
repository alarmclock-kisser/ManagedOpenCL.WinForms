__kernel void testImagingKernel_outOfPlaceXX(
    __global const uchar* inputPixels,
    __global uchar* outputPixels,
    const int width,
    const int height,
    float threshold,
    const int thickness,
    const int edgeR,
    const int edgeG,
    const int edgeB)
{
    int x = get_global_id(0);
    int y = get_global_id(1);
}