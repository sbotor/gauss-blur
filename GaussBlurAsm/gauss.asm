.data?
data_array qword ptr ?
helper_array qword ptr ?
img_stride qword ?
img_height qword ?
img_size qword ?
kernel_array qword ptr ?

.code

Init proc
	mov data_array, rcx
	mov helper_array, rdx
	mov img_stride, r8
	mov img_height, r9
	mov rcx, [rsp+40]
	mov kernel_array, rcx

	; Calculate size
	mov rax, img_stride
	mul img_height
	mov img_size, rax

	ret
Init endp

BlurX proc

	; R8 - startPos
	; R9 - endPos
	; R10 - startRow
	; R11 - endRow
	; R15 - imageStride
	
	.code
	; Setup
	mov r15, img_stride ; Save image stride to R15
	mov r8, rcx ; Save starting position to R8
	mov r9, rdx ; Save ending position to R9

	mov rax, r8 ; Get starting position
	xor rdx, rdx ; Clear RDX
	div r15 ; start_row = start_pos / image_stride
	mov r10, rax ; Save the starting row to R10

	mov rax, r9 ; Get ending position
	xor rdx, rdx ; Clear RDX
	div r15 ; end_row = end_pos / image_stride
	mov r11, rax ; Save the ending row to R11

	; Outer loop
	mov rcx, r10 ; Outer loop counter in RCX
OUTER_LOOP:
	
	; R12 - row offset
	mov rax, rcx ; Get outer loop counter
	mul r15 ; Multiply by image stride
	mov r12, rax ; Save the row offset to R12

	; Inner loop
	mov rdx, 0 ; Inner loop counter in RDX
	
	; R13 - current pixel position
	INNER_LOOP:
		mov r13, r12 ; Get current row offset
		add r13, rdx ; Current pixel position in R13

		cmp r13, R8 ; Compare with start_pos
		jl INNER_LOOP_END
		cmp r13, R9 ; Compare with end_pos
		jge INNER_LOOP_END

		call BlurPixelX

	INNER_LOOP_END:
		add rdx, 3 ; Increment inner counter by 3
		cmp rdx, r15 ; Compare to image stride
		jl INNER_LOOP

OUTER_LOOP_END:
	inc rcx ; Increment outer counter by 1
	cmp rcx, r11 ; Compare to ending row
	jle OUTER_LOOP

	ret

BlurX endp

ADD_COLORS_X macro offs, kern_offs
	vmovupd ymm2, ymmword ptr [rcx + kern_offs * 32]
	pmovzxbd xmm1, dword ptr [rbx + r13 + offs * 3]
	vcvtdq2pd ymm1, xmm1
	vmulpd ymm1, ymm1, ymm2
	vaddpd ymm0, ymm0, ymm1
endm

BlurPixelX proc
	push rbx
	push rcx
	push rdx
	push r8
	push r9
	push r10
	
	vxorpd ymm0, ymm0, ymm0 ; Zero ymm0 which holds the sum
	mov rbx, data_array
	mov rcx, kernel_array
	
THIRD_COL:
	cmp rdx, 5
	jle SECOND_COL
	ADD_COLORS_X -2, 0
	ADD_COLORS_X -1, 1
	jmp PIXEL_CELL

SECOND_COL:
	cmp rdx, 2
	jle PIXEL_CELL
	ADD_COLORS_X -1, 1

PIXEL_CELL:
	ADD_COLORS_X 0, 2

	add rdx, 8
THIRD_TO_LAST_COL:
	cmp rdx, r15
	jge SECOND_TO_LAST_COL
	ADD_COLORS_X 1, 2
	ADD_COLORS_X 2, 3
	jmp GET_COLORS

SECOND_TO_LAST_COL:
	sub rdx, 3
	cmp rdx, r15
	jge GET_COLORS
	ADD_COLORS_X 1, 2
	
GET_COLORS:
	vcvtpd2dq xmm0, ymm0
	pextrd r8d, xmm0, 0
	pextrd r9d, xmm0, 1
	pextrd r10d, xmm0, 2

	call NormalizeColors
	mov [rbx + r13], r8b
	mov [rbx + r13 + 1], r9b
	mov [rbx + r13 + 2], r10b

	pop r10
	pop r9
	pop r8
	pop rdx
	pop rcx
	pop rbx
	ret

BlurPixelX endp

BlurY proc
	; R8 - startPos
	; R9 - endPos
	; R10 - startRow
	; R11 - endRow
	; R15 - imageStride
	
	.code
	; Setup
	mov r15, img_stride ; Save image stride to R15
	mov r8, rcx ; Save starting position to R8
	mov r9, rdx ; Save ending position to R9

	mov rax, r8 ; Get starting position
	xor rdx, rdx ; Clear RDX
	div r15 ; start_row = start_pos / image_stride
	mov r10, rax ; Save the starting row to R10

	mov rax, r9 ; Get ending position
	xor rdx, rdx ; Clear RDX
	div r15 ; end_row = end_pos / image_stride
	mov r11, rax ; Save the ending row to R11

	; Outer loop
	mov rcx, r10 ; Outer loop counter in RCX
OUTER_LOOP:
	
	; R12 - row offset
	mov rax, rcx ; Get outer loop counter
	mul r15 ; Multiply by image stride
	mov r12, rax ; Save the row offset to R12

	; Inner loop
	mov rdx, 0 ; Inner loop counter in RDX
	
	; R13 - current pixel position
	INNER_LOOP:
		mov r13, r12 ; Get current row offset
		add r13, rdx ; Current pixel position in R13

		cmp r13, R8 ; Compare with start_pos
		jl INNER_LOOP_END
		cmp r13, R9 ; Compare with end_pos
		jge INNER_LOOP_END

		call BlurPixelY

	INNER_LOOP_END:
		add rdx, 3 ; Increment inner counter by 3
		cmp rdx, r15 ; Compare to image stride
		jl INNER_LOOP

OUTER_LOOP_END:
	inc rcx ; Increment outer counter by 1
	cmp rcx, r11 ; Compare to ending row
	jle OUTER_LOOP

	ret
	
BlurY endp

ADD_COLORS_Y macro
	vmovupd ymm2, ymmword ptr [rcx + kern_offs * 32]
	pmovzxbd xmm1, dword ptr [rbx + r13 + offs * 3]
	vcvtdq2pd ymm1, xmm1
	vmulpd ymm1, ymm1, ymm2
	vaddpd ymm0, ymm0, ymm1
endm

BlurPixelY proc
	push rbx
	push rcx
	push rdx
	push r8
	push r9
	push r10
	
	vxorpd ymm0, ymm0, ymm0 ; Zero ymm0 which holds the sum
	mov rbx, data_array
	mov rcx, kernel_array


	pop r10
	pop r9
	pop r8
	pop rdx
	pop rcx
	pop rbx
	ret

BlurPixelY endp

NormalizeColors proc
	
FIRST_L:
	cmp r8d, 0
	jge FIRST_H
	mov r8d, 0
	jmp SECOND_L
FIRST_H:
	cmp r8d, 255
	jle SECOND_L
	mov r8d, 255

SECOND_L:
	cmp r9d, 0
	jge SECOND_H
	mov r9d, 0
SECOND_H:
	cmp r9d, 255
	jle THIRD_L
	mov r9d, 255

THIRD_L:
	cmp r10d, 0
	jge THIRD_H
	mov r10d, 0
THIRD_H:
	cmp r10d, 255
	jle RETURN
	mov r10d, 255

RETURN:
	ret
	
NormalizeColors endp

END