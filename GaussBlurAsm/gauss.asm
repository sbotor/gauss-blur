.data?
data_array qword ptr ? ; Image data
helper_array qword ptr ? ; Helper array
kernel_array qword ptr ? ; Kernel array

stride qword ? ; Image stride in bytes
height qword ? ; Image height in pixels
padding qword ? ; Image padding in bytes
byte_width qword ? ; Image width in bytes (stride - padding)

byte_width_6 qword ? ; byte_width - 6
height_2 qword ?

.data
perm_mask_x dword 3, 4, 5, 0, 0, 0, 0, 0  ; Mask for moving 3-5 bytes to 0-3 in YMM

.code

Init proc

	;---- Arguments ----;
	; TODO
	;-------------------;

	mov data_array, rcx ; Image data array ptr
	mov helper_array, rdx ; Helper array ptr
	mov stride, r8 ; Image stride in bytes
	mov height, r9 ; Image height in pixels
	mov rcx, [rsp+40]
	mov kernel_array, rcx ; Kernel array ptr

	; Calculate padding (stride % 4)
	mov rax, r8
	xor rdx, rdx
	mov r10, 4
	div r10
	mov padding, rdx

	; Calculate byte width (stride - padding)
	sub r8, rdx
	mov byte_width, r8

	; Calculate additional byte widths
	sub r8, 6
	mov byte_width_6, r8

	; Calculate img_height - 2
	sub r9, 2
	mov height_2, r9

	ret
Init endp

BlurX proc
	push rbx
	push rsi
	push rdi
	push r10
	push r11
	push r12
	push r13
	push r14
	push r15

	;---- Arguments ----;
	; TODO
	;-------------------;

	;---- Registers ----;
	; R8 - end index (end)
	; RSI - data array pointer
	; RDI - helper array pointer

	; R11 - image stride
	; R12 - image byte width
	; R13 - image byte width decreased by 6
	; R14 - padding
	; R15 - image height decreased by 2

	; Main loop:
		; RCX - loop counter (i)
		; R9 - byte index in the current row (x)
		; R10 - current row (y)

	; XMM registers:
		; XMM0 - center pixel data and convolution sum
		; XMM3 - kernel data for the center pixel

	; YMM registers:
		; YMM1-YMM2 - image data for the neighbouring pixels
		; YMM4-YMM5 - kernel data for the neighbouring pixels
		; YMM6 - permutation mask
	;-------------------;

	mov r8, rdx ; Store end index
	mov rsi, data_array ; Store data pointer
	mov rdi, helper_array ; Store helper pointer

	mov r11, stride ; Store image stride
	mov r12, byte_width ; Store byte width
	mov r13, byte_width_6 ; Store byte_width - 6
	mov r14, padding ; Store padding
	mov r15, height_2 ; Store height - 2

	; Store kernel
	mov rbx, kernel_array
	vmovdqu ymm4, ymmword ptr [rbx]
	movdqu xmm3, xmmword ptr [rbx + 6 * 4]
	vmovdqu ymm5, ymmword ptr [rbx + 9 * 4]

	; Store permutation mask
	vmovdqu	ymm6, ymmword ptr [perm_mask_x]

	while_loop: ; i < end
		cmp ecx, r8d
		jge return

		; y = i / stride
		; x = i % stride
		mov eax, ecx ; Move loop counter to RAX
		xor edx, edx
		div r11d ; Divide by stride
		mov r10d, eax ; Move the result to R10
		mov r9d, edx ; Store remainder in R9

		; if (x >= 6 && x <= byte_width - 6 && y > 2 && y < imageHeight - 2)
		cmp r9d, 6 ; if (x >= 6)
		jl end_while
		cmp r9d, r13d ; if (x <= byte_width - 6)
		jg end_while
		cmp r10d, 2 ; if (y > 2)
		jle end_while
		cmp r10d, r15d ; if (y < imageHeight - 2)
		jge end_while

		pmovzxbd xmm0, [rsi + rcx] ; Move center pixel data
		;pmuldq xmm0, xmm3
		;psrad xmm0, 24
		cvtdq2ps xmm0, xmm0
		mulps xmm0, xmm3
		cvtps2dq xmm0, xmm0

		vpmovzxbd ymm1, qword ptr [rsi + rcx - 6] ; Move first 8 bytes containing two pixels to the left
		;vpmuldq ymm1, ymm1, ymm4
		;vpsrad ymm1, ymm1, 24
		vcvtdq2ps ymm1, ymm1
		vmulps ymm1, ymm1, ymm4
		vcvtps2dq ymm1, ymm1
		
		vpmovzxbd ymm2, qword ptr [rsi + rcx + 3] ; Move last 8 bytes containing two pixels to the right
		;vpmuldq ymm2, ymm2, ymm5
		;vpsrad ymm1, ymm1, 24
		vcvtdq2ps ymm2, ymm2
		vmulps ymm2, ymm2, ymm5
		vcvtps2dq ymm2, ymm2

		; Add the values
		vpaddd ymm1, ymm1, ymm2
		paddd xmm0, xmm1
		vpermd ymm1, ymm6, ymm1
		paddd xmm0, xmm1

	color1:
		xor rax, rax
		pextrd eax, xmm0, 0
		cmp eax, 255
		jg high1
		cmp eax, 0
		jl low1
		mov [rdi + rcx], al
		jmp color2
	high1:
		mov byte ptr [rdi + rcx], 255
		jmp color2
	low1:
		mov byte ptr [rdi + rcx], 0

	color2:
		pextrd eax, xmm0, 1
		cmp eax, 255
		jg high2
		cmp eax, 0
		jl low2
		mov [rdi + rcx + 1], al
		jmp color3
	high2:
		mov byte ptr [rdi + rcx + 1], 255
		jmp color3
	low2:
		mov byte ptr [rdi + rcx + 1], 0

	color3:
		pextrd eax, xmm0, 2
		cmp eax, 255
		jg high3
		cmp eax, 0
		jl low3
		mov [rdi + rcx + 2], al
		jmp end_while
	high3:
		mov byte ptr [rdi + rcx + 2], 255
		jmp end_while
	low3:
		mov byte ptr [rdi + rcx + 2], 0

	end_while:
		add ecx, 3 ; i += 3
		; if (i % byte_width == 0)
		xor rdx, rdx
		mov rax, rcx
		div r12d
		cmp edx, 0
		jne while_loop
		add ecx, r14d ; i += padding
		jmp while_loop

return:
	pop r15
	pop r14
	pop r13
	pop r12
	pop r11	
	pop r10
	pop rdi
	pop rsi
	pop rbx
	ret

BlurX endp

BlurY proc
	push rbx
	push rsi
	push rdi
	push r10
	push r11
	push r12
	push r13
	push r14
	push r15

	;---- Arguments ----;
	; TODO
	;-------------------;

	;---- Registers ----;
	; R8 - end index (end)
	; RSI - data array pointer
	; RDI - helper array pointer

	; R11 - image stride
	; R12 - image byte width
	; R13 - image byte width decreased by 6
	; R14 - padding
	; R15 - image height decreased by 2

	; Main loop:
		; RCX - loop counter (i)
		; R9 - byte index in the current row (x)
		; R10 - current row (y)

	; XMM registers:
		; XMM0 - center pixel data and convolution sum
		; XMM3 - kernel data for the center pixel

	; YMM registers:
		; YMM1-YMM2 - image data for the neighbouring pixels
		; YMM4-YMM5 - kernel data for the neighbouring pixels
		; YMM6 - permutation mask
	;-------------------;

	mov r8, rdx ; Store end index
	mov rdi, data_array ; Store data pointer
	mov rsi, helper_array ; Store helper pointer

	mov r11, stride ; Store image stride
	mov r12, byte_width ; Store byte width
	mov r13, byte_width_6 ; Store byte_width - 6
	mov r14, padding ; Store padding
	mov r15, height_2 ; Store height - 2

	; Store kernel
	mov rbx, kernel_array
	vmovdqu ymm4, ymmword ptr [rbx]
	movdqu xmm3, xmmword ptr [rbx + 6 * 4]
	vmovdqu ymm5, ymmword ptr [rbx + 9 * 4]

	; Store permutation mask
	vmovdqu	ymm6, ymmword ptr [perm_mask_x]

	while_loop: ; i < end
		cmp ecx, r8d
		jge return

		; y = i / stride
		; x = i % stride
		mov eax, ecx ; Move loop counter to RAX
		xor edx, edx
		div r11d ; Divide by stride
		mov r10d, eax ; Move the result to R10
		mov r9d, edx ; Store remainder in R9

		; if (x >= 6 && x <= byte_width - 6 && y > 2 && y < imageHeight - 2)
		cmp r9d, 6 ; if (x >= 6)
		jl end_while
		cmp r9d, r13d ; if (x <= byte_width - 6)
		jg end_while
		cmp r10d, 2 ; if (y > 2)
		jle end_while
		cmp r10d, r15d ; if (y < imageHeight - 2)
		jge end_while

		mov al, [rsi + rcx]
		mov [rdi + rcx], al

		mov al, [rsi + rcx + 1]
		mov [rdi + rcx + 1], al

		mov al, [rsi + rcx + 2]
		mov [rdi + rcx + 2], al

	end_while:
		add ecx, 3 ; i += 3
		; if (i % byte_width == 0)
		xor rdx, rdx
		mov rax, rcx
		div r12d
		cmp edx, 0
		jne while_loop
		add ecx, r14d ; i += padding
		jmp while_loop

return:
	pop r15
	pop r14
	pop r13
	pop r12
	pop r11	
	pop r10
	pop rdi
	pop rsi
	pop rbx
	ret

BlurY endp

end