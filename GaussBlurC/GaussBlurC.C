#include "pch.h"
#include "GaussBlurC.h"

void normalizeColors(float* c) {
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

#define ADD_COLOR_X(n, offset, kern_pos) colors[n] += src[i + 3 * offset + n] * kernel[kern_pos]
void BlurX(BYTE* src, BYTE* dest, int start, int end, int imageStride, int imageHeight, float* kernel) {
	const int padding = (imageStride % 4),
		byte_width = imageStride - padding;

	int i = start;
	while (i < end) {
		
		float colors[3] = { 0.0 };
		int x = i % imageStride;
		int y = i / imageStride;

		if (x >= 6 && x <= byte_width - 6 && y > 1 && y < imageHeight - 2) {
			// Two pixels to the left
			ADD_COLOR_X(0, -2, 2);
			ADD_COLOR_X(1, -2, 2);
			ADD_COLOR_X(2, -2, 2);

			ADD_COLOR_X(0, -1, 1);
			ADD_COLOR_X(1, -1, 1);
			ADD_COLOR_X(2, -1, 1);

			// The pixel itself
			ADD_COLOR_X(0, 0, 0);
			ADD_COLOR_X(1, 0, 0);
			ADD_COLOR_X(2, 0, 0);

			// Two pixels to the right
			ADD_COLOR_X(0, 1, 1);
			ADD_COLOR_X(1, 1, 1);
			ADD_COLOR_X(2, 1, 1);

			ADD_COLOR_X(0, 2, 2);
			ADD_COLOR_X(1, 2, 2);
			ADD_COLOR_X(2, 2, 2);

			normalizeColors(colors);

			dest[i] = colors[0];
			dest[i + 1] = colors[1];
			dest[i + 2] = colors[2];
		}

		i += 3;
		if (i % byte_width == 0) {
			i += padding;
		}
	}
}

#define ADD_COLOR_Y(n, offset, kern_offset) colors[n] += src[i + imageStride * offset + n] * kernel[kern_offset]
void BlurY(BYTE* src, BYTE* dest, int start, int end, int imageStride, int imageHeight, float* kernel) {
	const int padding = (imageStride % 4),
		byte_width = imageStride - padding;

	int i = start;
	while (i < end) {

		float colors[3] = { 0.0 };
		int x = i % imageStride;
		int y = i / imageStride;

		if (x >= 6 && x <= byte_width - 6 && y > 1 && y < imageHeight - 2) {
			// Two pixels up
			ADD_COLOR_Y(0, -2, 2);
			ADD_COLOR_Y(1, -2, 2);
			ADD_COLOR_Y(2, -2, 2);

			ADD_COLOR_Y(0, -1, 1);
			ADD_COLOR_Y(1, -1, 1);
			ADD_COLOR_Y(2, -1, 1);

			// The pixel itself
			ADD_COLOR_Y(0, 0, 0);
			ADD_COLOR_Y(1, 0, 0);
			ADD_COLOR_Y(2, 0, 0);

			// Two pixels down
			ADD_COLOR_Y(0, 1, 1);
			ADD_COLOR_Y(1, 1, 1);
			ADD_COLOR_Y(2, 1, 1);

			ADD_COLOR_Y(0, 2, 2);
			ADD_COLOR_Y(1, 2, 2);
			ADD_COLOR_Y(2, 2, 2);

			normalizeColors(colors);

			dest[i] = colors[0];
			dest[i + 1] = colors[1];
			dest[i + 2] = colors[2];
		}

		i += 3;
		if (i % byte_width == 0) {
			i += padding;
		}
	}
}