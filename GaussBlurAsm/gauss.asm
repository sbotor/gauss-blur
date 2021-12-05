.DATA
data_array QWORD 0
helper_array QWORD 0
img_stride QWORD 0
img_height QWORD 0
img_size QWORD 0
kernel_array QWORD 0

.CODE

Init PROC
	MOV data_array, RCX
	MOV helper_array, RDX
	MOV img_stride, R8
	MOV img_height, R9
	MOV RCX, [RSP+40]
	MOV kernel_array, RCX

	; Calculate size
	MOV RAX, img_stride
	MUL img_height
	MOV img_size, RAX

	RET
Init ENDP

BlurX PROC
	LOCAL start_pos : QWORD,
	end_pos : QWORD,
	start_row : QWORD,
	end_row : QWORD,
	colors[3] : DWORD,
	ret_colors[3] : BYTE

	.CODE
	; Setup
	MOV start_pos, RCX
	MOV end_pos, RDX
	
	MOV RAX, start_pos
	XOR RDX, RDX
	DIV img_stride
	MOV start_row, RAX

	MOV RAX, end_pos
	XOR RDX, RDX
	DIV img_stride
	MOV end_row, RAX

	; Outer loop
	MOV R10, start_pos ; Starting position in R10
	MOV R11, end_pos ; Ending position in R11
	MOV RCX, start_row ; Outer loop counter in RCX
OUTER_LOOP_X:
	MOV RAX, RCX
	MUL img_stride
	MOV R8, RAX ; Row offset in R8

		MOV RDX, 0
	INNER_LOOP_X:
		MOV R9, R8
		ADD R9, RDX ; Current pixel position in R9

		CMP R9, R10 ; Compare with start_pos
		JL INNER_LOOP_X_END
		CMP R9, R11 ; Compare with end_pos
		JGE INNER_LOOP_X_END

		PUSH RBX
		PUSH RCX
		PUSH RDX
		CALL BlurPixelX
		POP RDX
		POP RCX
		POP RBX

	INNER_LOOP_X_END:
		ADD RDX, 3
		CMP RDX, img_stride
		JL INNER_LOOP_X
		NOP

OUTER_LOOP_X_END:
	INC RCX
	CMP RCX, end_row
	JLE OUTER_LOOP_X
	NOP

	RET

BlurX ENDP

BlurPixelX PROC
	
	MOV RCX, data_array
	XOR RAX, RAX
	MOV AL, BYTE PTR [RCX + R9]
	
	XOR RBX, RBX
	MOV BL, BYTE PTR [RCX + R9 + 1]
	ADD RAX, RBX
	
	XOR RBX, RBX
	MOV BL, BYTE PTR [RCX + R9 + 2]
	ADD RAX, RBX

	XOR RDX, RDX
	MOV BL, 3
	DIV BL

	MOV BYTE PTR [RCX + R9], AL
	MOV BYTE PTR [RCX + R9 + 1], AL
	MOV BYTE PTR [RCX + R9 + 2], AL

	RET

BlurPixelX ENDP

BlurY PROC
	LOCAL start_pos : QWORD,
	end_pos : QWORD,
	start_row : QWORD,
	end_row : QWORD,
	colors[4] : DWORD

	.CODE
	; Setup
	MOV start_pos, RCX
	MOV end_pos, RDX
	
	MOV RCX, start_pos
OUTER_LOOP_Y:
	MOV RAX, QWORD PTR [helper_array]
	MOV QWORD PTR [data_array], RAX
	
OUTER_LOOP_Y_END:
	INC RCX
	CMP RCX, end_pos
	JLE OUTER_LOOP_Y

	RET
	
BlurY ENDP

END