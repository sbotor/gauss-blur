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

void addX(float* colors, int index, int pixel_off, BYTE* src, float* kernel) {
	int off = abs(pixel_off);

	colors[0] += src[index + 3 * pixel_off] * kernel[off];
	colors[1] += src[index + 3 * pixel_off + 1] * kernel[off];
	colors[2] += src[index + 3 * pixel_off + 2] * kernel[off];
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

		if (x >= 6 && x <= byte_width - 6 && y > 2 && y < imageHeight - 2) {
			// Two pixels to the left
			addX(colors, i, -2, src, kernel);
			addX(colors, i, -1, src, kernel);
			
			// The pixel itself
			addX(colors, i, 0, src, kernel);

			// Two pixels to the right
			addX(colors, i, 1, src, kernel);
			addX(colors, i, 2, src, kernel);

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

void addY(float* colors, int index, int pixel_off, BYTE* src, float* kernel, int stride) {
	int off = abs(pixel_off);
	
	colors[0] += src[index + stride * pixel_off] * kernel[off];
	colors[1] += src[index + stride * pixel_off + 1] * kernel[off];
	colors[2] += src[index + stride * pixel_off + 2] * kernel[off];
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

		if (x >= 6 && x <= byte_width - 6 && y > 2 && y < imageHeight - 2) {
			// Two pixels up
			addY(colors, i, -2, src, kernel, imageStride);
			addY(colors, i, -1, src, kernel, imageStride);

			// The pixel itself
			addY(colors, i, 0, src, kernel, imageStride);

			// Two pixels down
			addY(colors, i, 1, src, kernel, imageStride);
			addY(colors, i, 2, src, kernel, imageStride);

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