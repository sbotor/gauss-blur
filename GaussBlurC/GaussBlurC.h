#ifndef GAUSSBLURC
#define GAUSSBLURC

void normalizeColors(float* c);

void addX(float* colors, int index, int pixel_off, BYTE* src, float* kernel);

__declspec(dllexport) void BlurX(BYTE* src, BYTE* dest, int start, int end, int imageStride, int imageHeight, float* kernel);

void addY(float* colors, int index, int pixel_off, BYTE* src, float* kernel, int stride);

__declspec(dllexport) void BlurY(BYTE* src, BYTE* dest, int start, int end, int imageStride, int imageHeight, float* kernel);

#endif // !GAUSSBLURC
