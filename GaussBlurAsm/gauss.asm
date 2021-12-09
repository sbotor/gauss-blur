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
	jl LOOP_X
	jmp RETURN
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
	
		pextrb eax, xmm0, 0
		mov [rbx + rcx], al
		pextrb eax, xmm0, 1
		mov [rbx + rcx + 1], al
		pextrb eax, xmm0, 2
		mov [rbx + rcx + 2], al

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

BlurY proc
	
	ret
	
BlurY endp

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