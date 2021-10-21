#include "pch.h"
#include "GaussBlurC.h"

int addTest(int first, int second) {
	return first + second;
}

//struct kernelStruct* createKernel(int size, double stdDev) {
//	struct kernelStruct* kernelP = malloc(sizeof(struct kernelStruct));
//
//	const double PI = acos(-1.0);
//
//	kernelP->size = size;
//	kernelP->maxCenterOffset = (size - 1) / 2;
//	kernelP->data = malloc(size * sizeof(double));
//
//	double variance = stdDev * stdDev,
//		constance = 1 / (sqrt(2.0 * PI * variance)),
//		kernelSum = 0;
//
//	/*for (int offset = -kernelP->maxCenterOffset; offset <= 0; offset++) {
//		double gaussValue = constance * exp(-(offset * offset) / (2 * variance));
//		
//		kernelP->data[offset + kernelP->maxCenterOffset] = gaussValue;
//		kernelP->data[kernelP->size + offset - 1] = gaussValue;
//		
//		kernelSum += gaussValue;
//	}*/
//
//	for (int offset = -kernelP->maxCenterOffset; offset <= kernelP->maxCenterOffset; offset++) {
//		double gaussValue = constance * exp(-(offset * offset) / (2 * variance));
//
//		kernelP->data[offset + kernelP->maxCenterOffset] = gaussValue;
//
//		kernelSum += gaussValue;
//	}
//
//	for (int offset = -kernelP->maxCenterOffset; offset <= kernelP->maxCenterOffset; offset++) {
//		kernelP->data[offset + kernelP->maxCenterOffset] /= kernelSum;
//	}
//
//	return kernelP;
//}

void normalizeColor(double* c) {
	if (*c > 255)
		*c = 255;
	else if (*c < 0)
		*c = 0;
}

void normalizeColors(double* c) {
	if (c[0] > 255)
		c[0] = 255;
	else if (c[0] < 0)
		c[0] = 0;

	if (c[1] > 255)
		c[1] = 255;
	else if (c[1] < 0)
		c[1] = 0;

	if (c[2] > 255)
		c[2] = 255;
	else if (c[2] < 0)
		c[2] = 0;
}

void blurX(double* src, double* dest, int start, int end, int imageStride, int imageHeight, double* kernel, int kernelSize) {
	int absStride = abs(imageStride), imageSize = absStride * imageHeight,
		startRow = start / imageStride,
		endRow = end / imageStride,
		maxCenterOffset = (kernelSize - 1) / 2;

	// First pass.
	for (int y = startRow; y < endRow; y++) {
		int rowOffset = y * imageStride;

		for (int x = 0; x < absStride; x += 3) {
			int pixel = x + rowOffset;
			if (pixel < start && pixel >= end)
				continue;

			double colors[3] = { 0.0 };

			for (int offset = -maxCenterOffset;
				offset <= maxCenterOffset;
				offset++) {
				if ((x + offset * 3 >= 0) && (x + offset * 3 + 2) < absStride) {
					colors[0] += src[pixel + 3 * offset] *
						kernel[offset + maxCenterOffset];

					colors[1] += src[pixel + 3 * offset + 1] *
						kernel[offset + maxCenterOffset];

					colors[2] += src[pixel + 3 * offset + 2] *
						kernel[offset + maxCenterOffset];
				}
			}

			normalizeColors(colors);

			dest[pixel] = colors[0];
			dest[pixel + 1] = colors[1];
			dest[pixel + 2] = colors[2];
		}
	}
}

void blurY(double* src, double* dest, int start, int end, int imageStride, int imageHeight, double* kernel, int kernelSize) {
	int absStride = abs(imageStride), imageSize = absStride * imageHeight,
		startRow = start / imageStride,
		endRow = end / imageStride,
		maxCenterOffset = (kernelSize - 1) / 2;

	// Second pass.
	for (int y = 0; y < imageHeight; y++) {
		int rowOffset = y * absStride;

		for (int x = 0; x < absStride; x += 3) {
			int pixel = x + rowOffset;
			if (pixel < start && pixel >= end)
				continue;

			double colors[3] = { 0.0 };

			for (int offset = -maxCenterOffset;
				offset <= maxCenterOffset; offset++) {
				if ((offset + y >= 0) && (offset + y < imageHeight)) {
					colors[0] += src[x + (y + offset) * imageStride] *
						kernel[offset + maxCenterOffset];

					colors[1] += src[x + (y + offset) * imageStride + 1] *
						kernel[offset + maxCenterOffset];

					colors[2] += src[x + (y + offset) * imageStride + 2] *
						kernel[offset + maxCenterOffset];
				}
			}

			normalizeColors(colors);

			dest[pixel] = colors[0];
			dest[pixel + 1] = colors[1];
			dest[pixel + 2] = colors[2];
		}
	}
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

//void blur(double* src, int start, int end, int imageStride, int imageHeight, int kernelSize, double stdDev) {
//		struct kernelStruct* kernelP = createKernel(kernelSize, stdDev);
//		int absStride = abs(imageStride), imageSize = absStride * imageHeight;
//		double* dest = malloc(imageSize * sizeof(double));
//
//		// First pass.
//		for (int y = 0; y < imageHeight; y++) {
//			int rowOffset = y * imageStride;
//
//			for (int x = 0; x < absStride; x += 3) {
//				double colors[3] = { 0.0, 0.0, 0.0 };
//				int pixel = x + rowOffset;
//
//				for (int offset = -kernelP->maxCenterOffset;
//					offset <= kernelP->maxCenterOffset; offset++) {
//					if ((x + offset * 3 >= 0) && (x + offset * 3 + 2) < absStride) {
//						colors[0] += src[pixel + 3 * offset] *
//							kernelP->data[offset + kernelP->maxCenterOffset];
//
//						colors[1] += src[pixel + 3 * offset + 1] *
//							kernelP->data[offset + kernelP->maxCenterOffset];
//
//						colors[2] += src[pixel + 3 * offset + 2] *
//							kernelP->data[offset + kernelP->maxCenterOffset];
//					}
//				}
//
//				colors[0] = normalizeColor(colors[0]);
//				colors[1] = normalizeColor(colors[1]);
//				colors[2] = normalizeColor(colors[2]);
//
//				dest[pixel] = colors[0];
//				dest[pixel + 1] = colors[1];
//				dest[pixel + 2] = colors[2];
//			}
//		}
//
//		// Second pass.
//		for (int y = 0; y < imageHeight; y++) {
//			int rowOffset = y * absStride;
//
//			for (int x = 0; x < absStride; x += 3) {
//				double colors[3] = { 0.0, 0.0, 0.0 };
//				int pixel = x + rowOffset;
//
//				for (int offset = -kernelP->maxCenterOffset;
//					offset <= kernelP->maxCenterOffset; offset++) {
//					if ((offset + y >= 0) && (offset + y < imageHeight)) {
//						colors[0] += dest[x + (y + offset) * imageStride] *
//							kernelP->data[offset + kernelP->maxCenterOffset];
//
//						colors[1] += dest[x + (y + offset) * imageStride + 1] *
//							kernelP->data[offset + kernelP->maxCenterOffset];
//
//						colors[2] += dest[x + (y + offset) * imageStride + 2] *
//							kernelP->data[offset + kernelP->maxCenterOffset];
//					}
//				}
//
//				colors[0] = normalizeColor(colors[0]);
//				colors[1] = normalizeColor(colors[1]);
//				colors[2] = normalizeColor(colors[2]);
//
//				src[pixel] = colors[0];
//				src[pixel + 1] = colors[1];
//				src[pixel + 2] = colors[2];
//			}
//		}
//
//
//	free(dest);
//	destroyKernel(kernelP);
//}

//void blur(double* src, int start, int end, int imageStride, int imageHeight, int kernelSize, double stdDev) {
//	struct kernelStruct* kernelP = createKernel(kernelSize, stdDev);
//	
//	int absStride = abs(imageStride), imageSize = absStride * imageHeight,
//		startRow = start / imageStride,
//		endRow = end / imageStride;
//	
//	double* dest = calloc(imageSize, sizeof(double));
//
//	// First pass.
//	for (int y = startRow; y < endRow; y++) {
//		int rowOffset = y * imageStride;
//
//		for (int x = 0; x < absStride; x += 3) {
//			int pixel = x + rowOffset;
//			if (pixel < start && pixel >= end)
//				continue;
//
//			for (int offset = -kernelP->maxCenterOffset;
//				offset <= kernelP->maxCenterOffset;
//				offset++) {
//				if ((x + offset * 3 >= 0) && (x + offset * 3 + 2) < absStride) {
//					dest[pixel] += src[pixel + 3 * offset] *
//						kernelP->data[offset + kernelP->maxCenterOffset];
//
//					dest[pixel + 1] += src[pixel + 3 * offset + 1] *
//						kernelP->data[offset + kernelP->maxCenterOffset];
//
//					dest[pixel + 2] += src[pixel + 3 * offset + 2] *
//						kernelP->data[offset + kernelP->maxCenterOffset];
//				}
//			}
//
//			normalizeColor(&(dest[pixel]));
//			normalizeColor(&(dest[pixel + 1]));
//			normalizeColor(&(dest[pixel + 2]));
//		}
//	}
//
//	memset(src, 0, imageSize * sizeof(double));
//	
//	// Second pass.
//	for (int y = 0; y < imageHeight; y++) {
//		int rowOffset = y * absStride;
//
//		for (int x = 0; x < absStride; x += 3) {
//			int pixel = x + rowOffset;
//			if (pixel < start && pixel >= end)
//				continue;
//
//			for (int offset = -kernelP->maxCenterOffset;
//				offset <= kernelP->maxCenterOffset; offset++) {
//				if ((offset + y >= 0) && (offset + y < imageHeight)) {
//					src[pixel] += dest[x + (y + offset) * imageStride] *
//						kernelP->data[offset + kernelP->maxCenterOffset];
//
//					src[pixel + 1] += dest[x + (y + offset) * imageStride + 1] *
//						kernelP->data[offset + kernelP->maxCenterOffset];
//
//					src[pixel + 2] += dest[x + (y + offset) * imageStride + 2] *
//						kernelP->data[offset + kernelP->maxCenterOffset];
//				}
//			}
//
//			normalizeColor(&(src[pixel]));
//			normalizeColor(&(src[pixel + 1]));
//			normalizeColor(&(src[pixel + 2]));
//		}
//	}
//
//
//	free(dest);
//	destroyKernel(kernelP);
//}