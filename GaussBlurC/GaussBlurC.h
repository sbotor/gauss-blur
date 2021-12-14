#ifndef GAUSSBLURC
#define GAUSSBLURC

__declspec(dllexport) int addTest(int first, int second);

__declspec(dllexport) void normalizeColor(float* c);

__declspec(dllexport) void normalizeColors(float* c);

__declspec(dllexport) void BlurX(BYTE* src, BYTE* dest, int start, int end, int imageStride, int imageHeight, float* kernel);

__declspec(dllexport) void BlurY(BYTE* src, BYTE* dest, int start, int end, int imageStride, int imageHeight, float* kernel);

#endif // !GAUSSBLURC
