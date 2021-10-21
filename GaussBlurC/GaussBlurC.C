#include "pch.h"
#include "GaussBlurC.h"

int addTest(int first, int second) {
	return first + second;
}

struct kernelStruct* createKernel(int size, double stdDev) {
	struct kernelStruct* kernelP = malloc(sizeof(struct kernelStruct));

	const double PI = acos(-1.0);

	kernelP->size = size;
	kernelP->maxCenterOffset = (size - 1) / 2;
	kernelP->data = malloc(size * sizeof(double));

	double variance = stdDev * stdDev,
		constance = 1 / (sqrt(2.0 * PI * variance)),
		kernelSum = 0;

	/*for (int centerOffset = -kernelP->maxCenterOffset; centerOffset <= 0; centerOffset++) {
		double gaussValue = constance * exp(-(centerOffset * centerOffset) / (2 * variance));
		
		kernelP->data[centerOffset + kernelP->maxCenterOffset] = gaussValue;
		kernelP->data[kernelP->size + centerOffset - 1] = gaussValue;
		
		kernelSum += gaussValue;
	}*/

	for (int centerOffset = -kernelP->maxCenterOffset; centerOffset <= kernelP->maxCenterOffset; centerOffset++) {
		double gaussValue = constance * exp(-(centerOffset * centerOffset) / (2 * variance));

		kernelP->data[centerOffset + kernelP->maxCenterOffset] = gaussValue;

		kernelSum += gaussValue;
	}

	for (int centerOffset = -kernelP->maxCenterOffset; centerOffset <= kernelP->maxCenterOffset; centerOffset++) {
		kernelP->data[centerOffset + kernelP->maxCenterOffset] /= kernelSum;
	}

	return kernelP;
}

double normalizeColor(double c) {
	if (c > 255)
		c = 255;
	if (c < 0)
		c = 0;

	return c;
}

void destroyKernel(struct kernelStruct* kernelP) {
	if (kernelP != NULL) {
		if (kernelP->data != NULL) {
			free(kernelP->data);
			kernelP->data = NULL;
		}

		free(kernelP);
		kernelP = NULL;
	}
}

void blur(double* pixelArray, int start, int end, int imageStride, int imageHeight, int kernelSize, double stdDev) {
	struct kernelStruct* kernelP = createKernel(kernelSize, stdDev);
	int absStride = abs(imageStride), imageSize = absStride * imageHeight;
	double* helperArray = malloc(imageSize * sizeof(double));

	// First pass.
	for (int y = 0; y < imageHeight; y++) {
		int columnOffset = y * imageStride;

		for (int x = 0; x < absStride; x += 3) {
			double colors[3] = { 0.0, 0.0, 0.0 };
			int pixelOffset = x + columnOffset;

			for (int centerOffset = -kernelP->maxCenterOffset;
				centerOffset <= kernelP->maxCenterOffset; centerOffset++) {
				if ((x + centerOffset * 3 >= 0) && (x + centerOffset * 3 + 2) < absStride) {
					colors[0] += pixelArray[pixelOffset + 3 * centerOffset] *
						kernelP->data[centerOffset + kernelP->maxCenterOffset];
					
					colors[1] += pixelArray[pixelOffset + 3 * centerOffset + 1] *
						kernelP->data[centerOffset + kernelP->maxCenterOffset];
					
					colors[2] += pixelArray[pixelOffset + 3 * centerOffset + 2] *
						kernelP->data[centerOffset + kernelP->maxCenterOffset];
				}
			}

			colors[0] = normalizeColor(colors[0]);
			colors[1] = normalizeColor(colors[1]);
			colors[2] = normalizeColor(colors[2]);

			helperArray[pixelOffset] = colors[0];
			helperArray[pixelOffset + 1] = colors[1];
			helperArray[pixelOffset + 2] = colors[2];
		}
	}

	// Second pass.
	for (int y = 0; y < imageHeight; y++) {
		int columnOffset = y * absStride;

		for (int x = 0; x < absStride; x += 3) {
			double colors[3] = { 0.0, 0.0, 0.0 };
			int pixelOffset = x + columnOffset;

			for (int centerOffset = -kernelP->maxCenterOffset;
				centerOffset <= kernelP->maxCenterOffset; centerOffset++) {
				if ((centerOffset + y >= 0) && (centerOffset + y < imageHeight)) {
					colors[0] += helperArray[x + (y + centerOffset) * imageStride] *
						kernelP->data[centerOffset + kernelP->maxCenterOffset];

					colors[1] += helperArray[x + (y + centerOffset) * imageStride + 1] *
						kernelP->data[centerOffset + kernelP->maxCenterOffset];

					colors[2] += helperArray[x + (y + centerOffset) * imageStride + 2] *
						kernelP->data[centerOffset + kernelP->maxCenterOffset];
				}
			}

			colors[0] = normalizeColor(colors[0]);
			colors[1] = normalizeColor(colors[1]);
			colors[2] = normalizeColor(colors[2]);

			pixelArray[pixelOffset] = colors[0];
			pixelArray[pixelOffset + 1] = colors[1];
			pixelArray[pixelOffset + 2] = colors[2];
		}
	}

	free(helperArray);
	destroyKernel(kernelP);
}