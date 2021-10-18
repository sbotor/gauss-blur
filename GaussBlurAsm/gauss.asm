.CODE

sanityTest PROC
.DATA
testChar DB 'A'

.CODE
XOR RAX, RAX
MOV AL, testChar
ret

sanityTest ENDP

;;;

testSIMD PROC

MOVUPS XMM0, [RCX]
MULPS XMM0, [RDX]
MOVUPS [R8], XMM0
ret

testSIMD ENDP

END