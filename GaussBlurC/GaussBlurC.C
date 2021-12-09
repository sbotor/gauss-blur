#include "pch.h"
#include "GaussBlurC.h"

const int MAX_CENT_OFF = 2;

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

#define ADD_COLOR_X(n, offset, kern_pos) colors[n] += src[pixel + 3 * offset + n] * kernel[kern_pos]
void BlurX(BYTE* src, BYTE* dest, int start, int end, int imageStride, int imageHeight, double* kernel) {
	const int absStride = abs(imageStride), imageSize = absStride * imageHeight,
		startRow = start / imageStride,
		endRow = end / imageStride;

	// First pass.
	for (int y = startRow; y <= endRow; y++) {
		int rowOffset = y * imageStride;
		
		for (int x = 0; x < absStride; x += 3) {
			int pixel = x + rowOffset;
			if (pixel < start || pixel >= end)
				continue;

			double colors[3] = { 0.0 };

			// Third column and more: two cells to the left
			if (x > 5) {
				ADD_COLOR_X(0, -2, 0);
				ADD_COLOR_X(1, -2, 0);
				ADD_COLOR_X(2, -2, 0);

				ADD_COLOR_X(0, -1, 1);
				ADD_COLOR_X(1, -1, 1);
				ADD_COLOR_X(2, -1, 1);
			}
			// Second column: one cell to the left
			else if (x > 2) {
				ADD_COLOR_X(0, -1, 1);
				ADD_COLOR_X(1, -1, 1);
				ADD_COLOR_X(2, -1, 1);
			}

			// The pixel itself
			ADD_COLOR_X(0, 0, 2);
			ADD_COLOR_X(1, 0, 2);
			ADD_COLOR_X(2, 0, 2);

			// Third to last column and less: two cell to the right
			if (x + 8 < absStride) {
				ADD_COLOR_X(0, 1, 3);
				ADD_COLOR_X(1, 1, 3);
				ADD_COLOR_X(2, 1, 3);

				ADD_COLOR_X(0, 2, 4);
				ADD_COLOR_X(1, 2, 4);
				ADD_COLOR_X(2, 2, 4);
			}
			// Second to last column: one cell to the right
			else if (x + 5 < absStride) {
				ADD_COLOR_X(0, 1, 3);
				ADD_COLOR_X(1, 1, 3);
				ADD_COLOR_X(2, 1, 3);
			}

			normalizeColors(colors);

			dest[pixel] = colors[0];
			dest[pixel + 1] = colors[1];
			dest[pixel + 2] = colors[2];
		}
	}
}

#define ADD_COLOR_Y(n, offset, kern_pos) colors[n] += src[x + (y + offset) * imageStride + n] * kernel[kern_pos]
void BlurY(BYTE* src, BYTE* dest, int start, int end, int imageStride, int imageHeight, double* kernel) {
	const int absStride = abs(imageStride), imageSize = absStride * imageHeight,
		startRow = start / imageStride,
		endRow = end / imageStride;

	// Second pass.
	for (int y = startRow; y <= endRow; y++) {
		int rowOffset = y * absStride;

		for (int x = 0; x < absStride; x += 3) {
			int pixel = x + rowOffset;
			if (pixel < start || pixel >= end)
				continue;

			double colors[3] = { 0.0 };
			
			// Third row and more: two cells up
			if (y > 1) {
				ADD_COLOR_Y(0, -2, 0);
				ADD_COLOR_Y(1, -2, 0);
				ADD_COLOR_Y(2, -2, 0);

				ADD_COLOR_Y(0, -1, 1);
				ADD_COLOR_Y(1, -1, 1);
				ADD_COLOR_Y(2, -1, 1);
			}
			// Second row: one cell up
			else if (y > 0) {
				ADD_COLOR_Y(0, -1, 1);
				ADD_COLOR_Y(1, -1, 1);
				ADD_COLOR_Y(2, -1, 1);
			}

			// The pixel itself
			ADD_COLOR_Y(0, 0, 2);
			ADD_COLOR_Y(1, 0, 2);
			ADD_COLOR_Y(2, 0, 2);

			// Third to last row: two cells down
			if (y + 2 < imageHeight) {
				ADD_COLOR_Y(0, 1, 3);
				ADD_COLOR_Y(1, 1, 3);
				ADD_COLOR_Y(2, 1, 3);

				ADD_COLOR_Y(0, 2, 4);
				ADD_COLOR_Y(1, 2, 4);
				ADD_COLOR_Y(2, 2, 4);
			}
			// Second to last row: one cell down
			else if (y + 1 < imageHeight) {
				ADD_COLOR_Y(0, 1, 3);
				ADD_COLOR_Y(1, 1, 3);
				ADD_COLOR_Y(2, 1, 3);
			}

			normalizeColors(colors);

			dest[pixel] = colors[0];
			dest[pixel + 1] = colors[1];
			dest[pixel + 2] = colors[2];
		}
	}
}
