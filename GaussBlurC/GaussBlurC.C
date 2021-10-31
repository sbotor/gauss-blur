#include "pch.h"
#include "GaussBlurC.h"

int addTest(int first, int second) {
	return first + second;
}

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

void blurX(BYTE* src, BYTE* dest, int start, int end, int imageStride, int imageHeight, double* kernel) {
	const int absStride = abs(imageStride), imageSize = absStride * imageHeight,
		startRow = start / imageStride,
		endRow = end / imageStride,
		maxCenterOffset = 2;

	// First pass.
	for (int y = startRow; y <= endRow; y++) {
		int rowOffset = y * imageStride;

		for (int x = 0; x < absStride; x += 3) {
			int pixel = x + rowOffset;
			if (pixel < start || pixel >= end)
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

void blurY(BYTE* src, BYTE* dest, int start, int end, int imageStride, int imageHeight, double* kernel) {
	const int absStride = abs(imageStride), imageSize = absStride * imageHeight,
		startRow = start / imageStride,
		endRow = end / imageStride,
		maxCenterOffset = 2;

	// Second pass.
	for (int y = startRow; y <= endRow; y++) {
		int rowOffset = y * absStride;

		for (int x = 0; x < absStride; x += 3) {
			int pixel = x + rowOffset;
			if (pixel < start || pixel >= end)
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
