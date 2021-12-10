.data?
data_array qword ptr ?
helper_array qword ptr ?
kernel_array qword ptr ?

img_stride qword ?
img_height qword ?
img_padding qword ?
img_byte_width qword ?

b_width_6_less qword ?
b_width_3_less qword ?
img_height_2_less qword ?
img_height_1_less qword ?

.code

Init proc
	mov data_array, rcx
	mov helper_array, rdx
	mov img_stride, r8
	mov img_height, r9
	mov rcx, [rsp+40]
	mov kernel_array, rcx

	; Calculate padding
	mov rax, r8
	xor rdx, rdx
	mov r10, 4
	div r10
	mov img_padding, rdx

	; Calculate byte width
	sub r8, img_padding
	mov img_byte_width, r8

	; Calculate additional byte widths
	sub r8, 3
	mov b_width_3_less, r8
	sub r8, 3
	mov b_width_6_less, r8

	; Calculate additional heights
	sub r9, 1
	mov img_height_1_less, r9
	sub r9, 1
	mov img_height_2_less, r9

	ret
Init endp

ADD_COLORS_X0 macro
	pmovzxbd xmm1, dword ptr [rbx + rcx]
	vcvtdq2pd ymm1, xmm1
	vmulpd ymm1, ymm1, ymm2
	vaddpd ymm0, ymm0, ymm1
endm

ADD_COLORS_X1 macro off
	pmovzxbd xmm1, dword ptr [rbx + rcx + 3 * off]
	vcvtdq2pd ymm1, xmm1
	vmulpd ymm1, ymm1, ymm3
	vaddpd ymm0, ymm0, ymm1
endm

ADD_COLORS_X2 macro off
	pmovzxbd xmm1, dword ptr [rbx + rcx + 3 * off]
	vcvtdq2pd ymm1, xmm1
	vmulpd ymm1, ymm1, ymm4
	vaddpd ymm0, ymm0, ymm1
endm

BlurX proc
	push rbx
	push rsi
	; RCX - startPos and loop counter
	; R8 - endPos
	; R9 - X position = i % stride
	; R10 - byte_width - 6
	; R11 - byte_width - 3
	; R12 - byte_width
	; R13 - padding
	; R14 - img_stride

	mov r8, rdx
	mov rbx, data_array
	mov r10, b_width_6_less
	mov r11, b_width_3_less
	mov r12, img_byte_width
	mov r13, img_padding
	mov r14, img_stride

	; Save kernel array info
	mov rdx, kernel_array
	vmovupd ymm2, ymmword ptr [rdx]
	vmovupd ymm3, ymmword ptr [rdx + 32]
	vmovupd ymm4, ymmword ptr [rdx + 64]

LOOP_X_CHECK:
	cmp rcx, r8
	jge RETURN
LOOP_X:
		mov rax, rcx
		xor rdx, rdx
		div r14
		mov r9, rdx
		vpxor ymm0, ymm0, ymm0 ; Clear ymm0 which holds the sum

	THIRD_COL:
		cmp r9, 6
		jl SECOND_COL
		ADD_COLORS_X2 -2
		ADD_COLORS_X1 -1
		ADD_COLORS_X0
		jmp THIRD_TO_LAST_COL
	SECOND_COL:
		cmp r9, 3
		jl CENTER_PIXEL
		ADD_COLORS_X1 -1

	CENTER_PIXEL:
		ADD_COLORS_X0

	THIRD_TO_LAST_COL:
		cmp r9, r10
		jg SECOND_TO_LAST_COL
		ADD_COLORS_X2 2
		ADD_COLORS_X1 1
		jmp GET_COLORS
	SECOND_TO_LAST_COL:
		cmp r9, r11
		jg GET_COLORS
		ADD_COLORS_X1 1
	
	GET_COLORS:
		vcvtpd2dq xmm0, ymm0
		packusdw xmm0, xmm1
		packuswb xmm0, xmm1
	
		mov rdx, helper_array
		pextrb eax, xmm0, 0
		mov [rdx + rcx], al
		pextrb eax, xmm0, 1
		mov [rdx + rcx + 1], al
		pextrb eax, xmm0, 2
		mov [rdx + rcx + 2], al

	LOOP_X_INC:
		add rcx, 3
		mov rax, rcx
		xor rdx, rdx
		div r12
		cmp rdx, 0
		jne LOOP_X_CHECK
		add rcx, r13
		jmp LOOP_X_CHECK

RETURN:
	pop rsi
	pop rbx
	ret

BlurX endp

ADD_COLORS_Y0 macro
	pmovzxbd xmm1, dword ptr [rbx + rcx]
	vcvtdq2pd ymm1, xmm1
	vmulpd ymm1, ymm1, ymm2
	vaddpd ymm0, ymm0, ymm1
endm

ADD_COLORS_Y1 macro
	pmovzxbd xmm1, dword ptr [rbx + r15]
	vcvtdq2pd ymm1, xmm1
	vmulpd ymm1, ymm1, ymm3
	vaddpd ymm0, ymm0, ymm1
endm

ADD_COLORS_Y2 macro
	pmovzxbd xmm1, dword ptr [rbx + r15]
	vcvtdq2pd ymm1, xmm1
	vmulpd ymm1, ymm1, ymm4
	vaddpd ymm0, ymm0, ymm1
endm

BlurY proc
	push rbx
	push rsi

	; RCX - startPos and loop counter
	; R8 - endPos
	; R9 - Y position = i / stride
	; R10 - height - 2
	; R11 - height - 1
	; R12 - byte_width
	; R13 - padding
	; R14 - img_stride
	; R15 - row offset

	mov r8, rdx
	mov rbx, helper_array
	mov r10, img_height_2_less
	mov r11, img_height_2_less
	mov r12, img_byte_width
	mov r13, img_padding
	mov r14, img_stride

	; Save kernel array info
	mov rdx, kernel_array
	vmovupd ymm2, ymmword ptr [rdx]
	vmovupd ymm3, ymmword ptr [rdx + 32]
	vmovupd ymm4, ymmword ptr [rdx + 64]

LOOP_Y_CHECK:
	cmp rcx, r8
	jge RETURN
LOOP_Y:
		mov rax, rcx
		xor rdx, rdx
		div r14
		mov r9, rax

		vpxor ymm0, ymm0, ymm0 ; Clear ymm0 which holds the sum

	THIRD_ROW:
		cmp r9, 2
		jle SECOND_ROW
		mov r15, rcx
		sub r15, r14
		ADD_COLORS_Y1
		sub r15, r14
		ADD_COLORS_Y2
		ADD_COLORS_Y0
		jmp THIRD_TO_LAST_ROW
	SECOND_ROW:
		cmp r9, 1
		jle CENTER_PIXEL
		mov r15, rcx
		sub r15, r14
		ADD_COLORS_Y1

	CENTER_PIXEL:
		ADD_COLORS_Y0

	THIRD_TO_LAST_ROW:
		cmp r9, r10
		jge SECOND_TO_LAST_ROW
		mov r15, rcx
		add r15, r14
		ADD_COLORS_Y1
		add r15, r14
		ADD_COLORS_Y2
		jmp GET_COLORS
	SECOND_TO_LAST_ROW:
		cmp r9, r11
		jge GET_COLORS
		mov r15, rcx
		add r15, r14
		ADD_COLORS_Y1
	
	GET_COLORS:
		vcvtpd2dq xmm0, ymm0
		packusdw xmm0, xmm1
		packuswb xmm0, xmm1
	
		mov rdx, data_array
		pextrb eax, xmm0, 0
		mov [rdx + rcx], al
		pextrb eax, xmm0, 1
		mov [rdx + rcx + 1], al
		pextrb eax, xmm0, 2
		mov [rdx + rcx + 2], al

	LOOP_Y_INC:
		add rcx, 3
		mov rax, rcx
		xor rdx, rdx
		div r12
		cmp rdx, 0
		jne LOOP_Y_CHECK
		add rcx, r13
		jmp LOOP_Y_CHECK

RETURN:
	pop rsi
	pop rbx
	ret
	
BlurY endp

END