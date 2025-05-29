__kernel void Mandelbrot02(
    __global uchar* pixels,
    int width,
    int height,
    float zoom,
    float offsetX,
    float offsetY,
    int maxIter)
{
    int px = get_global_id(0);
    int py = get_global_id(1);

    if (px >= width || py >= height)
        return;

    // Normalisierte Koordinaten, angepasst für zentrierten Zoom
    float x0 = (px - width / 2.0f) / (width / 2.0f) / zoom + offsetX;
    float y0 = (py - height / 2.0f) / (height / 2.0f) / zoom + offsetY;

    float x = 0.0f;
    float y = 0.0f;
    int iter = 0;

    // Optimierung: Quadratische Vergleiche vermeiden Wurzelberechnung
    while (x * x + y * y <= 4.0f && iter < maxIter)
    {
        float xtemp = x * x - y * y + x0;
        y = 2.0f * x * y + y0;
        x = xtemp;
        iter++;
    }

    int pixelIndex = (py * width + px) * 4;

    // Farbgebung: Verschiedene Farbschemata für mehr Vielfalt
    if (iter == maxIter) {
        pixels[pixelIndex + 0] = 0;     // R
        pixels[pixelIndex + 1] = 0;     // G
        pixels[pixelIndex + 2] = 0;     // B
        pixels[pixelIndex + 3] = 255;   // A
    } else {
        // Wähle ein Farbschema (du kannst diese ändern)
        uchar r, g, b;
        if (iter < maxIter / 4) {
            r = 255 * iter / (maxIter / 4);
            g = 0;
            b = 0;
        } else if (iter < maxIter / 2) {
            r = 255;
            g = 255 * (iter - maxIter / 4) / (maxIter / 4);
            b = 0;
        } else if (iter < maxIter * 3 / 4) {
            r = 255 - 255 * (iter - maxIter / 2) / (maxIter / 4);
            g = 255;
            b = 0;
        } else {
            r = 0;
            g = 255;
            b = 255 * (iter - maxIter * 3 / 4) / (maxIter / 4);
        }
        pixels[pixelIndex + 0] = r; // R
        pixels[pixelIndex + 1] = g; // G
        pixels[pixelIndex + 2] = b; // B
        pixels[pixelIndex + 3] = 255; // A
    }
}
