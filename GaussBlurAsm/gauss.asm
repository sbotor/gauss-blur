.DATA
dataArray DQ 0
helperArray DQ 0
imgStride DQ 0
imgHeight DQ 0
kernelArray DQ 0

.CODE

sanityTest PROC
	.DATA
	testChar DB 'A'
	
	.CODE
	XOR RAX, RAX
	MOV AL, testChar
	
	ret
sanityTest ENDP

testSIMD PROC
	VMOVUPD YMM0, [RCX]
	VMOVUPD YMM1, [RDX]
	VMULPD YMM0, YMM0, YMM1
	VMOVUPD [RCX], YMM0
	
	ret
testSIMD ENDP

init PROC
	MOV dataArray, RCX
	MOV helperArray, RDX
	MOV imgStride, R8
	MOV imgHeight, R9
	MOV RCX, [RSP+40]
	MOV kernelArray, RCX

ret
init ENDP

testParams PROC
	.DATA
	startPos DQ 0
	endPos DQ 0
	
	.CODE
	MOV startPos, RCX
	MOV endPos, RDX

	; TEST
	MOV RCX, dataArray
	MOV RCX, helperArray
	MOV RCX, imgStride
	MOV RCX, imgHeight
	MOV RCX, kernelArray

	ret
testParams ENDP

END