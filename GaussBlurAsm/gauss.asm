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
	; RCX - data array pointer
	; RDX - helper array pointer
	; R8 - image stride in bytes (int64)
	; R9 - image height in pixels
	; Stack:
		; 40 - kernel array pointer
	;-------------------;

	;---- Registers ----;
	; RCX - used as a helper register
	; RDX - used during division
	;-------------------;

	mov data_array, rcx ; Image data array ptr
	mov helper_array, rdx ; Helper array ptr
	mov stride, r8 ; Image stride in bytes
	mov height, r9 ; Image height in pixels
	mov rcx, [rsp+40]
	mov kernel_array, rcx ; Kernel array ptr

	; Calculate padding (stride % 4)
	mov rax, r8 ; Store stride
	xor rdx, rdx
	mov rcx, 4
	div rcx ; stride / 4
	mov padding, rdx ; Store remainder

	; Calculate byte width (stride - padding)
	sub r8, rdx ; padding in RDX, stride in R8
	mov byte_width, r8

	; Calculate additional byte widths
	sub r8, 6 ; R8 holds byte_width, subtract 6 to get image width without the last two pixels
	mov byte_width_6, r8

	; Calculate img_height - 2
	sub r9, 2 ; R9 holds image height, subtract 2 to get image height without the last two pixels
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

	;---- Arguments ----;
	; RCX - start index (int64)
	; RDX - end index (int64)
	;-------------------;

	;---- Registers ----;
	; R8 - end index (end)
	; RSI - data array pointer
	; RDI - helper array pointer

	; R10 - image stride
	; R11 - image byte width
	; R12 - image byte width decreased by 6
	; R13 - padding
	; R14 - image height decreased by 2

	; Main loop:
		; RCX - loop counter (i)
		; RDX - byte index in the current row (x)
		; R9 - current row (y)

	; XMM registers:
		; XMM0 - center pixel data and convolution sum
		; XMM1 - leftmost or rightmost pixel data
		; XMM2 - data for pixel to the left or right of the center
		
		; XMM3 - kernel data for the center pixel
		; XMM4 - kernel data for the left and right pixels
		; XMM5 - kernel data for the leftmost and rightmost pixels
	;-------------------;

	mov r8, rdx ; Store end index
	mov rsi, data_array ; Store data pointer
	mov rdi, helper_array ; Store helper pointer

	mov r10, stride ; Store image stride
	mov r11, byte_width ; Store byte width
	mov r12, byte_width_6 ; Store byte_width - 6
	mov r13, padding ; Store padding
	mov r14, height_2 ; Store height - 2

	; Store kernel
	mov rbx, kernel_array
	movdqu xmm3, xmmword ptr [rbx + 6 * 4]
	movdqu xmm4, xmmword ptr [rbx + 3 * 4]
	movdqu xmm5, xmmword ptr [rbx]

	while_loop: ; i < end
		cmp rcx, r8
		jge return

		; y = i / stride
		; x = i % stride
		mov rax, rcx ; Move loop counter to RAX
		xor rdx, rdx ; Clear RDX
		div r10 ; Divide by stride
		mov r9, rax ; Store result (y) in R9

		; if (x >= 6 && x <= byte_width - 6 && y > 1 && y < imageHeight - 2)
		cmp rdx, 6 ; if (x >= 6)
		jl end_while
		cmp rdx, r12 ; if (x <= byte_width - 6)
		jg end_while
		cmp r9, 1 ; if (y > 1)
		jle end_while
		cmp r9, r14 ; if (y < imageHeight - 2)
		jge return

		;---- Calculate ----;

		; Center pixel
		pmovzxbd xmm0, [rsi + rcx] ; Store data
		cvtdq2ps xmm0, xmm0 ; Convert to floats
		mulps xmm0, xmm3 ; Multiply with kernel

		; One pixel to the left
		pmovzxbd xmm2, [rsi + rcx - 3]
		cvtdq2ps xmm2, xmm2
		mulps xmm2, xmm4

		; Leftmost pixel
		pmovzxbd xmm1, [rsi + rcx - 6]
		cvtdq2ps xmm1, xmm1
		mulps xmm1, xmm5

		; Add left pixels
		addps xmm1, xmm2
		addps xmm0, xmm1

		; One pixel to the right
		pmovzxbd xmm1, [rsi + rcx + 3]
		cvtdq2ps xmm1, xmm1
		mulps xmm1, xmm4

		; Rightmost pixel
		pmovzxbd xmm2, [rsi + rcx + 6]
		cvtdq2ps xmm2, xmm2
		mulps xmm2, xmm5

		; Add right pixels
		addps xmm1, xmm2
		addps xmm0, xmm1
		cvtps2dq xmm0, xmm0 ; Convert back to dwords

		;--- Extract colors ----;
		xor rax, rax
	
	color1: ; Extract color 1 as dword
		pextrd eax, xmm0, 0
		
		; Ensure that the color value is in the byte range, saturate otherwise
		cmp eax, 255 ; Check if high
		jg high1
		cmp eax, 0 ; Check if low
		jl low1
		mov [rdi + rcx], al ; Value OK, move lowest 8 bits
		jmp color2
	high1: ; Value too high, use 255
		mov byte ptr [rdi + rcx], 255
		jmp color2
	low1: ; Value too high, use 0
		mov byte ptr [rdi + rcx], 0

	color2: ; Extract color 2 in the same way as color 1
		pextrd eax, xmm0, 1
		
		; Check value
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

	color3: ; Extract color 3 as before
		pextrd eax, xmm0, 2
		
		; Check value
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
		div r11 ; i / byte_width
		
		cmp edx, 0 ; if (remainder == 0)
		jne while_loop
		add rcx, r13 ; i += padding
		jmp while_loop

return:
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

	;---- Arguments ----;
	; RCX - start index (int64)
	; RDX - end index (int64)
	;-------------------;

	;---- Registers ----;
	; R8 - end index (end)
	; RDI - data array pointer
	; RSI - helper array pointer

	; R10 - image stride
	; R11 - image byte width
	; R12 - image byte width decreased by 6
	; R13 - padding
	; R14 - image height decreased by 2

	; Main loop:
		; RCX - loop counter (i)
		; RDX - byte index in the current row (x)
		; R9 - current row (y)

	; XMM registers:
		; XMM0 - center pixel data and convolution sum
		; XMM1 - umost or downmost pixel data
		; XMM2 - data for one pixel up or down of the center
		
		; XMM3 - kernel data for the center pixel
		; XMM4 - kernel data for the up and down pixels
		; XMM5 - kernel data for the upmost and downmost pixels
	;-------------------;

	mov r8, rdx ; Store end index
	mov rdi, data_array ; Store data pointer
	mov rsi, helper_array ; Store helper pointer

	mov r10, stride ; Store image stride
	mov r11, byte_width ; Store byte width
	mov r12, byte_width_6 ; Store byte_width - 6
	mov r13, padding ; Store padding
	mov r14, height_2 ; Store height - 2

	; Store kernel
	mov rbx, kernel_array
	movdqu xmm3, xmmword ptr [rbx + 6 * 4]
	movdqu xmm4, xmmword ptr [rbx + 3 * 4]
	movdqu xmm5, xmmword ptr [rbx]

	; Store permutation mask
	vmovdqu ymm6, ymmword ptr [perm_mask_x]

	while_loop: ; i < end
		cmp rcx, r8
		jge return

		; y = i / stride
		; x = i % stride
		mov rax, rcx ; Move loop counter to RAX
		xor rdx, rdx ; Clear RDX
		div r10 ; Divide by stride
		mov r9, rax ; Store result (y) in R9

		; if (x >= 6 && x <= byte_width - 6 && y > 1 && y < imageHeight - 2)
		cmp rdx, 6 ; if (x >= 6)
		jl end_while
		cmp rdx, r12 ; if (x <= byte_width - 6)
		jg end_while
		cmp r9, 1 ; if (y > 1)
		jle end_while
		cmp r9, r14 ; if (y < imageHeight - 2)
		jge return

		;---- Calculate ----;
		pmovzxbd xmm0, [rsi + rcx] ; Move pixel data to XMM, ignore byte 3
		cvtdq2ps xmm0, xmm0 ; Convert to floats
		mulps xmm0, xmm3 ; Multiply by kernel data

		; Top pixels
		mov rax, rcx 
		sub rax, r10 ; Calculate i - stride to find pixel to the top
		
		; Multiply the pixel data
		pmovzxbd xmm1, [rsi + rax]
		cvtdq2ps xmm1, xmm1
		mulps xmm1, xmm4
		
		sub rax, r10 ; Calculate i - 2 * stride to find pixel two rows up
		
		; Multiply the pixel data
		pmovzxbd xmm2, [rsi + rax]
		cvtdq2ps xmm2, xmm2
		mulps xmm2, xmm5

		; Add top pixels to the sum
		addps xmm1, xmm2
		addps xmm0, xmm1

		; Bottom pixels
		mov rax, rcx 
		add rax, r10 ; Calculate i + stride to find one pixel down

		; Multiply the pixel data
		pmovzxbd xmm1, [rsi + rax]
		cvtdq2ps xmm1, xmm1
		mulps xmm1, xmm4
		
		add rax, r10 ; Calculate i + 2 * stride to find pixel two rows down
		
		; Multiply the pixel data
		pmovzxbd xmm2, [rsi + rax]
		cvtdq2ps xmm2, xmm2
		mulps xmm2, xmm5

		; Add bottom pixels to the sum
		addps xmm1, xmm2
		addps xmm0, xmm1
		cvtps2dq xmm0, xmm0 ; Convert to dwords

		;--- Extract colors ----;
		xor rax, rax
	
	color1: ; Extract color 1 as dword
		pextrd eax, xmm0, 0
		
		; Ensure that the color value is in the byte range, saturate otherwise
		cmp eax, 255 ; Check if high
		jg high1
		cmp eax, 0 ; Check if low
		jl low1
		mov [rdi + rcx], al ; Value OK, move lowest 8 bits
		jmp color2
	high1: ; Value too high, use 255
		mov byte ptr [rdi + rcx], 255
		jmp color2
	low1: ; Value too high, use 0
		mov byte ptr [rdi + rcx], 0

	color2: ; Extract color 2 in the same way as color 1
		pextrd eax, xmm0, 1
		
		; Check value
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

	color3: ; Extract color 3 as before
		pextrd eax, xmm0, 2
		
		; Check value
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
		div r12 ; i / byte_width
		
		cmp edx, 0 ; if (remainder == 0)
		jne while_loop
		add rcx, r13 ; i += padding
		jmp while_loop

return:
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

BlurX_YMM proc ; TODO
	push rbx
	push rsi
	push rdi
	push r10
	push r11
	push r12
	push r13
	push r14

	mov r8, rdx ; Store end index
	mov rsi, data_array ; Store data pointer
	mov rdi, helper_array ; Store helper pointer

	mov r10, stride ; Store image stride
	mov r11, byte_width ; Store byte width
	mov r12, byte_width_6 ; Store byte_width - 6
	mov r13, padding ; Store padding
	mov r14, height_2 ; Store height - 2

	; Store kernel
	mov rbx, kernel_array
	vmovdqu ymm4, ymmword ptr [rbx]
	movdqu xmm3, xmmword ptr [rbx + 6 * 4]
	vmovdqu ymm5, ymmword ptr [rbx + 9 * 4]

	; Store permutation mask
	vmovdqu ymm6, ymmword ptr [perm_mask_x]

	while_loop: ; i < end
		cmp rcx, r8
		jge return

		; y = i / stride
		; x = i % stride
		mov rax, rcx ; Move loop counter to RAX
		xor rdx, rdx ; Clear RDX
		div r10 ; Divide by stride
		mov r9, rax ; Store result (y) in R9

		; if (x >= 6 && x <= byte_width - 6 && y > 1 && y < imageHeight - 2)
		cmp rdx, 6 ; if (x >= 6)
		jl end_while
		cmp rdx, r12 ; if (x <= byte_width - 6)
		jg end_while
		cmp r9, 1 ; if (y > 1)
		jle end_while
		cmp r9, r14 ; if (y < imageHeight - 2)
		jge return

		;---- Calculate ----;
		pmovzxbd xmm0, [rsi + rcx] ; Move center pixel data
		cvtdq2ps xmm0, xmm0 ; Convert to floats
		mulps xmm0, xmm3 ; Multiply by the kernel values

		vpmovzxbd ymm1, qword ptr [rsi + rcx - 6] ; Move first 8 bytes containing two pixels to the left
		vcvtdq2ps ymm1, ymm1
		vmulps ymm1, ymm1, ymm4
		
		vpmovzxbd ymm2, qword ptr [rsi + rcx + 3] ; Move last 8 bytes containing two pixels to the right
		vcvtdq2ps ymm2, ymm2
		vmulps ymm2, ymm2, ymm5

		vaddps ymm1, ymm1, ymm2 ; Add values from YMM registers
		addps xmm0, xmm1 ; Add lower 4 values from YMM to the XMM
		vpermd ymm1, ymm6, ymm1 ; Permute: place values 3, 4, 5 in places 0, 1, 2
		addps xmm0, xmm1 ; Add the permuted values
		cvtps2dq xmm0, xmm0 ; Convert back to dwords

		;--- Extract colors ----;
		xor rax, rax
	
	color1: ; Extract color 1
		pextrd eax, xmm0, 0
		
		; Ensure that the color value is in the byte range, saturate otherwise
		cmp eax, 255 ; Check if high
		jg high1
		cmp eax, 0 ; Check if low
		jl low1
		mov [rdi + rcx], al ; Value OK
		jmp color2
	high1: ; Value too high, use 255
		mov byte ptr [rdi + rcx], 255
		jmp color2
	low1: ; Value too high, use 0
		mov byte ptr [rdi + rcx], 0

	color2: ; Extract color 2 in the same way as color 1
		pextrd eax, xmm0, 1
		
		; Check value
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

	color3: ; Extract color 3 as before
		pextrd eax, xmm0, 2
		
		; Check value
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
		div r11 ; i / byte_width
		
		cmp edx, 0 ; if (remainder == 0)
		jne while_loop
		add rcx, r13 ; i += padding
		jmp while_loop

return:
	pop r14
	pop r13
	pop r12
	pop r11	
	pop r10
	pop rdi
	pop rsi
	pop rbx
	ret

BlurX_YMM endp

end