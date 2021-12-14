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

.data
permutation_mask dword  3, 4, 5, 0, 0, 0, 0, 0

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

	; RSI - data_array
	; RBX - helper_array

	mov r8, rdx
	mov rsi, data_array
	mov rbx, helper_array
	mov r10, b_width_6_less
	mov r11, b_width_3_less
	mov r12, img_byte_width
	mov r13, img_padding
	mov r14, img_stride

	; Save kernel array info
	mov rdx, kernel_array
	vmovups ymm10, [rdx] ; L2-L1
	vmovups ymm11, [rdx + 12] ; L1-C
	vmovups ymm12, [rdx + 24] ; C-R1
	vmovups ymm13, [rdx + 36] ; R1-R2
	vmovdqu ymm14, ymmword ptr [permutation_mask]

	loop_check:
		cmp rcx, r8
		jge return
	loop_x:
		xor rdx, rdx
		mov rax, rcx
		div r14
		mov r9, rdx
		
		cmp r9, 6 ; jump if i < 6
		jl L1?_R2?
		cmp r9, r10 ; jump if i > byte_width - 6
		jg L2_R1?

		; All 5 pixels
		vpmovzxbd ymm2, qword ptr [rsi + rcx - 6]
		vcvtdq2ps ymm2, ymm2
		vpmovzxbd ymm3, qword ptr [rsi + rcx + 3]
		vcvtdq2ps ymm3, ymm3
		vmulps ymm2, ymm2, ymm10
		vmulps ymm3, ymm3, ymm13
		
		pmovzxbd xmm0, [rsi + rcx]
		cvtdq2ps xmm0, xmm0
		mulps xmm0, xmm11
		
		vaddps ymm1, ymm2, ymm3
		addps xmm0, xmm1
		vpermd ymm1, ymm14, ymm1
		addps xmm0, xmm1
		jmp get_colors

	L1?_R2?:
		cmp r9, 3 ; jump if i < 3
		jl L0_R2?
		cmp r9, r10 ; jump if i < byte_width - 6
		jg L1_R1?
		
		vpmovzxbd ymm2, qword ptr [rsi + rcx - 3]
		vpmovzxbd ymm3, qword ptr [rsi + rcx + 3]
		vcvtdq2ps ymm2, ymm2
		vcvtdq2ps ymm3, ymm3

		vmulps ymm2, ymm2, ymm11
		vmulps ymm3, ymm3, ymm13
		
		vaddps ymm1, ymm1, ymm2
		vpermd ymm0, ymm14, ymm1
		addps xmm0, xmm1
		jmp get_colors

	L0_R2?:
		cmp r9, r10 ; jump if i < byte_width - 6
		jg L0_R1?
		cmp r9, r11 ; jump if i < byte_width - 3
		jg middle_only
		
		vpmovzxbd ymm2, qword ptr [rsi + rcx + 3]
		pmovzxbd xmm0, [rsi + rcx]
		vcvtdq2ps ymm2, ymm2
		cvtdq2ps xmm0, xmm0
		
		vmulps ymm2, ymm2, ymm13
		mulps xmm0, xmm12

		vpermd ymm1, ymm14, ymm2
		addps xmm0, xmm2
		addps xmm0, xmm1
		jmp get_colors

	L1_R1?:
		cmp r9, r11 ; jump if i < byte_width - 3
		jg L1_R0

		vpmovzxbd ymm2, qword ptr [rsi + rcx - 3]
		pmovzxbd xmm0, [rsi + rcx + 3]
		vcvtdq2ps ymm2, ymm2
		cvtdq2ps xmm0, xmm0

		vmulps ymm2, ymm2, ymm1
		mulps xmm0, xmm13

		vpermd ymm1, ymm14, ymm2
		addps xmm0, xmm2
		addps xmm0, xmm1
		jmp get_colors

	L1_R0:
		vpmovzxbd ymm1, qword ptr [rsi + rcx - 3]
		vcvtdq2ps ymm1, ymm2

		vmulps ymm1, ymm1, ymm11
		
		vpermd ymm0, ymm14, ymm1
		addps xmm0, xmm1
		jmp get_colors

	L2_R1?:
		cmp r9, r11 ; jump if i < byte_width - 3
		jg L2_R0
		
		vpmovzxbd ymm2, qword ptr [rsi + rcx - 6]
		vpmovzxbd ymm3, qword ptr [rsi + rcx]
		vcvtdq2ps ymm2, ymm2
		vcvtdq2ps ymm3, ymm3

		vmulps ymm2, ymm2, ymm10
		vmulps ymm3, ymm3, ymm13

		vaddps ymm1, ymm2, ymm3
		vpermd ymm0, ymm14, ymm1
		addps xmm0, xmm1
		jmp get_colors

	L2_R0:
		vpmovzxbd ymm2, qword ptr [rsi + rcx - 6]
		pmovzxbd xmm0, [rsi + rcx]
		vcvtdq2ps ymm2, ymm2
		cvtdq2ps xmm0, xmm0

		vmulps ymm2, ymm2, ymm10
		mulps xmm0, xmm12

		vpermd ymm1, ymm14, ymm2
		addps xmm1, xmm2
		addps xmm0, xmm1
		jmp get_colors

	L0_R1?:
		cmp r9, r11 ; jump if i < byte_width - 3
		jg middle_only
		
		vpmovzxbd ymm1, qword ptr [rsi + rcx]
		vcvtdq2ps ymm1, ymm1

		vmulps ymm1, ymm1, ymm12

		vpermd ymm0, ymm14, ymm1
		addps xmm0, xmm1
		jmp get_colors
		

	middle_only:
		pmovzxbd xmm0, [rsi + rcx]
		cvtdq2ps xmm0, xmm0
		mulps xmm0, xmm12

	get_colors:
		cvttps2dq xmm0, xmm0
		packusdw xmm0, xmm0
		packuswb xmm0, xmm0
		pextrb byte ptr [rbx + rcx], xmm0, 0
		pextrb byte ptr [rbx + rcx + 1], xmm0, 1
		pextrb byte ptr [rbx + rcx + 2], xmm0, 2
		
		add rcx, 3
		xor rdx, rdx
		mov rax, rcx
		div r12d
		cmp edx, 0
		jnz loop_check
		add rcx, r13
		jmp loop_check

return:
	pop rsi
	pop rbx
	ret

BlurX endp

BlurY proc
	push rbx
	
	mov rsi, helper_array
	mov r8, data_array

	loop_check:
		cmp rcx, rdx
		jge return
	loop_y:
		mov eax, [rsi + rcx]
		mov [r8 + rcx], eax
		add rcx, 4
		jmp loop_check

return:
	pop rbx
	ret

BlurY endp

END