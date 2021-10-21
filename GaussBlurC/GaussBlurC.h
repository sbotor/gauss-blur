#ifndef GAUSSBLURC
#define GAUSSBLURC

//struct kernelStruct {
//	double* data;
//	int size;
//	int maxCenterOffset;
//};

__declspec(dllexport) int addTest(int first, int second);

//__declspec(dllexport) struct kernelStruct* createKernel(int size, double stdDev);
//
//__declspec(dllexport) void destroyKernel(struct kernelStruct* kernelP);

__declspec(dllexport) void normalizeColor(double* c);

__declspec(dllexport) void normalizeColors(double* c);

__declspec(dllexport) void blurX(double* src, double* dest, int start, int end, int imageStride, int imageHeight, double* kernel, int kernelSize);

__declspec(dllexport) void blurY(double* src, double* dest, int start, int end, int imageStride, int imageHeight, double* kernel, int kernelSize);

//__declspec(dllexport) void blur(double* src, int start, int end, int imageStride, int imageHeight, int kernelSize, double stdDev);

//__declspec(dllexport) void blurThread(double* src, int start, int end, int imageStride, int imageHeight, int kernelSize, double stdDev);

#endif // !GAUSSBLURC
