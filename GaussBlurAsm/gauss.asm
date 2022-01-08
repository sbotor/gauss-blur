.data?
data_array qword ptr ?
helper_array qword ptr ?
kernel_array qword ptr ?

img_stride qword ?
img_height qword ?
img_padding qword ?
img_byte_width qword ?

stride_8_less qword ?
stride_4_less qword ?
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
	mov rax, img_stride
	sub rax, 4
	mov stride_4_less, rax
	sub rax, 4
	mov stride_8_less, rax

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
	; R10 - stride - 8
	; R11 - stride - 4
	; R12 - byte_width
	; R13 - padding
	; R14 - img_stride

	; RSI - data_array
	; RBX - helper_array

	mov r8, rdx
	mov rsi, data_array
	mov rbx, helper_array
	mov r10, stride_8_less
	mov r11, stride_4_less
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
		
	middle:
		pmovzxbd xmm0, [rsi + rcx]
		pmulld xmm0, xmm12
		psrad xmm0, 24

	two_left:
		cmp r9, 6
		jl short one_left

		vpmovzxbd ymm2, qword ptr [rsi + rcx - 6]
		vpmulld ymm2, ymm2, ymm10
		vpsrad ymm2, ymm2, 24
		vpermd ymm1, ymm14, ymm2
		paddd xmm1, xmm2
		paddd xmm0, xmm1
		jmp short two_right

	one_left:
		cmp r9, 3
		jl short two_right
		
		pmovzxbd xmm1, [rsi + rcx - 3]
		pmulld xmm1, xmm11
		psrad xmm1, 24
		paddd xmm0, xmm1

	two_right:
		cmp r9, r10
		jge short one_right

		vpmovzxbd ymm2, qword ptr [rsi + rcx + 3]
		vpmulld ymm2, ymm2, ymm13
		vpsrad ymm2, ymm2, 24
		vpermd ymm1, ymm14, ymm2
		paddd xmm1, xmm2
		paddd xmm0, xmm1
		jmp short get_colors

	one_right:
		cmp r9, r11
		jge short get_colors

		pmovzxbd xmm1, [rsi + rcx + 3]
		pmulld xmm1, xmm13
		psrad xmm1, 24
		paddd xmm0, xmm1

	get_colors:
		packusdw xmm0, xmm0
		packuswb xmm0, xmm0
		pextrb byte ptr [rbx + rcx], xmm0, 0
		pextrb byte ptr [rbx + rcx + 1], xmm0, 1
		pextrb byte ptr [rbx + rcx + 2], xmm0, 2
		
		add rcx, 3
		xor rdx, rdx
		mov rax, rcx
		div r12
		test edx, edx
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