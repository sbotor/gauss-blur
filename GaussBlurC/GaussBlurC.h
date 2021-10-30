#ifndef GAUSSBLURC
#define GAUSSBLURC

//struct kernelStruct {
//	double* data;
//	int size;
//	int maxCenterOffset;
//};

__declspec(dllexport) int addTest(int first, int second);

__declspec(dllexport) void normalizeColor(double* c);

__declspec(dllexport) void normalizeColors(double* c);

__declspec(dllexport) void blurX(BYTE* src, BYTE* dest, int start, int end, int imageStride, int imageHeight, double* kernel);

__declspec(dllexport) void blurY(BYTE* src, BYTE* dest, int start, int end, int imageStride, int imageHeight, double* kernel);

#endif // !GAUSSBLURC
