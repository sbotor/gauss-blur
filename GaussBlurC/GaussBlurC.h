#ifndef GAUSSBLURC
#define GAUSSBLURC

struct kernelStruct {
	double* data;
	int size;
	int maxCenterOffset;
};

__declspec(dllexport) int addTest(int first, int second);

__declspec(dllexport) struct kernelStruct* createKernel(int size, double stdDev);

__declspec(dllexport) void destroyKernel(struct kernelStruct* kernelP);

__declspec(dllexport) double normalizeColor(double c);

__declspec(dllexport) void blur(double* pixelArray, int start, int end, int imageStride, int imageHeight, int kernelSize, double stdDev);

#endif // !GAUSSBLURC
